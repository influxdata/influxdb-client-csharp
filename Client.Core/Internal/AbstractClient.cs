using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Internal;
using Newtonsoft.Json;
using NodaTime.Serialization.JsonNet;

namespace InfluxDB.Client.Core.Internal
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

        protected ConfiguredTaskAwaitable<RequestResult> Post(string path)
        {
            var request = new HttpRequestMessage(new HttpMethod(HttpMethodKind.Post.Name()), path);

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

        protected T Call<T>(RequestResult result, int? codeError, T defaultValue = default(T))
        {
            return Call<T>(result, null, codeError, defaultValue);
        }

        protected T Call<T>(RequestResult result, string nullError = null)
        {
            return Call<T>(result, nullError, null, default(T));
        }

        protected T Call<T>(RequestResult result, string nullError, int? codeError, T defaultValue)
        {
            Arguments.CheckNotNull(result, "RequestResult");

            var nullResponse = RaiseForInfluxError(result, nullError, codeError);
            if (nullResponse) return defaultValue;

            var readToEnd = new StreamReader(result.ResponseContent).ReadToEnd();


            var settings = new JsonSerializerSettings();
            settings.Converters.Add(NodaConverters.OffsetDateTimeConverter);
            settings.Converters.Add(NodaConverters.InstantConverter);
            settings.DateParseHandling = DateParseHandling.None;

            using (result)
            {
                return JsonConvert.DeserializeObject<T>(readToEnd, settings);
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

        protected static bool RaiseForInfluxError(RequestResult resultRequest, string nullError = null,
            int? codeError = null)
        {
            var statusCode = resultRequest.StatusCode;

            if (resultRequest.IsSuccessful()) return false;

            var exception = HttpException.Create(resultRequest);
            var errorMessage = exception.Message;

            //TODO remove https://github.com/influxdata/influxdb/issues/11589
            if (nullError != null && errorMessage != null &&
                (errorMessage.Equals(nullError) || exception.ErrorBody.ContainsKey("error") &&
                 exception.ErrorBody["error"].ToString().Equals(nullError)))
            {
                Trace.WriteLine($"Error is considered as null response: {errorMessage}");

                return true;
            }

            if (codeError != null && codeError.Value.Equals(statusCode))
            {
                Trace.WriteLine($"Error is considered as null response: {errorMessage}, {statusCode}");

                return true;
            }

            throw exception;
        }

        private StringContent CreateBody(object content)
        {
            var serializer = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            return new StringContent(JsonConvert.SerializeObject(content, Formatting.None, serializer));
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