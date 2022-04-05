# influxdb-client-csharp

[![CircleCI](https://circleci.com/gh/influxdata/influxdb-client-csharp.svg?style=svg)](https://circleci.com/gh/influxdata/influxdb-client-csharp)
[![codecov](https://codecov.io/gh/influxdata/influxdb-client-csharp/branch/master/graph/badge.svg)](https://codecov.io/gh/influxdata/influxdb-client-csharp)
[![Nuget](https://img.shields.io/nuget/v/InfluxDB.Client)](https://www.nuget.org/packages/InfluxDB.Client/)
[![License](https://img.shields.io/github/license/influxdata/influxdb-client-csharp.svg)](https://github.com/influxdata/influxdb-client-csharp/blob/master/LICENSE)
[![GitHub issues](https://img.shields.io/github/issues-raw/influxdata/influxdb-client-csharp.svg)](https://github.com/influxdata/influxdb-client-csharp/issues)
[![GitHub pull requests](https://img.shields.io/github/issues-pr-raw/influxdata/influxdb-client-csharp.svg)](https://github.com/influxdata/influxdb-client-csharp/pulls)
[![Slack Status](https://img.shields.io/badge/slack-join_chat-white.svg?logo=slack&style=social)](https://www.influxdata.com/slack)

This repository contains the reference C# client for the InfluxDB 2.x.

#### Note: Use this client library with InfluxDB 2.x and InfluxDB 1.8+ ([see details](#influxdb-18-api-compatibility)). For connecting to InfluxDB 1.7 or earlier instances, use the [influxdb-csharp](https://github.com/influxdata/influxdb-csharp) client library.

- [Features](#features)
- [Documentation](#documentation)
- [How To Use](#how-to-use)
    - [Writes and Queries in InfluxDB 2.x](#writes-and-queries-in-influxdb-2x)
    - [Use Management API to create a new Bucket in InfluxDB 2.x](#use-management-api-to-create-a-new-bucket-in-influxdb-2x)
    - [Flux queries in InfluxDB 1.7+](#flux-queries-in-influxdb-17)
- [Contributing](#contributing)
- [License](#license)


## Documentation

This section contains links to the client library documentation.

* [Product documentation](https://docs.influxdata.com/influxdb/latest/api-guide/client-libraries/), [Getting Started](#how-to-use)
* [Examples](Examples)
* [API Reference](https://influxdata.github.io/influxdb-client-csharp/api/InfluxDB.Client.html)
* [Changelog](CHANGELOG.md)

| Client | Description                                                                            | Documentation | Compatibility |
| --- |----------------------------------------------------------------------------------------| --- |---------------|
| **[Client](./Client#influxdbclient)** | The reference C# client that allows query, write and InfluxDB 2.x management.          | [readme](./Client#influxdbclient)| 2.x           |
| **[Client.Linq](./Client.Linq#influxdbclientlinq)**  | The library supports to use a LINQ expression to query the InfluxDB.                   | [readme](./Client.Linq#influxdbclientlinq) | 2.x           |
| **[Client.Legacy](./Client.Legacy#influxdbclientflux)**  | The reference C# client that allows you to perform Flux queries against InfluxDB 1.7+. | [readme](./Client.Legacy#influxdbclientflux) | 1.7+          |

## Features

- Supports querying using the Flux language over the InfluxDB 1.7+ REST API (`/api/v2/query endpoint`) 
- InfluxDB 2.x client
    - Querying data using the Flux language
    - Writing data using
        - [Line Protocol](https://docs.influxdata.com/influxdb/v1.6/write_protocols/line_protocol_tutorial/) 
        - [Data Point](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/Writes/PointData.cs#L17) 
        - POCO
    - InfluxDB 2.x Management API client for managing
        - sources, buckets
        - tasks
        - authorizations
        - health check
        - ...
    
## How To Use 

### Writes and Queries in InfluxDB 2.x

The following example demonstrates how to write data to InfluxDB 2.x and read them back using the Flux language:

#### Installation

Use the latest version:

##### .Net CLI
```bash
dotnet add package InfluxDB.Client
```

##### Or when using Package Manager
```bash
Install-Package InfluxDB.Client
```

```c#
using System;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;
using Task = System.Threading.Tasks.Task;

namespace Examples
{
    public static class QueriesWritesExample
    {
        private static readonly char[] Token = "".ToCharArray();

        public static async Task Main(string[] args)
        {
            var influxDBClient = InfluxDBClientFactory.Create("http://localhost:8086", Token);

            //
            // Write Data
            //
            using (var writeApi = influxDBClient.GetWriteApi())
            {
                //
                // Write by Point
                //
                var point = PointData.Measurement("temperature")
                    .Tag("location", "west")
                    .Field("value", 55D)
                    .Timestamp(DateTime.UtcNow.AddSeconds(-10), WritePrecision.Ns);
                
                writeApi.WritePoint("bucket_name", "org_id", point);
                
                //
                // Write by LineProtocol
                //
                writeApi.WriteRecord("bucket_name", "org_id", WritePrecision.Ns, "temperature,location=north value=60.0");
                
                //
                // Write by POCO
                //
                var temperature = new Temperature {Location = "south", Value = 62D, Time = DateTime.UtcNow};
                writeApi.WriteMeasurement("bucket_name", "org_id", WritePrecision.Ns, temperature);
            }
            
            //
            // Query data
            //
            var flux = "from(bucket:\"temperature-sensors\") |> range(start: 0)";

            var fluxTables = await influxDBClient.GetQueryApi().QueryAsync(flux, "org_id");
            fluxTables.ForEach(fluxTable =>
            {
                var fluxRecords = fluxTable.Records;
                fluxRecords.ForEach(fluxRecord =>
                {
                    Console.WriteLine($"{fluxRecord.GetTime()}: {fluxRecord.GetValue()}");
                });
            });

            influxDBClient.Dispose();
        }
        
        [Measurement("temperature")]
        private class Temperature
        {
            [Column("location", IsTag = true)] public string Location { get; set; }

            [Column("value")] public double Value { get; set; }

            [Column(IsTimestamp = true)] public DateTime Time;
        }
    }
}
```

### Use Management API to create a new Bucket in InfluxDB 2.x  

The following example demonstrates how to use a InfluxDB 2.x Management API. For further information see [client documentation](./Client#management-api).

#### Installation

Use the latest version:

##### .Net CLI
```bash
dotnet add package InfluxDB.Client
```

##### Or when using Package Manager
```bash
Install-Package InfluxDB.Client
```

```c#
using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Task = System.Threading.Tasks.Task;

namespace Examples
{
    public static class ManagementExample
    {
        public static async Task Main(string[] args)
        {
            const string url = "http://localhost:8086";
            const string token = "my-token";
            const string org = "my-org";
            
            using var client = InfluxDBClientFactory.Create(url, token);

            // Find ID of Organization with specified name (PermissionAPI requires ID of Organization).
            var orgId = (await client.GetOrganizationsApi().FindOrganizationsAsync(org: org)).First().Id;

            //
            // Create bucket "iot_bucket" with data retention set to 3,600 seconds
            //
            var retention = new BucketRetentionRules(BucketRetentionRules.TypeEnum.Expire, 3600);

            var bucket = await client.GetBucketsApi().CreateBucketAsync("iot_bucket", retention, orgId);

            //
            // Create access token to "iot_bucket"
            //
            var resource = new PermissionResource(PermissionResource.TypeEnum.Buckets, bucket.Id, null,
                orgId);

            // Read permission
            var read = new Permission(Permission.ActionEnum.Read, resource);

            // Write permission
            var write = new Permission(Permission.ActionEnum.Write, resource);

            var authorization = await client.GetAuthorizationsApi()
                .CreateAuthorizationAsync(orgId, new List<Permission> { read, write });

            //
            // Created token that can be use for writes to "iot_bucket"
            //
            Console.WriteLine($"Authorized token to write into iot_bucket: {authorization.Token}");
        }
    }
}
```

### InfluxDB 1.8 API compatibility

[InfluxDB 1.8.0 introduced forward compatibility APIs](https://docs.influxdata.com/influxdb/v1.8/tools/api/#influxdb-2-0-api-compatibility-endpoints) for InfluxDB 2.x. This allow you to easily move from InfluxDB 1.x to InfluxDB 2.x Cloud or open source.

The following forward compatible APIs are available:

| API | Endpoint | Description                                                                                                                                                                                                                                                    |
|:----------|:----------|:---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [QueryApi.cs](Client/QueryApi.cs) | [/api/v2/query](https://docs.influxdata.com/influxdb/v1.8/tools/api/#apiv2query-http-endpoint) | Query data in InfluxDB 1.8.0+ using the InfluxDB 2.x API and [Flux](https://docs.influxdata.com/flux/latest/) _(endpoint should be enabled by [`flux-enabled` option](https://docs.influxdata.com/influxdb/latest/administration/config/#flux-enabled-false))_ |
| [WriteApi.cs](Client/WriteApi.cs) | [/api/v2/write](https://docs.influxdata.com/influxdb/v1.8/tools/api/#apiv2write-http-endpoint) | Write data to InfluxDB 1.8.0+ using the InfluxDB 2.x API                                                                                                                                                                                                       |
| [PingAsync](Client/InfluxDBClient.cs#L431) | [/ping](https://docs.influxdata.com/influxdb/v1.8/tools/api/#ping-http-endpoint) | Checks the status of InfluxDB instance and version of InfluxDB.                                                                                                                                                                                                |    

For detail info see [InfluxDB 1.8 example](Examples/InfluxDB18Example.cs).


### Flux queries in InfluxDB 1.7+

The following example demonstrates querying using the Flux language.

#### Installation

Use the latest version:

##### .Net CLI
```bash
dotnet add package InfluxDB.Client.Flux
```

##### Or when using Package Manager
```bash
Install-Package InfluxDB.Client.Flux
``` 

```c#
using System;
using InfluxDB.Client.Flux;

namespace Examples
{
    public static class FluxExample
    {
        public static void Run()
        {
            using var fluxClient = FluxClientFactory.Create("http://localhost:8086/");

            var fluxQuery = "from(bucket: \"telegraf\")\n"
                               + " |> filter(fn: (r) => (r[\"_measurement\"] == \"cpu\" AND r[\"_field\"] == \"usage_system\"))"
                               + " |> range(start: -1d)"
                               + " |> sample(n: 5, pos: 1)";

            fluxClient.QueryAsync(fluxQuery, record =>
                            {
                                // process the flux query records
                                Console.WriteLine(record.GetTime() + ": " + record.GetValue());
                            },
                            (error) =>
                            {
                                // error handling while processing result
                                Console.WriteLine(error.ToString());

                            }, () =>
                            {
                                // on complete
                                Console.WriteLine("Query completed");
                            }).GetAwaiter().GetResult();
        }
    }
}
```

## Contributing

If you would like to contribute code you can do through GitHub by forking the repository and sending a pull request into the `master` branch.

## License

The InfluxDB 2.x Clients are released under the [MIT License](https://opensource.org/licenses/MIT).
