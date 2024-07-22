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
using RestSharp.Interceptors;

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

        protected Task Query(RestRequest query,
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

            return Query(query, Consumer, onError, onComplete, cancellationToken);
        }

        protected Task QueryRaw(RestRequest query,
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

            return Query(query, Consumer, onError, onComplete, cancellationToken);
        }

        protected void QuerySync(RestRequest query,
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

            QuerySync(query, Consumer, onError, onComplete, cancellationToken);
        }

        private async Task Query(RestRequest query,
            Action<Stream> consumer,
            Action<Exception> onError, Action onComplete, CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(query, "query");
            Arguments.CheckNotNull(consumer, "consumer");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            try
            {
                query.AdvancedResponseWriter = (response, request) =>
                {
                    var result = GetStreamFromResponse(response, cancellationToken);
                    result = AfterIntercept((int)response.StatusCode,
                        () => response.Headers.ToHeaderParameters(response.Content.Headers),
                        result);

                    RaiseForInfluxError(response, result);
                    consumer(result);

                    return FromHttpResponseMessage(response, request);
                };

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

        private void QuerySync(RestRequest query,
            Action<CancellationToken, Stream> consumer,
            Action<Exception> onError, Action onComplete, CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(query, "query");
            Arguments.CheckNotNull(consumer, "consumer");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            try
            {
                query.AdvancedResponseWriter = (response, request) =>
                {
                    var result = GetStreamFromResponse(response, cancellationToken);
                    result = AfterIntercept((int)response.StatusCode,
                        () => response.Headers.ToHeaderParameters(response.Content.Headers),
                        result);

                    RaiseForInfluxError(response, result);
                    consumer(cancellationToken, result);

                    return FromHttpResponseMessage(response, request);
                };

                BeforeIntercept(query);

                var restResponse = RestClient.ExecuteSync(query, cancellationToken);
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
            RestRequest query,
            Func<FluxRecord, T> convert,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(query, nameof(query));

            query.Interceptors = new List<Interceptor>
            {
                new RequestBeforeAfterInterceptor<T>(
                    BeforeIntercept,
                    (statusCode, headers, body) => AfterIntercept(statusCode, headers, body)
                )
            };

            var stream = await RestClient.DownloadStreamAsync(query, cancellationToken).ConfigureAwait(false);
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
                Trace.WriteLine("Socket closed by remote server or end of data",
                    InfluxDBTraceFilter.CategoryInfluxQueryError);
                Trace.WriteLine(exception, InfluxDBTraceFilter.CategoryInfluxQueryError);
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

        private RestResponse FromHttpResponseMessage(HttpResponseMessage response, RestRequest request)
        {
            return new RestResponse(request)
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

    /// <summary>
    /// The interceptor that is called before and after the request.
    /// </summary>
    internal class RequestBeforeAfterInterceptor<T> : Interceptor
    {
        private readonly Action<RestRequest> _beforeRequest;
        private readonly Action<int, Func<IEnumerable<HeaderParameter>>, T> _afterRequest;

        /// <summary>
        /// Construct the interceptor.
        /// </summary>
        /// <param name="beforeRequest">Intercept request before HTTP call</param>
        /// <param name="afterRequest">Intercept response before parsing resutlts</param>
        internal RequestBeforeAfterInterceptor(
            Action<RestRequest> beforeRequest = null,
            Action<int, Func<IEnumerable<HeaderParameter>>, T> afterRequest = null)
        {
            _beforeRequest = beforeRequest;
            _afterRequest = afterRequest;
        }

        public override ValueTask BeforeRequest(RestRequest request, CancellationToken cancellationToken)
        {
            _beforeRequest?.Invoke(request);
            return base.BeforeRequest(request, cancellationToken);
        }

        public override ValueTask AfterHttpRequest(HttpResponseMessage responseMessage,
            CancellationToken cancellationToken)
        {
            _afterRequest?.Invoke(
                (int)responseMessage.StatusCode,
                () => responseMessage.Headers.ToHeaderParameters(responseMessage.Content.Headers),
                default);
            return base.AfterHttpRequest(responseMessage, cancellationToken);
        }
    }
}