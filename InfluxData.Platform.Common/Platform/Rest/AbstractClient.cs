using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Platform.Common.Flux.Domain;
using Platform.Common.Flux.Error;
using Platform.Common.Flux.Parser;

namespace Platform.Common.Platform.Rest
{
    public class AbstractClient
    {
        protected readonly DefaultClientIo Client;      

        public AbstractClient()
        {
            Client = new DefaultClientIo();
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

        private struct ErrorsWrapper
        {
            public readonly IReadOnlyList<string> Error;

            public ErrorsWrapper(IReadOnlyList<string> errors)
            {
                Error = errors;
            }
        }

        public static void RaiseForInfluxError(RequestResult resultRequest)
        {
            var statusCode = resultRequest.StatusCode;

            if (statusCode >= 200 && statusCode < 300)
            {
                return;
            }

            var wrapper = new ErrorsWrapper(InfluxException.GetErrorMessage(resultRequest).ToList().AsReadOnly());
            
            var response = new QueryErrorResponse(statusCode, wrapper.Error);

            if (wrapper.Error != null)
            {
                throw new InfluxException(response);
            }

            throw new HttpException(response);
        }
    }
}