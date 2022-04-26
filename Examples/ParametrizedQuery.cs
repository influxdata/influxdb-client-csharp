using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace Examples
{
    /// <summary>
    /// Parameterized Queries are supported only in InfluxDB Cloud, currently there is no support in InfluxDB OSS.
    /// </summary>
    public static class ParametrizedQuery
    {
        private const string Url = "https://us-west-2-1.aws.cloud2.influxdata.com";
        private const string Token = "my-token";
        private const string Org = "my-org";
        private const string Bucket = "my-bucket";

        public static async Task Main(string[] args)
        {
            var options = InfluxDBClientOptions.Builder
                .CreateNew()
                .Url(Url)
                .AuthenticateToken(Token)
                .Bucket(Bucket)
                .Org(Org)
                .Build();

            using var client = InfluxDBClientFactory.Create(options);

            //
            // Prepare Data
            //
            Console.WriteLine("*** Write Points ***");

            var point = PointData.Measurement("mem")
                .Tag("location", "Prague")
                .Field("temperature", 21.5);
            await client.GetWriteApiAsync().WritePointAsync(point);

            Console.WriteLine($"{point.ToLineProtocol()}");

            //
            // Query Data
            //
            Console.WriteLine("*** Query Points ***");

            var query = "from(bucket: params.bucketParam) |> range(start: duration(v: params.startParam))";
            var bindParams = new Dictionary<string, object>
            {
                { "bucketParam", Bucket },
                { "startParam", "-1h" }
            };

            var tables = await client.GetQueryApi()
                .QueryAsync(new Query(query: query, _params: bindParams, dialect: QueryApi.Dialect));

            // print results
            foreach (var record in tables.SelectMany(table => table.Records))
                Console.WriteLine(
                    $"{record.GetTime()} {record.GetMeasurement()}: {record.GetField()} {record.GetValue()}");
        }
    }
}