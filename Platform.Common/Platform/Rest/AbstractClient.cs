using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Platform.Common.Flux.Csv;
using Platform.Common.Flux.Domain;
using Platform.Common.Flux.Error;

namespace Platform.Common.Platform.Rest
{
    public class AbstractClient
    {
        protected readonly DefaultClientIo Client;
        
        protected static Action EmptyAction = () => 
        {
            
        };
        
        protected static Action<Exception> ErrorConsumer = e => { throw e; };

        public AbstractClient(DefaultClientIo client)
        {
            Client = client;
        }

        private FluxCsvParser _csvParser = new FluxCsvParser(); 
        
        public async Task Query(HttpRequestMessage query, 
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
        
        public async Task QueryRaw(HttpRequestMessage query,
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

            try
            {
                DefaultCancellable cancellable = new DefaultCancellable();
                
                var responseHttp = await Client.DoRequest(query).ConfigureAwait(false);

                RaiseForInfluxError(responseHttp);

                consumer(cancellable, responseHttp.ResponseContent);

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

        protected void ParseFluxResponseToLines(Action<String> onResponse,
                        ICancellable cancellable,
                        BufferedStream bufferedStream)
        {
            using (StreamReader sr = new StreamReader(bufferedStream))
            {
                string line;

                while ((line = sr.ReadLine()) != null && !cancellable.IsCancelled())
                {
                    onResponse(line);
                }
            }
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
                Console.WriteLine("Socket closed by remote server or end of data");
                Console.WriteLine(exception);
            } 
            else 
            {
                onError(exception);
            }
        }
        
        protected bool IsCloseException(Exception exception) 
        {
            Arguments.CheckNotNull(exception, "exception");

            return exception is EndOfStreamException;
        }
        
        public class FluxResponseConsumerRecord : FluxCsvParser.IFluxResponseConsumer
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

        #pragma warning disable
        private struct ErrorsWrapper
        {
            public readonly IReadOnlyList<string> Errors;
        }
        #pragma warning enable
        
        public static void RaiseForInfluxError(RequestResult resultRequest)
        {
            var statusCode = resultRequest.StatusCode;

            if (statusCode >= 200 && statusCode < 300)
            {
                return;
            }

            StreamReader reader = new StreamReader(resultRequest.ResponseContent);
            string responseString = reader.ReadToEnd();

            var wrapper = resultRequest.ResponseContent.Length > 1
                            ? JsonConvert.DeserializeObject<ErrorsWrapper>(responseString)
                            : new ErrorsWrapper();

            var response = new QueryErrorResponse(statusCode, wrapper.Errors);

            var message = InfluxException.GetErrorMessage(resultRequest);

            if (message != null)
            {
                throw new InfluxException(response);
            }

            throw new HttpException(response);
        }
        
        public static string GetDefaultDialect()
        {
            JObject json = new JObject();
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

            JObject json = new JObject();
            json.Add("query", query);

            if (dialect != null)
            {
                json.Add("dialect", JObject.Parse(dialect));
            }

            return json.ToString();
        }
    }
}