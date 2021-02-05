using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Internal;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace InfluxDB.Client.Core.Internal
{
    public abstract class AbstractQueryClient
    {
        private static readonly FluxResultMapper Mapper = new FluxResultMapper();

        protected static readonly Action EmptyAction = () => { };

        protected static readonly Action<Exception> ErrorConsumer = e => throw e;

        private readonly FluxCsvParser _csvParser = new FluxCsvParser();

        protected readonly RestClient RestClient;

        protected AbstractQueryClient(RestClient restClient)
        {
            Arguments.CheckNotNull(restClient, nameof(restClient));

            RestClient = restClient;
        }

        protected Task Query(RestRequest query, FluxCsvParser.IFluxResponseConsumer responseConsumer,
            Action<Exception> onError,
            Action onComplete)
        {
            void Consumer(ICancellable cancellable, Stream bufferedStream)
            {
                try
                {
                    _csvParser.ParseFluxResponse(bufferedStream, cancellable, responseConsumer);
                }
                catch (IOException e)
                {
                    onError(e);
                }
            }

            return Query(query, Consumer, onError, onComplete);
        }

        protected Task QueryRaw(RestRequest query,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete)
        {
            void Consumer(ICancellable cancellable, Stream bufferedStream)
            {
                try
                {
                    ParseFluxResponseToLines(line => onResponse(cancellable, line), cancellable, bufferedStream);
                }
                catch (IOException e)
                {
                    CatchOrPropagateException(e, onError);
                }
            }

            return Query(query, Consumer, onError, onComplete);
        }

        protected async Task Query(RestRequest query, Action<ICancellable, Stream> consumer,
            Action<Exception> onError, Action onComplete)
        {
            Arguments.CheckNotNull(query, "query");
            Arguments.CheckNotNull(consumer, "consumer");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            try
            {
                var cancellable = new DefaultCancellable();

                BeforeIntercept(query);
                    
                query.AdvancedResponseWriter = (responseStream, response) =>
                {
                    responseStream = AfterIntercept((int)response.StatusCode, () => response.Headers, responseStream);
                    
                    RaiseForInfluxError(response, responseStream);
                    consumer(cancellable, responseStream);
                };

                await Task.Run(() => { RestClient.DownloadData(query, true); }).ConfigureAwait(false);
                if (!cancellable.IsCancelled())
                {
                    onComplete();
                }
            }
            catch (Exception e)
            {
                onError(e);
            }
        }

        protected async IAsyncEnumerable<T> QueryEnumerable<T>(RestRequest query, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(query, nameof(query));

            BeforeIntercept(query);

            var response = await RestClient.ExecuteTaskAsync(query, cancellationToken).ConfigureAwait(false);

            response.Content = AfterIntercept((int)response.StatusCode, () => LoggingHandler.ToHeaders(response.Headers), response.Content);

            RaiseForInfluxError(response, response.Content);

            await foreach(var (_, record) in _csvParser.ParseFluxResponseAsync(new StringReader(response.Content), cancellationToken).ConfigureAwait(false))
            {
                if (!(record is null))
                    yield return Mapper.ToPoco<T>(record);
            }
        }

        protected abstract void BeforeIntercept(RestRequest query);

        protected abstract T AfterIntercept<T>(int statusCode, Func<IList<HttpHeader>> headers, T body);

        protected void ParseFluxResponseToLines(Action<String> onResponse,
            ICancellable cancellable,
            Stream bufferedStream)
        {
            using (var sr = new StreamReader(bufferedStream))
            {
                string line;

                while ((line = sr.ReadLine()) != null && !cancellable.IsCancelled())
                {
                    onResponse(line);
                }
            }
        }

        public class FluxResponseConsumerPoco<T> : FluxCsvParser.IFluxResponseConsumer
        {
            private readonly Action<ICancellable, T> _onNext;

            public FluxResponseConsumerPoco(Action<ICancellable, T> onNext)
            {
                _onNext = onNext;
            }

            public void Accept(int index, ICancellable cancellable, FluxTable table)
            {
            }

            public void Accept(int index, ICancellable cancellable, FluxRecord record)
            {
                _onNext(cancellable, Mapper.ToPoco<T>(record));
            }
        }

        public static string GetDefaultDialect()
        {
            var json = new JObject();
            json.Add("header", true);
            json.Add("delimiter", ",");
            json.Add("quoteChar", "\"");
            json.Add("commentPrefix", "#");
            json.Add("annotations", new JArray("datatype", "group", "default"));

            return json.ToString();
        }

        public static string CreateBody(string dialect, string query)
        {
            Arguments.CheckNonEmptyString(query, "Flux query");

            var json = new JObject();
            json.Add("query", query);

            if (!string.IsNullOrEmpty(dialect))
            {
                json.Add("dialect", JObject.Parse(dialect));
            }

            return json.ToString();
        }

        protected void CatchOrPropagateException(Exception exception,
            Action<Exception> onError)
        {
            Arguments.CheckNotNull(exception, "exception");
            Arguments.CheckNotNull(onError, "onError");

            //
            // Socket closed by remote server or end of data
            //
            if (IsCloseException(exception))
            {
                Trace.WriteLine("Socket closed by remote server or end of data");
                Trace.WriteLine(exception);
            }
            else
            {
                onError(exception);
            }
        }

        private bool IsCloseException(Exception exception)
        {
            Arguments.CheckNotNull(exception, "exception");

            return exception is EndOfStreamException;
        }

        protected void RaiseForInfluxError(object result, object body)
        {
            if (result is IRestResponse restResponse)
            {
                if (restResponse.IsSuccessful) return;

                if (restResponse.ErrorException is InfluxException)
                {
                    throw restResponse.ErrorException;
                }

                throw HttpException.Create(restResponse, body);
            }

            var httpResponse = (IHttpResponse) result;
            if ((int) httpResponse.StatusCode >= 200 && (int) httpResponse.StatusCode < 300)
            {
                return;
            }

            if (httpResponse.ErrorException is InfluxException)
            {
                throw httpResponse.ErrorException;
            }
            
            throw HttpException.Create(httpResponse, body);
        }

        protected class FluxResponseConsumerRecord : FluxCsvParser.IFluxResponseConsumer
        {
            private readonly Action<ICancellable, FluxRecord> _onNext;

            public FluxResponseConsumerRecord(Action<ICancellable, FluxRecord> onNext)
            {
                _onNext = onNext;
            }

            public void Accept(int index, ICancellable cancellable, FluxTable table)
            {
            }

            public void Accept(int index, ICancellable cancellable, FluxRecord record)
            {
                _onNext(cancellable, record);
            }
        }
    }
}