using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
    public abstract class AbstractQueryClient : AbstractRestClient
    {
        protected static readonly Action EmptyAction = () => { };

        protected static readonly Action<Exception> ErrorConsumer = e => throw e;

        private readonly FluxCsvParser _csvParser;

        protected RestClient RestClient;
        protected readonly IFluxResultMapper Mapper;

        protected AbstractQueryClient(IFluxResultMapper mapper) : this(mapper, new FluxCsvParser())
        {
        }

        protected AbstractQueryClient(IFluxResultMapper mapper, FluxCsvParser csvParser)
        {
            Arguments.CheckNotNull(mapper, nameof(mapper));

            Mapper = mapper;
            _csvParser = csvParser;
        }

        protected Task Query(Func<Func<HttpResponseMessage, RestResponse>, RestRequest> queryFn,
            FluxCsvParser.IFluxResponseConsumer responseConsumer,
            Action<Exception> onError,
            Action onComplete, CancellationToken cancellationToken)
        {
            void Consumer(Stream bufferedStream)
            {
                try
                {
                    _csvParser.ParseFluxResponse(bufferedStream, cancellationToken, responseConsumer);
                }
                catch (IOException e)
                {
                    onError(e);
                }
            }

            return Query(queryFn, Consumer, onError, onComplete, cancellationToken);
        }

        protected Task QueryRaw(Func<Func<HttpResponseMessage, RestResponse>, RestRequest> queryFn,
            Action<string> onResponse,
            Action<Exception> onError,
            Action onComplete, CancellationToken cancellationToken)
        {
            void Consumer(Stream bufferedStream)
            {
                try
                {
                    ParseFluxResponseToLines(line => onResponse(line), cancellationToken, bufferedStream);
                }
                catch (IOException e)
                {
                    CatchOrPropagateException(e, onError);
                }
            }

            return Query(queryFn, Consumer, onError, onComplete, cancellationToken);
        }

        protected void QuerySync(Func<Func<HttpResponseMessage, RestResponse>, RestRequest> queryFn,
            FluxCsvParser.IFluxResponseConsumer responseConsumer,
            Action<Exception> onError,
            Action onComplete,
            CancellationToken cancellationToken)
        {
            void Consumer(CancellationToken cancellable, Stream bufferedStream)
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

            QuerySync(queryFn, Consumer, onError, onComplete, cancellationToken);
        }

        private async Task Query(Func<Func<HttpResponseMessage, RestResponse>, RestRequest> queryFn,
            Action<Stream> consumer,
            Action<Exception> onError, Action onComplete, CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(queryFn, "queryFn");
            Arguments.CheckNotNull(consumer, "consumer");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            try
            {
                var query = queryFn.Invoke(response =>
                {
                    var result = GetStreamFromResponse(response, cancellationToken);
                    result = AfterIntercept((int)response.StatusCode,
                        () => response.Headers.ToHeaderParameters(response.Content.Headers),
                        result);

                    RaiseForInfluxError(response, result);
                    consumer(result);

                    return FromHttpResponseMessage(response);
                });

                BeforeIntercept(query);

                var restResponse = await RestClient.ExecuteAsync(query, Method.Post, cancellationToken)
                    .ConfigureAwait(false);
                if (restResponse.ErrorException != null)
                {
                    throw restResponse.ErrorException;
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    onComplete();
                }
            }
            catch (Exception e)
            {
                onError(e);
            }
        }

        private void QuerySync(Func<Func<HttpResponseMessage, RestResponse>, RestRequest> queryFn,
            Action<CancellationToken, Stream> consumer,
            Action<Exception> onError, Action onComplete, CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(queryFn, "queryFn");
            Arguments.CheckNotNull(consumer, "consumer");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            try
            {
                var query = queryFn.Invoke(response =>
                {
                    var result = GetStreamFromResponse(response, cancellationToken);
                    result = AfterIntercept((int)response.StatusCode,
                        () => response.Headers.ToHeaderParameters(response.Content.Headers),
                        result);

                    RaiseForInfluxError(response, result);
                    consumer(cancellationToken, result);

                    return FromHttpResponseMessage(response);
                });

                BeforeIntercept(query);

                var restResponse = RestClient.ExecuteAsync(query, Method.Post, cancellationToken).ConfigureAwait(false)
                    .GetAwaiter().GetResult();
                if (restResponse.ErrorException != null)
                {
                    throw restResponse.ErrorException;
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    onComplete();
                }
            }
            catch (Exception e)
            {
                onError(e);
            }
        }

        protected async IAsyncEnumerable<T> QueryEnumerable<T>(
            Func<Func<HttpResponseMessage, RestResponse>, RestRequest> queryFn, Func<FluxRecord, T> convert,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(queryFn, nameof(queryFn));

            Stream stream = null;
            var query = queryFn.Invoke(response =>
            {
                stream = GetStreamFromResponse(response, cancellationToken);
                stream = AfterIntercept((int)response.StatusCode,
                    () => response.Headers.ToHeaderParameters(response.Content.Headers), stream);

                RaiseForInfluxError(response, stream);

                return FromHttpResponseMessage(response);
            });

            BeforeIntercept(query);

            var restResponse = await RestClient.ExecuteAsync(query, cancellationToken).ConfigureAwait(false);
            if (restResponse.ErrorException != null)
            {
                throw restResponse.ErrorException;
            }

            await foreach (var (_, record) in _csvParser
                               .ParseFluxResponseAsync(new StreamReader(stream), cancellationToken)
                               .ConfigureAwait(false))
                if (!(record is null))
                {
                    yield return convert.Invoke(record);
                }
        }

        protected abstract void BeforeIntercept(RestRequest query);

        protected abstract T AfterIntercept<T>(int statusCode, Func<IEnumerable<HeaderParameter>> headers, T body);

        protected void ParseFluxResponseToLines(Action<string> onResponse,
            CancellationToken cancellable,
            Stream bufferedStream)
        {
            using (var sr = new StreamReader(bufferedStream))
            {
                string line;

                while ((line = sr.ReadLine()) != null && !cancellable.IsCancellationRequested) onResponse(line);
            }
        }

        public class FluxResponseConsumerPoco : FluxCsvParser.IFluxResponseConsumer
        {
            private readonly Action<object> _onNext;
            private readonly IFluxResultMapper _converter;
            private readonly Type _type;

            public FluxResponseConsumerPoco(Action<object> onNext, IFluxResultMapper converter, Type type)
            {
                _onNext = onNext;
                _converter = converter;
                _type = type;
            }

            public void Accept(int index, FluxTable table)
            {
            }

            public void Accept(int index, FluxRecord record)
            {
                _onNext(_converter.ConvertToEntity(record, _type));
            }
        }

        public class FluxResponseConsumerPoco<T> : FluxCsvParser.IFluxResponseConsumer
        {
            private readonly Action<T> _onNext;
            private readonly IFluxResultMapper _converter;

            public FluxResponseConsumerPoco(Action<T> onNext, IFluxResultMapper converter)
            {
                _onNext = onNext;
                _converter = converter;
            }

            public void Accept(int index, FluxTable table)
            {
            }

            public void Accept(int index, FluxRecord record)
            {
                _onNext(_converter.ConvertToEntity<T>(record));
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
                if (restResponse.IsSuccessful)
                {
                    return;
                }

                if (restResponse.ErrorException is InfluxException)
                {
                    throw restResponse.ErrorException;
                }

                throw HttpException.Create(restResponse, body);
            }

            var httpResponse = (HttpResponseMessage)result;
            if ((int)httpResponse.StatusCode >= 200 && (int)httpResponse.StatusCode < 300)
            {
                return;
            }

            throw HttpException.Create(httpResponse, body);
        }

        protected class FluxResponseConsumerRecord : FluxCsvParser.IFluxResponseConsumer
        {
            private readonly Action<FluxRecord> _onNext;

            public FluxResponseConsumerRecord(Action<FluxRecord> onNext)
            {
                _onNext = onNext;
            }

            public void Accept(int index, FluxTable table)
            {
            }

            public void Accept(int index, FluxRecord record)
            {
                _onNext(record);
            }
        }

        private RestResponse FromHttpResponseMessage(HttpResponseMessage response)
        {
            return new RestResponse
            {
                ErrorException = response.IsSuccessStatusCode
                    ? null
                    : new HttpRequestException($"Request failed with status code {response.StatusCode}")
            };
        }

        private Stream GetStreamFromResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
#if NET5_0_OR_GREATER
            var readAsStreamAsync = response.Content.ReadAsStreamAsync(cancellationToken);
# else
            var readAsStreamAsync = response.Content.ReadAsStreamAsync();
#endif
            var streamFromResponse = readAsStreamAsync.ConfigureAwait(false).GetAwaiter().GetResult();
            if (response.Content.Headers.ContentEncoding.Any(x => "gzip".Equals(x, StringComparison.OrdinalIgnoreCase)))
            {
                streamFromResponse = new GZipStream(streamFromResponse, CompressionMode.Decompress);
            }

            return streamFromResponse;
        }
    }
}