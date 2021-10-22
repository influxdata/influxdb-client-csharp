using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Core.Exceptions;
using RestSharp;

namespace InfluxDB.Client.Core.Internal
{
    public abstract class AbstractRestClient
    {
        protected async Task<bool> PingAsync(Task<IRestResponse> request)
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

        protected async Task<string> VersionAsync(Task<IRestResponse> request)
        {
            try
            {
                var response = await request.ConfigureAwait(false);

                return GetVersion(response);
            }
            catch (Exception e)
            {
                throw new InfluxException(e);
            }
        }
        
        private string GetVersion(IRestResponse responseHttp)
        {
            Arguments.CheckNotNull(responseHttp, "responseHttp");

            var value = responseHttp.Headers
                .Where(header => header.Name.Equals("X-Influxdb-Version"))
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