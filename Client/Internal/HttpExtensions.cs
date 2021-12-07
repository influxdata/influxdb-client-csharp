using System;
using System.Net;
using InfluxDB.Client.Core.Api;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Internal;

namespace InfluxDB.Client.Internal
{
    internal static class HttpStatusCodeExtensions
    {
        internal static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
        {
            var status = (int)statusCode;
            return status >= 200 && status <= 299;
        }
    }

    internal static class HttpExceptionExtensions

    {
        internal static HttpException Create(IApiResponse response)
        {
            return HttpException.Create(response.Content, response.Headers, response.ErrorText, response.StatusCode);
        }
    }

    internal static class InfluxDBClientOptionsExtensions
    {
        internal static ApiClient ToApiClient(this InfluxDBClientOptions options, LoggingHandler loggingHandler, GzipHandler gzipHandle)
        {
            Enum.TryParse(options.AuthScheme.ToString(), true, out ApiClient.AuthenticationType authScheme);
            return new ApiClient(options.Url, options.Token, options.Username, options.Password,
                authScheme, options.Timeout, options.ReadWriteTimeout, options.AllowHttpRedirects, options.VerifySsl,
                options.WebProxy, loggingHandler, gzipHandle);
        }
    }
}