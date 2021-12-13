using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Core.Api;

namespace InfluxDB.Client.Core.Internal
{
    public abstract class AbstractRestClient
    {
        protected async Task<bool> PingAsync(Task<ApiResponse<object>> request,
            ExceptionFactory exceptionFactory)
        {
            try
            {
                var response = await request.ConfigureAwait(false);
                var exception = exceptionFactory("GetPing", response);
                if (exception != null) throw exception;

                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error: {e.Message}");
                return false;
            }
        }

        protected string VersionAsync(IDictionary<string, IList<string>> headers)
        {
            Arguments.CheckNotNull(headers, "headers");

            var value = headers
                .Where(header => header.Key.Equals("X-Influxdb-Version"))
                .Select(header => string.Join(", ", header.Value))
                .FirstOrDefault();

            if (value != null)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return "unknown";
        }
    }
}