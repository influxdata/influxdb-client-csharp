# InfluxDB.Client

> This library is under development and no stable version has been released yet.  
> The API can change at any moment.

The reference client that allows query, write and management (bucket, organization, users) for the InfluxDB 2.0.

## Features
 
- [Querying data using Flux language](#queries)
- [Writing data using](#writes)
    - [Line Protocol](#by-lineprotocol) 
    - [Data Point](#by-data-point) 
    - [POCO](#by-poco)
- [InfluxDB 2.0 Management API](#management-api)
    - sources, buckets
    - tasks
    - authorizations
    - health check
- [Advanced Usage](#advanced-usage)

## Queries

For querying data we use [QueryApi](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/Client/QueryApi.cs#L1) that allow perform synchronous, asynchronous and also use raw query response.

### Synchronous query

The synchronous query is not intended for large query results because the Flux response can be potentially unbound.

```c#
using System;
using System.Threading.Tasks;
using InfluxDB.Client;

namespace Examples
{
    public static class SynchronousQuery
    {
        private static readonly char[] Token = "".ToCharArray();

        public static async Task Main(string[] args)
        {
            var influxDBClient = InfluxDBClientFactory.Create("http://localhost:9999", Token);

            var flux = "from(bucket:\"temperature-sensors\") |> range(start: 0)";
            
            var queryApi = influxDBClient.GetQueryApi();

            //
            // QueryData
            //
            var tables = await queryApi.Query(flux, "org_id");
            tables.ForEach(table =>
            {
                table.Records.ForEach(record =>
                {
                    Console.WriteLine($"{record.GetTime()}: {record.GetValueByKey("_value")}");
                });
            });

            influxDBClient.Dispose();
        }        
    }
}
```

The synchronous query offers a possibility map [FluxRecords](http://bit.ly/flux-spec#record) to POCO:

```c#
using System;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Core;

namespace Examples
{
    public static class SynchronousQuery
    {
        private static readonly char[] Token = "".ToCharArray();

        public static async Task Main(string[] args)
        {
            var influxDBClient = InfluxDBClientFactory.Create("http://localhost:9999", Token);

            var flux = "from(bucket:\"temperature-sensors\") |> range(start: 0)";
            
            var queryApi = influxDBClient.GetQueryApi();

            //
            // QueryData
            //
            var temperatures = await queryApi.Query<Temperature>(flux, "org_id");
            temperatures.ForEach(temperature =>
            {
                Console.WriteLine($"{temperature.Location}: {temperature.Value} at {temperature.Time}");
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

### Asynchronous query

The Asynchronous query offers possibility to process unbound query and allow user to handle exceptions, 
stop receiving more results and notify that all data arrived. 

```c#
using System;
using System.Threading.Tasks;
using InfluxDB.Client;

namespace Examples
{
    public static class AsynchronousQuery
    {
        private static readonly char[] Token = "".ToCharArray();

        public static async Task Main(string[] args)
        {
            var influxDBClient = InfluxDBClientFactory.Create("http://localhost:9999", Token);

            var flux = "from(bucket:\"temperature-sensors\") |> range(start: 0)";

            var queryApi = influxDBClient.GetQueryApi();

            //
            // QueryData
            //
            await queryApi.Query(flux, "org_id", (cancellable, record) =>
            {
                //
                // The callback to consume a FluxRecord.
                //
                // cancelable - object has the cancel method to stop asynchronous query
                //
                Console.WriteLine($"{record.GetTime()}: {record.GetValueByKey("_value")}");
            }, exception =>
            {
                //
                // The callback to consume any error notification.
                //
                Console.WriteLine($"Error occurred: {exception.Message}");
            }, () =>
            {
                //
                // The callback to consume a notification about successfully end of stream.
                //
                Console.WriteLine("Query completed");
            });

            influxDBClient.Dispose();
        }
    }
}
```

And there is also a possibility map [FluxRecords](http://bit.ly/flux-spec#record) to POCO:

```c#
using System;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Core;

namespace Examples
{
    public static class AsynchronousQuery
    {
        private static readonly char[] Token = "".ToCharArray();

        public static async Task Main(string[] args)
        {
            var influxDBClient = InfluxDBClientFactory.Create("http://localhost:9999", Token);

            var flux = "from(bucket:\"temperature-sensors\") |> range(start: 0)";
            
            var queryApi = influxDBClient.GetQueryApi();

            //
            // QueryData
            //
            await queryApi.Query<Temperature>(flux, "org_id", (cancellable, temperature) =>
            {
                //
                // The callback to consume a FluxRecord mapped to POCO.
                //
                // cancelable - object has the cancel method to stop asynchronous query
                //
                Console.WriteLine($"{temperature.Location}: {temperature.Value} at {temperature.Time}");
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

### Raw query

The Raw query allows direct processing original [CSV response](http://bit.ly/flux-spec#csv): 

```c#
using System;
using System.Threading.Tasks;
using InfluxDB.Client;

namespace Examples
{
    public static class RawQuery
    {
        private static readonly char[] Token = "".ToCharArray();

        public static async Task Main(string[] args)
        {
            var influxDBClient = InfluxDBClientFactory.Create("http://localhost:9999", Token);

            var flux = "from(bucket:\"temperature-sensors\") |> range(start: 0)";

            var queryApi = influxDBClient.GetQueryApi();

            //
            // QueryData
            //
            var csv = await queryApi.QueryRaw(flux, "org_id");
            
            Console.WriteLine($"CSV response: {csv}");

            influxDBClient.Dispose();
        }
    }
}
```

The Asynchronous version allows processing line by line:

```c#
using System;
using System.Threading.Tasks;
using InfluxDB.Client;

namespace Examples
{
    public static class RawQueryAsynchronous
    {
        private static readonly char[] Token = "".ToCharArray();

        public static async Task Main(string[] args)
        {
            var influxDBClient = InfluxDBClientFactory.Create("http://localhost:9999", Token);

            var flux = "from(bucket:\"temperature-sensors\") |> range(start: 0)";

            var queryApi = influxDBClient.GetQueryApi();

            //
            // QueryData
            //
            await queryApi.QueryRaw(flux, "org_id", (cancellable, line) =>
            {
                //
                // The callback to consume a line of CSV response
                //
                // cancelable - object has the cancel method to stop asynchronous query
                //
                Console.WriteLine($"Response: {line}");
            });

            influxDBClient.Dispose();
        }
    }
}
```

## Writes

For writing data we use [WriteApi](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/Client/WriteApi.cs#L1) that supports:

1. writing data using [InfluxDB Line Protocol](https://docs.influxdata.com/influxdb/v1.6/write_protocols/line_protocol_tutorial/), Data Point, POCO 
2. use batching for writes
4. produces events that allow user to be notified and react to this events
    - `WriteSuccessEvent` - published when arrived the success response from Platform server
    - `WriteErrorEvent` - published when occurs a unhandled exception
    - `WriteRetriableErrorEvent` - published when occurs a retriable error
5. use GZIP compression for data

The writes are processed in batches which are configurable by `WriteOptions`:

| Property | Description | Default Value |
| --- | --- | --- |
| **BatchSize** | the number of data point to collect in batch | 1000 |
| **FlushInterval** | the number of milliseconds before the batch is written | 1000 |
| **JitterInterval** | the number of milliseconds to increase the batch flush interval by a random amount (see documentation above) | 0 |
| **RetryInterval** | the number of milliseconds to retry unsuccessful write. The retry interval is used when the InfluxDB server does not specify "Retry-After" header. | 1000 |

### Writing data

#### By POCO

Write Measurement into specified bucket:

```c#
using System;
using InfluxDB.Client;
using InfluxDB.Client.Core;

namespace Examples
{
    public static class WritePoco
    {
        private static readonly char[] Token = "".ToCharArray();

        public static void Main(string[] args)
        {
            var influxDBClient = InfluxDBClientFactory.Create("http://localhost:9999", Token);

            //
            // Write Data
            //
            using (var writeApi = influxDBClient.GetWriteApi())
            {
                //
                // Write by POCO
                //
                var temperature = new Temperature {Location = "south", Value = 62D, Time = DateTime.UtcNow};

                writeApi.WriteMeasurement("bucket_name", "org_id", TimeUnit.Nanos, temperature);
            }
            
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

#### By Data Point

Write Data point into specified bucket:

```c#
using System;
using InfluxDB.Client;
using InfluxDB.Client.Writes;

namespace Examples
{
    public static class WriteDataPoint
    {
        private static readonly char[] Token = "".ToCharArray();

        public static void Main(string[] args)
        {
            var influxDBClient = InfluxDBClientFactory.Create("http://localhost:9999", Token);

            //
            // Write Data
            //
            using (var writeApi = influxDBClient.GetWriteApi())
            {
                //
                // Write by Data Point
                
                var point = Point.Measurement("temperature")
                    .Tag("location", "west")
                    .Field("value", 55D)
                    .Timestamp(DateTime.UtcNow.AddSeconds(-10), TimeUnit.Nanos);
                
                writeApi.WritePoint("bucket_name", "org_id", point);
            }
            
            influxDBClient.Dispose();
        }
    }
}
```

#### By LineProtocol

Write Line Protocol record into specified bucket:

```c#
using InfluxDB.Client;

namespace Examples
{
    public static class WriteLineProtocol
    {
        private static readonly char[] Token = "".ToCharArray();

        public static void Main(string[] args)
        {
            var influxDBClient = InfluxDBClientFactory.Create("http://localhost:9999", Token);

            //
            // Write Data
            //
            using (var writeApi = influxDBClient.GetWriteApi())
            {
                //
                //
                // Write by LineProtocol
                //
                writeApi.WriteRecord("bucket_name", "org_id", TimeUnit.Nanos, "temperature,location=north value=60.0");
            }
            
            influxDBClient.Dispose();
        }
    }
}
```

### Handle the Events

#### Handle the Success write

```c#
//
// Register event handler
//
writeApi.EventHandler += (sender, eventArgs) =>
{
    if (eventArgs is WriteSuccessEvent @event)
    {
        string data = @event.LineProtocol;
        
        //
        // handle success
        //
    }
};
```

#### Handle the Error Write

```c#
//
// Register event handler
//
writeApi.EventHandler += (sender, eventArgs) =>
{
    if (eventArgs is WriteErrorEvent @event)
    {
        var exception = @event.Exception;
        
        //
        // handle error
        //
    }
};
```

## Management API

The client has following management API:

| API endpoint | Description | Implementation |
| --- | --- | --- |
| **/api/v2/authorizations** | Managing authorization data | [AuthorizationsApi](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/Client/AuthorizationsApi.cs#L1) |
| **/api/v2/buckets** | Managing bucket data | [BucketsApi](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/Client/BucketsApi.cs#L1) |
| **/api/v2/orgs** | Managing organization data | [OrganizationsApi](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/Client/OrganizationsApi.cs#L1) |
| **/api/v2/users** | Managing user data | [UsersApi](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/Client/UsersApi.cs#L1) |
| **/api/v2/sources** | Managing sources | [SourcesApi](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/Client/SourcesApi.cs#L1) |
| **/api/v2/tasks** | Managing one-off and recurring tasks | [TasksApi](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/Client/TasksApi.cs#L1) |
| **/api/v2/scrapers** | Managing ScraperTarget data | [ScraperTargetsApi](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/Client/ScraperTargetsApi.cs#L1) |
| **/api/v2/labels** | Managing resource labels | [LabelsApi](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/Client/LabelsApi.cs#L1) |
| **/api/v2/telegrafs** | Managing telegraf config data | [TelegrafsApi](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/Client/TelegrafsApi.cs#L1) |
| **/api/v2/setup** | Managing onboarding setup | [InfluxDBClient#OnBoarding()](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/Client/InfluxDBClient.cs#L191-) |
| **/ready** | Get the readiness of a instance at startup| [InfluxDBClient#Ready()](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/Client/InfluxDBClient.cs#L169--) |
| **/health** | Get the health of an instance anytime during execution | [InfluxDBClient#Health()](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/Client/InfluxDBClient.cs#L160--) |

The following example demonstrates how to use a InfluxDB 2.0 Management API. For further information see endpoints implementation.

```c#
using System.Collections.Generic;
using InfluxDB.Client;
using InfluxDB.Client.Domain;
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
            var retention = new RetentionRule{EverySeconds = 3600};

            var bucket = await influxDBClient.GetBucketsApi().CreateBucket("iot_bucket", retention, "org_id");
            
            //
            // Create access token to "iot_bucket"
            //
            var resource = new PermissionResource{Id = bucket.Id, OrgId = "org_id",Type = ResourceType.Buckets};

            // Read permission
            var read = new Permission{Resource = resource, Action = Permission.ReadAction};
            
            // Write permission
            var write = new Permission{Resource = resource, Action = Permission.WriteAction};

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

## Advanced Usage

### Gzip support
`InfluxDBClient` does not enable gzip compress for http request body by default. If you want to enable gzip to reduce transfer data's size, you can call:

```c#
influxDBClient.EnableGzip();
```

#### Log HTTP Request and Response

The Requests and Responses can be logged by changing the LogLevel. LogLevel values are None, Basic, Headers, Body. Note that 
applying the `Body` LogLevel will disable chunking while streaming and will load the whole response into memory.  

```c#
influxDBClient.SetLogLevel(LogLevel.Body)
```

#### Check the server status and version

Server availability can be checked using the `influxDBClient.health()` endpoint.
 
## Version

The latest package for .NET CLI:
```bash
dotnet add package InfluxDB.Client --version 1.0-alpha --source https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/
```
  
Or when using with Package Manager:
```bash
Install-Package InfluxDB.Client -Version 1.0-alpha -Source https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/
```