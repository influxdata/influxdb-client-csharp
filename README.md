# influxdb-client-csharp

> This library is under development and no stable version has been released yet.  
> The API can change at any moment.

[![Build Status](https://travis-ci.org/bonitoo-io/influxdb-client-csharp.svg?branch=master)](https://travis-ci.org/bonitoo-io/influxdb-client-csharp)
[![codecov](https://codecov.io/gh/bonitoo-io/influxdb-client-csharp/branch/master/graph/badge.svg)](https://codecov.io/gh/bonitoo-io/influxdb-client-csharp)
[![License](https://img.shields.io/github/license/bonitoo-io/influxdb-client-csharp.svg)](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/LICENSE)
[![GitHub issues](https://img.shields.io/github/issues-raw/bonitoo-io/influxdb-client-csharp.svg)](https://github.com/bonitoo-io/influxdb-client-csharp/issues)
[![GitHub pull requests](https://img.shields.io/github/issues-pr-raw/bonitoo-io/influxdb-client-csharp.svg)](https://github.com/bonitoo-io/influxdb-client-csharp/pulls)

This repository contains the reference C# client for the InfluxDB 2.0.

- [Features](#features)
- [Documentation](#documentation)
- [How To Use](#how-to-use)
    - [Writes and Queries in InfluxDB 2.0](#writes-and-queries-in-influxdb-20)
    - [Use Management API to create a new Bucket in InfluxDB 2.0](#use-management-api-to-create-a-new-bucket-in-influxdb-20)
    - [Flux queries in InfluxDB 1.7+](#flux-queries-in-influxdb-17)
- [Contributing](#contributing)
- [License](#license)

## Features

- Supports querying using the Flux language over the InfluxDB 1.7+ REST API (`/api/v2/query endpoint`) 
- InfluxDB 2.0 client
    - Querying data using the Flux language
    - Writing data using
        - [Line Protocol](https://docs.influxdata.com/influxdb/v1.6/write_protocols/line_protocol_tutorial/) 
        - [Data Point](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/Client/Writes/Point.cs#L15) 
        - POCO
    - InfluxDB 2.0 Management API client for managing
        - sources, buckets
        - tasks
        - authorizations
        - health check
        - ...
## Documentation

The C# clients are implemented for the InfluxDB 2.0 and InfluxDB 1.7+:

| Client | Description | Documentation | Compatibility |
| --- | --- | --- |                                      --- |
| **[Client](./Client)** | The reference C# client that allows query, write and InfluxDB 2.0 management. | [readme](./Client#influxdbclient)| 2.0 |
| **[Client.Legacy](./Client.Legacy)**  | The reference C# client that allows you to perform Flux queries against InfluxDB 1.7+. | [readme](./Client.Legacy#influxdbclientflux) | 1.7+ |


## How To Use 

This clients are a work in progress and hosted in Bonitoo.io Nuget Repository.

If you want to use it with a .Net CLI or Package Manager, you have to specify source of packages. See examples below.

### Writes and Queries in InfluxDB 2.0

The following example demonstrates how to write data to InfluxDB 2.0 and read them back using the Flux language:

#### Installation

Use the latest version:

##### .Net CLI
```bash
dotnet add package InfluxDB.Client --version 1.0.0-alpha --source https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/
```

##### Or when using Package Manager
```bash
Install-Package InfluxDB.Client -Version 1.0.0-alpha -Source https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/
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
            var influxDBClient = InfluxDBClientFactory.Create("http://localhost:9999", Token);

            //
            // Write Data
            //
            using (var writeApi = influxDBClient.GetWriteApi())
            {
                //
                // Write by Point
                //
                var point = Point.Measurement("temperature")
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

            var fluxTables = await influxDBClient.GetQueryApi().Query(flux, "org_id");
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

### Use Management API to create a new Bucket in InfluxDB 2.0  

The following example demonstrates how to use a InfluxDB 2.0 Management API. For further information see [client documentation](./Client#management-api).

#### Installation

Use the latest version:

##### .Net CLI
```bash
dotnet add package InfluxDB.Client --version 1.0.0-alpha --source https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/
```

##### Or when using Package Manager
```bash
Install-Package InfluxDB.Client -Version 1.0.0-alpha -Source https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/
```

```c#
using System.Collections.Generic;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Task = System.Threading.Tasks.Task;

namespace Examples
{
    public static class ManagementExample
    {
        private static readonly char[] Token = "".ToCharArray();

        public static async Task Main(string[] args)
        {
            var influxDBClient = InfluxDBClientFactory.Create("http://localhost:9999", Token);

            //
            // Create bucket "iot_bucket" with data retention set to 3,600 seconds
            //
            var retention = new BucketRetentionRules(BucketRetentionRules.TypeEnum.Expire, 3600);

            var bucket = await influxDBClient.GetBucketsApi().CreateBucket("iot_bucket", retention, "org_id");
            
            //
            // Create access token to "iot_bucket"
            //
            var resource = new PermissionResource{Id = bucket.Id, OrgID = "org_id",Type = PermissionResource.TypeEnum.Buckets};

            // Read permission
            var read = new Permission{Resource = resource, Action = Permission.ActionEnum.Read};
            
            // Write permission
            var write = new Permission{Resource = resource, Action = Permission.ActionEnum.Write};

            var authorization = await influxDBClient.GetAuthorizationsApi()
                .CreateAuthorization("org_id", new List<Permission> {read, write});

            
            //
            // Created token that can be use for writes to "iot_bucket"
            //
            var token = authorization.Token;
            
            influxDBClient.Dispose();
        }        
    }
}
```

### Flux queries in InfluxDB 1.7+

The following example demonstrates querying using the Flux language.

#### Installation

Use the latest version:

##### .Net CLI
```bash
dotnet add package InfluxDB.Client.Flux --version 1.0.0-alpha --source https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/
```

##### Or when using Package Manager
```bash
Install-Package InfluxDB.Client.Flux -Version 1.0.0-alpha -Source https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/
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
            var fluxClient = FluxClientFactory.Create("http://localhost:8086/");

            var fluxQuery = "from(bucket: \"telegraf\")\n"
                               + " |> filter(fn: (r) => (r[\"_measurement\"] == \"cpu\" AND r[\"_field\"] == \"usage_system\"))"
                               + " |> range(start: -1d)"
                               + " |> sample(n: 5, pos: 1)";

            fluxClient.Query(fluxQuery, (cancellable, record) =>
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

The InfluxDB 2.0 JVM Based Clients are released under the [MIT License](https://opensource.org/licenses/MIT).
