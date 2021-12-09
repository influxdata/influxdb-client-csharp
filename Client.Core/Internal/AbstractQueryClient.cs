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
    public abstract class AbstractQueryClient: AbstractRestClient
    {
        protected static readonly Action EmptyAction = () => { };

        protected static readonly Action<Exception> ErrorConsumer = e => throw e;

        private readonly FluxCsvParser _csvParser = new FluxCsvParser();

        protected readonly ApiClient ApiClient;
        protected readonly IFluxResultMapper Mapper;
        protected readonly ExceptionFactory ExceptionFactory;

        protected AbstractQueryClient(ApiClient apiClient, ExceptionFactory exceptionFactory, IFluxResultMapper mapper)
        {
            Arguments.CheckNotNull(apiClient, nameof(apiClient));
            Arguments.CheckNotNull(mapper, nameof(mapper));
            Arguments.CheckNotNull(exceptionFactory, nameof(exceptionFactory));

            ApiClient = apiClient;
            Mapper = mapper;
            ExceptionFactory = exceptionFactory;
        }

        protected Task Query(RequestOptions query, FluxCsvParser.IFluxResponseConsumer responseConsumer,
            Action<Exception> onError,
            Action onComplete, CancellationToken cancellationToken)
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

            return Query(query, Consumer, onError, onComplete, cancellationToken);
        }

        protected Task QueryRaw(RequestOptions query,
            Action<CancellationToken, string> onResponse,
            Action<Exception> onError,
            Action onComplete, CancellationToken cancellationToken)
        {
            void Consumer(CancellationToken cancellable, Stream bufferedStream)
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

        private async Task Query(RequestOptions query, Action<CancellationToken, Stream> consumer,
            Action<Exception> onError, Action onComplete, CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(query, "query");
            Arguments.CheckNotNull(consumer, "consumer");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            try
            {
                var cancellable = new DefaultCancellable();

                var restResponse = await ApiClient
                    .PostAsync<Stream>("/api/v2/query", query, ApiClient.Configuration, cancellationToken)
                    .ConfigureAwait(false);
               
                // check success
                var exception = ExceptionFactory("PostQuery", restResponse);
                if (exception != null) throw exception;
                
                consumer(cancellationToken, restResponse.Data);
                
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

        private void QuerySync(RequestOptions query, Action<CancellationToken, Stream> consumer,
            Action<Exception> onError, Action onComplete, CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(query, "query");
            Arguments.CheckNotNull(consumer, "consumer");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");
            try
            {
                var cancellable = new DefaultCancellable();

                var restResponse = ApiClient.Post<Stream>("/api/v2/query", query, ApiClient.Configuration);

                // check success
                var exception = ExceptionFactory("PostQuery", restResponse);
                if (exception != null) throw exception;
                
                consumer(cancellationToken, restResponse.Data);
                
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

        protected async IAsyncEnumerable<T> QueryEnumerable<T>(RequestOptions query, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(query, nameof(query));

            var restResponse = await ApiClient
                .PostAsync<Stream>("/api/v2/query", query, ApiClient.Configuration, cancellationToken)
                .ConfigureAwait(false);

            // check success
            var exception = ExceptionFactory("PostQuery", restResponse);
            if (exception != null) throw exception;

            await foreach(var (_, record) in _csvParser.ParseFluxResponseAsync(new StreamReader(restResponse.Data), cancellationToken).ConfigureAwait(false))
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
            private readonly Action<CancellationToken, object> _onNext;
            private readonly IFluxResultMapper _converter;
            private readonly Type _type;

            public FluxResponseConsumerPoco(Action<CancellationToken, object> onNext, IFluxResultMapper converter, Type type)
            {
                _onNext = onNext;
                _converter = converter;
                _type = type;
            }

            public void Accept(int index, CancellationToken cancellable, FluxTable table)
            {
            }

            public void Accept(int index, CancellationToken cancellable, FluxRecord record)
            {
                _onNext(cancellable, _converter.ConvertToEntity(record,_type));
            }
        }

        public class FluxResponseConsumerPoco<T> : FluxCsvParser.IFluxResponseConsumer
        {
            private readonly Action<CancellationToken, T> _onNext;
            private readonly IFluxResultMapper _converter;

            public FluxResponseConsumerPoco(Action<CancellationToken, T> onNext, IFluxResultMapper converter)
            {
                _onNext = onNext;
                _converter = converter;
            }

            public void Accept(int index, CancellationToken cancellable, FluxTable table)
            {
            }

            public void Accept(int index, CancellationToken cancellable, FluxRecord record)
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

        protected class FluxResponseConsumerRecord : FluxCsvParser.IFluxResponseConsumer
        {
            private readonly Action<CancellationToken, FluxRecord> _onNext;

            public FluxResponseConsumerRecord(Action<CancellationToken, FluxRecord> onNext)
            {
                _onNext = onNext;
            }

            public void Accept(int index, CancellationToken cancellable, FluxTable table)
            {
            }

            public void Accept(int index, CancellationToken cancellable, FluxRecord record)
            {
                _onNext(cancellable, record);
            }
        }
    }
}