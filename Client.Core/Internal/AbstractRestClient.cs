using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace InfluxDB.Client.Core.Internal
{
    public abstract class AbstractRestClient
    {
        protected async Task<bool> PingAsync(Task request)
        {
            try
            {
                await request.ConfigureAwait(false);

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
                .Select(header => header.Value.ToString())
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