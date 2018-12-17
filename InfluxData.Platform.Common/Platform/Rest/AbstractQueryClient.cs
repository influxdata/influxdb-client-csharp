using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Platform.Common.Flux.Domain;
using Platform.Common.Flux.Parser;

namespace Platform.Common.Platform.Rest
{
    public class AbstractQueryClient : AbstractClient
    {
        private static readonly FluxResultMapper Mapper = new FluxResultMapper();
        
        protected static readonly Action EmptyAction = () => 
        {
            
        };
        
        protected static readonly Action<Exception> ErrorConsumer = e => throw e;
        
        private readonly FluxCsvParser _csvParser = new FluxCsvParser();
        
        protected AbstractQueryClient()
        {
        }

        protected AbstractQueryClient(DefaultClientIo client) : base(client)
        {
        }


        protected async Task Query(HttpRequestMessage query, 
                        FluxCsvParser.IFluxResponseConsumer responseConsumer,
                        Action<Exception> onError, 
                        Action onComplete)
        {
            void Consumer(ICancellable cancellable, BufferedStream bufferedStream)
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

            await Query(query, Consumer, onError, onComplete);
        }

        protected async Task QueryRaw(HttpRequestMessage query,
                        Action<ICancellable, string> onResponse,
                        Action<Exception> onError, 
                        Action onComplete)
        {
            void Consumer(ICancellable cancellable, BufferedStream bufferedStream)
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

            await Query(query, Consumer, onError, onComplete);
        }

        protected async Task Query(HttpRequestMessage query, Action<ICancellable, BufferedStream> consumer,
                        Action<Exception> onError, Action onComplete)
        {
            Arguments.CheckNotNull(query, "query");
            Arguments.CheckNotNull(consumer, "consumer");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            RequestResult requestResult = null;
            try
            {
                var cancellable = new DefaultCancellable();

                requestResult = await Client.DoRequest(query).ConfigureAwait(false);

                RaiseForInfluxError(requestResult);

                consumer(cancellable, requestResult.ResponseContent);

                if (!cancellable.IsCancelled())
                {
                    onComplete();
                }
            }
            catch (Exception e)
            {
                onError(e);
            }
            finally
            {
                requestResult?.Dispose();
            }
        }

        protected void ParseFluxResponseToLines(Action<String> onResponse,
                        ICancellable cancellable,
                        BufferedStream bufferedStream)
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
    }
}