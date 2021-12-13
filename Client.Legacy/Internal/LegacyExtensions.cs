using System;
using InfluxDB.Client.Core.Api;
using InfluxDB.Client.Core.Internal;

namespace InfluxDB.Client.Flux.Internal
{
    public static class FluxConnectionOptionsExtensions
    {
        internal static ApiClient ToApiClient(this FluxConnectionOptions options, LoggingHandler loggingHandler)
        {
            return new ApiClient(options.Url, new char[] { }, options.Username, options.Password,
                ApiClient.AuthenticationType.None, options.Timeout, false, true,
                options.WebProxy, loggingHandler, null);
        }
    }
}