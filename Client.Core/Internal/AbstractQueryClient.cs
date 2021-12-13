using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Core.Api;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Internal;
using Newtonsoft.Json.Linq;

namespace InfluxDB.Client.Core.Internal
{
    public abstract class AbstractQueryClient : AbstractRestClient
    {
        internal static readonly Action EmptyAction = () => { };

        internal static readonly Action<Exception> ErrorConsumer = e => throw e;

        private readonly FluxCsvParser _csvParser = new FluxCsvParser();
        private readonly ApiClient _apiClient;
        private readonly ExceptionFactory _exceptionFactory;
        internal readonly IFluxResultMapper Mapper;

        protected AbstractQueryClient(ApiClient apiClient, ExceptionFactory exceptionFactory, IFluxResultMapper mapper)
        {
            Arguments.CheckNotNull(apiClient, nameof(apiClient));
            Arguments.CheckNotNull(mapper, nameof(mapper));
            Arguments.CheckNotNull(exceptionFactory, nameof(exceptionFactory));

            _apiClient = apiClient;
            _exceptionFactory = exceptionFactory;
            Mapper = mapper;
        }

        internal Task Query(RequestOptions query, FluxCsvParser.IFluxResponseConsumer responseConsumer,
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

        internal Task QueryRaw(RequestOptions query, Action<string> onResponse, Action<Exception> onError,
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

        protected void QuerySync(RequestOptions query, FluxCsvParser.IFluxResponseConsumer responseConsumer,
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

        private async Task Query(RequestOptions query, Action<Stream> consumer, Action<Exception> onError,
            Action onComplete, CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(query, "query");
            Arguments.CheckNotNull(consumer, "consumer");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            try
            {
                var restResponse = await _apiClient
                    .PostAsync<Stream>("/api/v2/query", query, _apiClient.Configuration, cancellationToken)
                    .ConfigureAwait(false);

                // check success
                var exception = _exceptionFactory("PostQuery", restResponse);
                if (exception != null) throw exception;

                consumer(restResponse.Data);

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

        private void QuerySync(RequestOptions query, Action<CancellationToken, Stream> consumer,
            Action<Exception> onError, Action onComplete, CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(query, "query");
            Arguments.CheckNotNull(consumer, "consumer");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");
            try
            {
                var restResponse = _apiClient.Post<Stream>("/api/v2/query", query, _apiClient.Configuration);

                // check success
                var exception = _exceptionFactory("PostQuery", restResponse);
                if (exception != null) throw exception;

                consumer(cancellationToken, restResponse.Data);

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

        protected async IAsyncEnumerable<T> QueryEnumerable<T>(RequestOptions query,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(query, nameof(query));

            var restResponse = await _apiClient
                .PostAsync<Stream>("/api/v2/query", query, _apiClient.Configuration, cancellationToken)
                .ConfigureAwait(false);

            // check success
            var exception = _exceptionFactory("PostQuery", restResponse);
            if (exception != null) throw exception;

            await foreach (var (_, record) in _csvParser
                .ParseFluxResponseAsync(new StreamReader(restResponse.Data), cancellationToken).ConfigureAwait(false))
            {
                if (!(record is null))
                    yield return Mapper.ConvertToEntity<T>(record);
            }
        }

        protected void ParseFluxResponseToLines(Action<String> onResponse,
            CancellationToken cancellable,
            Stream bufferedStream)
        {
            using (var sr = new StreamReader(bufferedStream))
            {
                string line;

                while ((line = sr.ReadLine()) != null && !cancellable.IsCancellationRequested)
                {
                    onResponse(line);
                }
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

        internal class FluxResponseConsumerRecord : FluxCsvParser.IFluxResponseConsumer
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
    }
}