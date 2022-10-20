using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
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

            [Column(IsTimestamp = true)] public DateTime Time { get; set; }

            public override string ToString()
            {
                return $"{Time:MM/dd/yyyy hh:mm:ss.fff tt} {Location} value: {Value}";
            }
        }

        public static async Task Main()
        {
            using (var client = new InfluxDBClient("http://localhost:9999",
                       "my-user", "my-password"))
            {
                var organizationClient = client.GetOrganizationsApi();

                var medicalGMBH = await organizationClient
                    .CreateOrganizationAsync("Medical Corp " +
                                             DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                 CultureInfo.InvariantCulture));

                //
                // Create New Bucket with retention 1h
                //
                var temperatureBucket =
                    await client.GetBucketsApi().CreateBucketAsync("temperature-sensors", medicalGMBH.Id);

                //
                // Add Permissions to read and write to the Bucket
                //
                var resource = new PermissionResource
                    {Type = PermissionResource.TypeBuckets, OrgID = medicalGMBH.Id, Id = temperatureBucket.Id};

                var readBucket = new Permission(Permission.ActionEnum.Read, resource);
                var writeBucket = new Permission(Permission.ActionEnum.Write, resource);

                var authorization = await client.GetAuthorizationsApi()
                    .CreateAuthorizationAsync(medicalGMBH, new List<Permission> {readBucket, writeBucket});

                Console.WriteLine($"The token to write to temperature-sensors bucket is: {authorization.Token}");
            }

            //
            // Create new client with specified authorization token
            //

            using (var client = new InfluxDBClient("http://localhost:9999",
                       "my-user", "my-password"))
            {

                var writeOptions = new WriteOptions
                {
                    BatchSize = 5000,
                    FlushInterval = 1000,
                    JitterInterval = 1000,
                    RetryInterval = 5000
                };

                var organizationClient = client.GetOrganizationsApi();

                var medicalGMBH = await organizationClient
                    .CreateOrganizationAsync("Medical Corp " +
                                             DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                 CultureInfo.InvariantCulture));

                var temperatureBucket = await client.GetBucketsApi().FindBucketByNameAsync("temperature-sensors");

                //
                // Write data
                //
                using (var writeClient = client.GetWriteApi(writeOptions))
                {
                    //
                    // Write by POCO
                    //
                    var temperature = new Temperature {Location = "south", Value = 62D, Time = DateTime.UtcNow};
                    writeClient.WriteMeasurement(temperature, WritePrecision.Ns, "temperature-sensors", medicalGMBH.Id);

                    //
                    // Write by Point
                    //
                    var point = PointData.Measurement("temperature")
                        .Tag("location", "west")
                        .Field("value", 55D)
                        .Timestamp(DateTime.UtcNow.AddSeconds(-10), WritePrecision.Ns);
                    writeClient.WritePoint(point, "temperature-sensors", medicalGMBH.Id);

                    //
                    // Write by LineProtocol
                    //
                    var record = "temperature,location=north value=60.0";
                    writeClient.WriteRecord(record, WritePrecision.Ns, "temperature-sensors", medicalGMBH.Id);

                    writeClient.Flush();
                    Thread.Sleep(2000);
                }

                //
                // Read data
                //
                var fluxTables = await client.GetQueryApi()
                    .QueryAsync("from(bucket:\"temperature-sensors\") |> range(start: 0)", medicalGMBH.Id);
                fluxTables.ForEach(fluxTable =>
                {
                    var fluxRecords = fluxTable.Records;
                    fluxRecords.ForEach(fluxRecord =>
                    {
                        Console.WriteLine($"{fluxRecord.GetTime()}: {fluxRecord.GetValue()}");
                    });
                });

                //
                // Delete data
                //
                await client.GetDeleteApi().Delete(DateTime.UtcNow.AddMinutes(-1), DateTime.Now, "", temperatureBucket,
                    medicalGMBH);
            }
        }
    }
}