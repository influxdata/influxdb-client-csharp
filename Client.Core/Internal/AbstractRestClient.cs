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
        internal const string OrgArgumentValidation =
            "'org' parameter. Please specify the organization as a method parameter or use default configuration at 'InfluxDBClientOptions.Org'.";

        internal const string BucketArgumentValidation =
            "'bucket' parameter. Please specify the bucket as a method parameter or use default configuration at 'InfluxDBClientOptions.Bucket'.";

        protected async Task<bool> PingAsync(Task<RestResponse> request)
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

        protected async Task<string> VersionAsync(Task<RestResponse> request)
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

        private string GetVersion(RestResponse responseHttp)
        {
            Arguments.CheckNotNull(responseHttp, "responseHttp");

            var value = responseHttp.Headers
                .Where(header => header.Name.Equals("X-Influxdb-Version", StringComparison.OrdinalIgnoreCase))
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