using System.Threading.Tasks;
using InfluxDB.Client;

namespace Examples
{
    /// <summary>
    /// Warning: Invocable Scripts are supported only in InfluxDB Cloud,
    /// currently there is no support in InfluxDB OSS.
    /// </summary>
    public static class InvocableScripts
    {
        public static async Task Main(string[] args)
        {
            const string host = "https://us-west-2-1.aws.cloud2.influxdata.com";
            const string token = "my-token";
            const string bucket = "my-bucket";
            const string organization = "my-org";
            
            var options = new InfluxDBClientOptions.Builder()
                .Url(host)
                .AuthenticateToken(token.ToCharArray())
                .Org(organization)
                .Bucket(bucket)
                .Build();

            using var client = InfluxDBClientFactory.Create(options);
        }
    }
}