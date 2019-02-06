using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using InfluxDB.Client;
using InfluxDB.Client.Core;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Writes;
using Task = System.Threading.Tasks.Task;

namespace Examples
{
    public static class PlatformExample
    {
        [Measurement("temperature")]
        private class Temperature
        {
            [Column("location", IsTag = true)] public string Location { get; set; }

            [Column("value")] public double Value { get; set; }

            [Column(IsTimestamp = true)] public DateTime Time;
        }

        public static async Task Example(InfluxDBClient influxDB)
        {
            var organizationClient = influxDB.GetOrganizationsApi();
            
            var medicalGMBH = await organizationClient
                            .CreateOrganization("Medical Corp " + 
                                                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                                CultureInfo.InvariantCulture));


            //
            // Create New Bucket with retention 1h
            //
            var temperatureBucket = await influxDB.GetBucketsApi().CreateBucket("temperature-sensors", medicalGMBH.Id);

            //
            // Add Permissions to read and write to the Bucket
            //
            var resource = new PermissionResource
                {Type = ResourceType.Buckets, OrgId = medicalGMBH.Id, Id = temperatureBucket.Id};
            var readBucket = new Permission
            {
                Resource = resource, 
                Action = Permission.ReadAction
            };

            var writeBucket = new Permission
            {
                Resource = resource, 
                Action = Permission.WriteAction
            };

            var authorization = await influxDB.GetAuthorizationsApi()
                .CreateAuthorization(medicalGMBH, new List<Permission> {readBucket, writeBucket});

            Console.WriteLine($"The token to write to temperature-sensors bucket is: {authorization.Token}");
            
            influxDB.Dispose();
            
            //
            // Create new client with specified authorization token
            //

            influxDB = InfluxDBClientFactory.Create("http://localhost:9999", authorization.Token.ToCharArray());

            var writeOptions = WriteOptions
                .CreateNew()
                .BatchSize(5000)
                .FlushInterval(1000)
                .JitterInterval(1000)
                .RetryInterval(5000)
                .Build();
            
            //
            // Write data
            //
            using (var writeClient = influxDB.GetWriteApi(writeOptions))
            {
                //
                // Write by POCO
                //
                var temperature = new Temperature {Location = "south", Value = 62D, Time = DateTime.UtcNow};
                writeClient.WriteMeasurement("temperature-sensors", medicalGMBH.Id, TimeUnit.Nanos, temperature);

                //
                // Write by Point
                //
                var point = Point.Measurement("temperature")
                    .Tag("location", "west")
                    .Field("value", 55D)
                    .Timestamp(DateTime.UtcNow.AddSeconds(-10), TimeUnit.Nanos);
                writeClient.WritePoint("temperature-sensors", medicalGMBH.Id, point);

                //
                // Write by LineProtocol
                //
                var record = "temperature,location=north value=60.0";
                writeClient.WriteRecord("temperature-sensors", medicalGMBH.Id, TimeUnit.Nanos, record);
                
                writeClient.Flush();
                Thread.Sleep(2000);
            }
            
            //
            // Read data
            //
            var fluxTables = await influxDB.GetQueryApi().Query("from(bucket:\"temperature-sensors\") |> range(start: 0)", medicalGMBH.Id);
            fluxTables.ForEach(fluxTable =>
            {
                var fluxRecords = fluxTable.Records;
                fluxRecords.ForEach(fluxRecord =>
                {
                    Console.WriteLine($"{fluxRecord.GetTime()}: {fluxRecord.GetValue()}");
                });
            });


            influxDB.Dispose();
        }
        
        public static void Run()
        {
            var client = InfluxDBClientFactory.Create("http://localhost:9999",
                            "my-user", "my-password".ToCharArray());

            Example(client).Wait();
        }
    }
}