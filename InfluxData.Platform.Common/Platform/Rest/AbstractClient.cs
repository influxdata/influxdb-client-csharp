using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Platform.Common.Flux.Domain;
using Platform.Common.Flux.Error;
using Platform.Common.Flux.Parser;

namespace Platform.Common.Platform.Rest
{
    public class AbstractClient
    {
        protected readonly DefaultClientIo Client;

        protected AbstractClient()
        {
            Client = new DefaultClientIo();
        }

        protected AbstractClient(DefaultClientIo client)
        {
            Client = client;
        }

        protected ConfiguredTaskAwaitable<RequestResult> Post(object body, string path)
        {
            var request = new HttpRequestMessage(new HttpMethod(HttpMethodKind.Post.Name()), path)
            {
                Content = CreateBody(body)
            };

            return Client.DoRequest(request).ConfigureAwait(false);
        }
        protected ConfiguredTaskAwaitable<RequestResult> Patch(object body, string path)
        {
            var request = new HttpRequestMessage(new HttpMethod(HttpMethodKind.Patch.Name()), path)
            {
                Content = CreateBody(body)
            };

            return Client.DoRequest(request).ConfigureAwait(false);
        }
        
        protected ConfiguredTaskAwaitable<RequestResult> Delete(string path)
        {
            var request = new HttpRequestMessage(new HttpMethod(HttpMethodKind.Delete.Name()), path);

            return Client.DoRequest(request).ConfigureAwait(false);
        }
        
        protected ConfiguredTaskAwaitable<RequestResult> Get(string path)
        {
            var request = new HttpRequestMessage(new HttpMethod(HttpMethodKind.Get.Name()), path);

            return Client.DoRequest(request).ConfigureAwait(false);
        }
        
        protected T Call<T>(RequestResult result, string nullError = null)
        {
            Arguments.CheckNotNull(result, "RequestResult");
            
            RaiseForInfluxError(result, nullError);

            var readToEnd = new StreamReader(result.ResponseContent).ReadToEnd();

            return JsonConvert.DeserializeObject<T>(readToEnd);
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

        private bool IsCloseException(Exception exception) 
        {
            Arguments.CheckNotNull(exception, "exception");

            return exception is EndOfStreamException;
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

        private struct ErrorsWrapper
        {
            public readonly IReadOnlyList<string> Error;

            public ErrorsWrapper(IReadOnlyList<string> errors)
            {
                Error = errors;
            }
        }

        protected static void RaiseForInfluxError(RequestResult resultRequest, string nullError = null)
        {
            var statusCode = resultRequest.StatusCode;

            if (statusCode >= 200 && statusCode < 300)
            {
                return;
            }

            var wrapper = new ErrorsWrapper(InfluxException.GetErrorMessage(resultRequest).ToList().AsReadOnly());
            
            if (nullError != null && wrapper.Error.Count > 0 && wrapper.Error[0].Equals(nullError))
            {
                Console.WriteLine("Error is considered as null response: {0}", wrapper);
                
                return;
            }
            
            var response = new QueryErrorResponse(statusCode, wrapper.Error);

            if (wrapper.Error != null)
            {
                throw new InfluxException(response);
            }

            throw new HttpException(response);
        }
        
        private StringContent CreateBody(object content)
        {
            var serializer = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            return new StringContent(JsonConvert.SerializeObject(content, Formatting.None, serializer));
        }
    }
}