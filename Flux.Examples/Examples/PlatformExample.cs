using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using InfluxData.Platform.Client.Option;
using InfluxData.Platform.Client.Write;
using Platform.Common.Platform;
using Task = System.Threading.Tasks.Task;

namespace Flux.Examples.Examples
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

        public static async Task Example(PlatformClient platform)
        {
            var organizationClient = platform.CreateOrganizationClient();
            
            var medicalGMBH = await organizationClient
                            .CreateOrganization("Medical Corp " + 
                                                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                                CultureInfo.InvariantCulture));


            //
            // Create New Bucket with retention 1h
            //
            var temperatureBucket = await platform.CreateBucketClient().CreateBucket("temperature-sensors", medicalGMBH.Id);

            //
            // Add Permissions to read and write to the Bucket
            //
            var resource = new PermissionResource
                {Type = PermissionResourceType.Bucket, OrgId = medicalGMBH.Id, Id = temperatureBucket.Id};
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

            var authorization = await platform.CreateAuthorizationClient()
                .CreateAuthorization(medicalGMBH, new List<Permission> {readBucket, writeBucket});

            Console.WriteLine($"The token to write to temperature-sensors bucket is: {authorization.Token}");
            
            platform.Dispose();
            
            //
            // Create new client with specified authorization token
            //

            platform = PlatformClientFactory.Create("http://localhost:9999", authorization.Token.ToCharArray());

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
            using (var writeClient = platform.CreateWriteClient(writeOptions))
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
            var fluxTables = await platform.CreateQueryClient().Query("from(bucket:\"temperature-sensors\") |> range(start: 0)", medicalGMBH.Id);
            fluxTables.ForEach(fluxTable =>
            {
                var fluxRecords = fluxTable.Records;
                fluxRecords.ForEach(fluxRecord =>
                {
                    Console.WriteLine($"{fluxRecord.GetTime()}: {fluxRecord.GetValue()}");
                });
            });


            platform.Dispose();
        }
        
        public static void Run()
        {
            var platform = PlatformClientFactory.Create("http://localhost:9999",
                            "my-user", "my-password".ToCharArray());

            Example(platform).Wait();
        }
    }
}