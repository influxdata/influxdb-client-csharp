using System;
using InfluxDB.Client.Core.Api;
using InfluxDB.Client.Core.Internal;

namespace InfluxDB.Client.Internal
{
    internal static class InfluxDBClientOptionsExtensions
    {
        internal static ApiClient ToApiClient(this InfluxDBClientOptions options, LoggingHandler loggingHandler, GzipHandler gzipHandler)
        {
            Enum.TryParse(options.AuthScheme.ToString(), true, out ApiClient.AuthenticationType authScheme);
            var url = options.Url.EndsWith("/") ? options.Url.Remove(options.Url.Length - 1) : options.Url;
            
            return new ApiClient(url, options.Token, options.Username, options.Password,
                authScheme, options.Timeout, options.AllowHttpRedirects, options.VerifySsl,
                options.WebProxy, loggingHandler, gzipHandler);
        }
    }
}