using System;
using InfluxDB.Client;

namespace Examples
{
    public class SynchronousQuery
    {
        public static void Main()
        {
            using var client = new InfluxDBClient("http://localhost:9999", "my-token");

            const string query = "from(bucket:\"my-bucket\") |> range(start: 0)";

            //
            // QueryData
            //
            var queryApi = client.GetQueryApiSync();
            var tables = queryApi.QuerySync(query, "my-org");

            //
            // Process results
            //
            tables.ForEach(table =>
            {
                table.Records.ForEach(record =>
                {
                    Console.WriteLine($"{record.GetTime()}: {record.GetValueByKey("_value")}");
                });
            });
        }
    }
}