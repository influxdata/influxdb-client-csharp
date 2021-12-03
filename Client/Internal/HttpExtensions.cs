using System.Net;
using InfluxDB.Client.Api.Client;
using InfluxDB.Client.Core.Exceptions;

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
}