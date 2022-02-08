using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
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
    public abstract class AbstractQueryClient: AbstractRestClient
    {
        protected static readonly Action EmptyAction = () => { };

        protected static readonly Action<Exception> ErrorConsumer = e => throw e;

        private readonly FluxCsvParser _csvParser = new FluxCsvParser();

        protected RestClient RestClient;
        protected readonly IFluxResultMapper Mapper;

        protected AbstractQueryClient(IFluxResultMapper mapper)
        {
            Arguments.CheckNotNull(mapper, nameof(mapper));

            Mapper = mapper;
        }

        protected Task Query(Func<Func<HttpResponseMessage, RestResponse>, RestRequest> queryFn, FluxCsvParser.IFluxResponseConsumer responseConsumer,
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

            return Query(queryFn, Consumer, onError, onComplete);
        }

        protected Task QueryRaw(Func<Func<HttpResponseMessage, RestResponse>, RestRequest> queryFn,
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

            return Query(queryFn, Consumer, onError, onComplete);
        }
        
        protected void QuerySync(Func<Func<HttpResponseMessage, RestResponse>, RestRequest> queryFn, FluxCsvParser.IFluxResponseConsumer responseConsumer,
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

            QuerySync(queryFn, Consumer, onError, onComplete);
        }

        private async Task Query(Func<Func<HttpResponseMessage, RestResponse>, RestRequest> queryFn, Action<ICancellable, Stream> consumer,
            Action<Exception> onError, Action onComplete)
        {
            Arguments.CheckNotNull(queryFn, "queryFn");
            Arguments.CheckNotNull(consumer, "consumer");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            try
            {
                var cancellable = new DefaultCancellable();

                var query = queryFn.Invoke(response =>
                {
                    var result = response.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    result = AfterIntercept((int)response.StatusCode, () => response.Headers.ToHeaderParameters(), result);

                    RaiseForInfluxError(response, result);
                    consumer(cancellable, result);

                    return FromHttpResponseMessage(response);
                });

                BeforeIntercept(query);
                    
                var restResponse = await RestClient.ExecuteAsync(query, Method.Post).ConfigureAwait(false);
                if (restResponse.ErrorException != null)
                {
                    throw restResponse.ErrorException;
                }
                
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

        private void QuerySync(Func<Func<HttpResponseMessage, RestResponse>, RestRequest> queryFn, Action<ICancellable, Stream> consumer,
            Action<Exception> onError, Action onComplete)
        {
            Arguments.CheckNotNull(queryFn, "queryFn");
            Arguments.CheckNotNull(consumer, "consumer");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            try
            {
                var cancellable = new DefaultCancellable();
                
                var query = queryFn.Invoke(response =>
                {
                    var result = response.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    result = AfterIntercept((int)response.StatusCode, () => response.Headers.ToHeaderParameters(), result);

                    RaiseForInfluxError(response, result);
                    consumer(cancellable, result);

                    return FromHttpResponseMessage(response);
                });

                BeforeIntercept(query);
                    
                var restResponse = RestClient.ExecuteAsync(query, Method.Post).ConfigureAwait(false).GetAwaiter().GetResult();
                if (restResponse.ErrorException != null)
                {
                    throw restResponse.ErrorException;
                }
                
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

        protected async IAsyncEnumerable<T> QueryEnumerable<T>(Func<Func<HttpResponseMessage, RestResponse>, RestRequest> queryFn, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(queryFn, nameof(queryFn));

            var query = queryFn.Invoke(null);
            BeforeIntercept(query);

            var response = await RestClient.ExecuteAsync(query, cancellationToken).ConfigureAwait(false);

            AfterIntercept((int)response.StatusCode, () => response.Headers, response.Content);

            RaiseForInfluxError(response, response.Content);

            //TODO ??? response.stream
            await foreach(var (_, record) in _csvParser.ParseFluxResponseAsync(new StringReader(response.Content), cancellationToken).ConfigureAwait(false))
            {
                if (!(record is null))
                    yield return Mapper.ConvertToEntity<T>(record);
            }
        }

        protected abstract void BeforeIntercept(RestRequest query);

        protected abstract T AfterIntercept<T>(int statusCode, Func<IEnumerable<HeaderParameter>> headers, T body);

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
        
        public class FluxResponseConsumerPoco : FluxCsvParser.IFluxResponseConsumer
        {
            private readonly Action<ICancellable, object> _onNext;
            private readonly IFluxResultMapper _converter;
            private readonly Type _type;

            public FluxResponseConsumerPoco(Action<ICancellable, object> onNext, IFluxResultMapper converter, Type type)
            {
                _onNext = onNext;
                _converter = converter;
                _type = type;
            }

            public void Accept(int index, ICancellable cancellable, FluxTable table)
            {
            }

            public void Accept(int index, ICancellable cancellable, FluxRecord record)
            {
                _onNext(cancellable, _converter.ConvertToEntity(record,_type));
            }
        }

        public class FluxResponseConsumerPoco<T> : FluxCsvParser.IFluxResponseConsumer
        {
            private readonly Action<ICancellable, T> _onNext;
            private readonly IFluxResultMapper _converter;

            public FluxResponseConsumerPoco(Action<ICancellable, T> onNext, IFluxResultMapper converter)
            {
                _onNext = onNext;
                _converter = converter;
            }

            public void Accept(int index, ICancellable cancellable, FluxTable table)
            {
            }

            public void Accept(int index, ICancellable cancellable, FluxRecord record)
            {
                _onNext(cancellable, _converter.ConvertToEntity<T>(record));
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
            if (result is RestResponse restResponse)
            {
                if (restResponse.IsSuccessful) return;

                if (restResponse.ErrorException is InfluxException)
                {
                    throw restResponse.ErrorException;
                }

                throw HttpException.Create(restResponse, body);
            }

            var httpResponse = (HttpResponseMessage) result;
            if ((int) httpResponse.StatusCode >= 200 && (int) httpResponse.StatusCode < 300)
            {
                return;
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

        private RestResponse FromHttpResponseMessage(HttpResponseMessage response)
        {
            var uri = response.RequestMessage!.RequestUri;
            return new RestResponse
            {
                // Content = null,
                // RawBytes = null,
                // ContentEncoding = response.Content.Headers.ContentEncoding,
                // Version = response.RequestMessage?.Version,
                // ContentLength = response.Content.Headers.ContentLength,
                // ContentType = response.Content.Headers.ContentType?.MediaType,
                // ResponseStatus = response.IsSuccessStatusCode ? ResponseStatus.Completed : ResponseStatus.Error,
                ErrorException = response.IsSuccessStatusCode
                ? null
                : new HttpRequestException($"Request failed with status code {response.StatusCode}"),
                // ResponseUri = uri,
                // Server = response.Headers.Server.ToString(),
                // StatusCode = response.StatusCode,
                // StatusDescription = response.ReasonPhrase,
                // IsSuccessful = response.IsSuccessStatusCode,
                // Request = null,
                // Headers = response.Headers.ToHeaderParameters().ToList(),
                // ContentHeaders = response.Content.Headers.ToHeaderParameters().ToList(),
                // Cookies = RestClient.CookieContainer.GetCookies(uri),
                // RootElement = null
            };
        }
    }
}