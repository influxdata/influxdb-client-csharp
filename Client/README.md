# InfluxDB.Client

[![CircleCI](https://circleci.com/gh/influxdata/influxdb-client-csharp.svg?style=svg)](https://circleci.com/gh/influxdata/influxdb-client-csharp)

The reference client that allows query, write and management (bucket, organization, users) for the InfluxDB 2.x.

## Documentation

This section contains links to the client library documentation.

* [Product documentation](https://docs.influxdata.com/influxdb/latest/api-guide/client-libraries/), [Getting Started](#queries)
* [Examples](../Examples)
* [API Reference](https://influxdata.github.io/influxdb-client-csharp/api/InfluxDB.Client.html)
* [Changelog](../CHANGELOG.md)

## Features
 
- [Querying data using Flux language](#queries)
    - [Asynchronous](#asynchronous-query)
    - [Streaming](#streaming-query)
    - [Synchronous](#synchronous-query)
    - [Raw Query](#raw-query)
- [Writing data using](#writes)
    - [Line Protocol](#by-lineprotocol) 
    - [Data Point](#by-data-point) 
    - [POCO](#by-poco)
    - [Default Tags](#default-tags)
- [Delete data](#delete-data)    
- [InfluxDB 2.x Management API](#management-api)
    - sources, buckets
    - tasks
    - authorizations
    - health check
- [Advanced Usage](#advanced-usage)
    - [Monitoring & Alerting](#monitoring--alerting)
    - [Custom mapping of DomainObject to/from InfluxDB](#custom-mapping-of-domainobject-tofrom-influxdb)
    - [Client configuration file](#client-configuration-file)
    - [Client connection string](#client-connection-string)
    - [Gzip support](#gzip-support)
    - [How to use WebProxy](#how-to-use-webproxy)
    - [Proxy and redirects configuration](#proxy-and-redirects-configuration)

## Queries

For querying data we use [QueryApi](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/QueryApi.cs#L1) that allow perform asynchronous, streaming, synchronous and also use raw query response.

### Asynchronous Query

The asynchronous query is not intended for large query results because the Flux response can be potentially unbound.

```c#
using System;
using System.Threading.Tasks;
using InfluxDB.Client;

namespace Examples
{
    public static class AsynchronousQuery
    {
        private static readonly string Token = "";

        public static async Task Main()
        {
            using var client = new InfluxDBClient("http://localhost:8086", Token);

            var flux = "from(bucket:\"temperature-sensors\") |> range(start: 0)";
            
            var queryApi = client.GetQueryApi();

            //
            // QueryData
            //
            var tables = await queryApi.QueryAsync(flux, "org_id");
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
```

The asynchronous query offers a possibility map [FluxRecords](http://bit.ly/flux-spec#record) to POCO:

```c#
using System;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Core;

namespace Examples
{
    public static class AsynchronousQuery
    {
        private static readonly string Token = "";

        public static async Task Main()
        {
            using var client = new InfluxDBClient("http://localhost:8086", Token);

            var flux = "from(bucket:\"temperature-sensors\") |> range(start: 0)";
            
            var queryApi = client.GetQueryApi();

            //
            // QueryData
            //
            var temperatures = await queryApi.QueryAsync<Temperature>(flux, "org_id");
            temperatures.ForEach(temperature =>
            {
                Console.WriteLine($"{temperature.Location}: {temperature.Value} at {temperature.Time}");
            });
        }  
        
        [Measurement("temperature")]
        private class Temperature
        {
            [Column("location", IsTag = true)] public string Location { get; set; }

            [Column("value")] public double Value { get; set; }

            [Column(IsTimestamp = true)] public DateTime Time { get; set; }
        }
    }
}
```

### Streaming Query

The Streaming query offers possibility to process unbound query and allow user to handle exceptions, 
stop receiving more results and notify that all data arrived. 

```c#
using System;
using System.Threading.Tasks;
using InfluxDB.Client;

namespace Examples
{
    public static class StreamingQuery
    {
        private static readonly string Token = "";

        public static async Task Main()
        {
            using var client = new InfluxDBClient("http://localhost:8086", Token);

            var flux = "from(bucket:\"temperature-sensors\") |> range(start: 0)";

            var queryApi = client.GetQueryApi();

            //
            // QueryData
            //
            await queryApi.QueryAsync(flux, record =>
            {
                //
                // The callback to consume a FluxRecord.
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
            }, "org_id");
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
    public static class StreamingQuery
    {
        private static readonly string Token = "";

        public static async Task Main()
        {
            using var client = new InfluxDBClient("http://localhost:8086", Token);

            var flux = "from(bucket:\"temperature-sensors\") |> range(start: 0)";
            
            var queryApi = client.GetQueryApi();

            //
            // QueryData
            //
            await queryApi.QueryAsync<Temperature>(flux, temperature =>
            {
                //
                // The callback to consume a FluxRecord mapped to POCO.
                //
                Console.WriteLine($"{temperature.Location}: {temperature.Value} at {temperature.Time}");
            }, org: "org_id");
        }  
        
        [Measurement("temperature")]
        private class Temperature
        {
            [Column("location", IsTag = true)] public string Location { get; set; }

            [Column("value")] public double Value { get; set; }

            [Column(IsTimestamp = true)] public DateTime Time { get; set; }
        }
    }
}
```

### Raw Query

The Raw query allows direct processing original [CSV response](http://bit.ly/flux-spec#csv): 

```c#
using System;
using System.Threading.Tasks;
using InfluxDB.Client;

namespace Examples
{
    public static class RawQuery
    {
        private static readonly string Token = "";

        public static async Task Main()
        {
            using var client = new InfluxDBClient("http://localhost:8086", Token);

            var flux = "from(bucket:\"temperature-sensors\") |> range(start: 0)";

            var queryApi = client.GetQueryApi();

            //
            // QueryData
            //
            var csv = await queryApi.QueryRawAsync(flux, org: "org_id");
            
            Console.WriteLine($"CSV response: {csv}");
        }
    }
}
```

The Streaming version allows processing line by line:

```c#
using System;
using System.Threading.Tasks;
using InfluxDB.Client;

namespace Examples
{
    public static class RawQueryAsynchronous
    {
        private static readonly string Token = "";

        public static async Task Main()
        {
            using var client = new InfluxDBClient("http://localhost:8086", Token);

            var flux = "from(bucket:\"temperature-sensors\") |> range(start: 0)";

            var queryApi = client.GetQueryApi();

            //
            // QueryData
            //
            await queryApi.QueryRawAsync(flux, line =>
            {
                //
                // The callback to consume a line of CSV response
                //
                Console.WriteLine($"Response: {line}");
            }, org: "org_id");
        }
    }
}
```

### Synchronous query

The synchronous query is not intended for large query results because the response can be potentially unbound.

```c#
using System;
using InfluxDB.Client;

namespace Examples
{
    public static class SynchronousQuery
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
```

## Writes

For writing data we use [WriteApi](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/WriteApi.cs#L1) or 
[WriteApiAsync](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/WriteApiAsync.cs) which is simplified version of WriteApi without batching support.

[WriteApi](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/WriteApi.cs#L1) supports:

1. writing data using [InfluxDB Line Protocol](https://docs.influxdata.com/influxdb/v1.6/write_protocols/line_protocol_tutorial/), Data Point, POCO 
1. use batching for writes
1. produces events that allow user to be notified and react to this events
    - `WriteSuccessEvent` - published when arrived the success response from server
    - `WriteErrorEvent` - published when occurs a unhandled exception from server
    - `WriteRetriableErrorEvent` - published when occurs a retriable error from server
    - `WriteRuntimeExceptionEvent` - published when occurs a runtime exception in background batch processing
1. use GZIP compression for data

The writes are processed in batches which are configurable by `WriteOptions`:

| Property | Description | Default Value |
| --- | --- | --- |
| **BatchSize** | the number of data point to collect in batch | 1000 |
| **FlushInterval** | the number of milliseconds before the batch is written | 1000 |
| **JitterInterval** | the number of milliseconds to increase the batch flush interval by a random amount| 0 |
| **RetryInterval** | the number of milliseconds to retry unsuccessful write. The retry interval is used when the InfluxDB server does not specify "Retry-After" header. | 5000 |
| **MaxRetries** | the number of max retries when write fails | 3 |
| **MaxRetryDelay** | the maximum delay between each retry attempt in milliseconds | 125_000 |
| **ExponentialBase** |  the base for the exponential retry delay, the next delay is computed using random exponential backoff as a random value within the interval  ``retryInterval * exponentialBase^(attempts-1)`` and ``retryInterval * exponentialBase^(attempts)``. Example for ``retryInterval=5_000, exponentialBase=2, maxRetryDelay=125_000, maxRetries=5`` Retry delays are random distributed values within the ranges of ``[5_000-10_000, 10_000-20_000, 20_000-40_000, 40_000-80_000, 80_000-125_000]`` | 2 |

### Writing data

#### By POCO

Write Measurement into specified bucket:

```c#
using System;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;

namespace Examples
{
    public static class WritePoco
    {
        private static readonly string Token = "";

        public static void Main()
        {
            using var client = new InfluxDBClient("http://localhost:8086", Token);

            //
            // Write Data
            //
            using (var writeApi = client.GetWriteApi())
            {
                //
                // Write by POCO
                //
                var temperature = new Temperature {Location = "south", Value = 62D, Time = DateTime.UtcNow};

                writeApi.WriteMeasurement(temperature, WritePrecision.Ns, "bucket_name", "org_id");
            }
        }
        
        [Measurement("temperature")]
        private class Temperature
        {
            [Column("location", IsTag = true)] public string Location { get; set; }

            [Column("value")] public double Value { get; set; }

            [Column(IsTimestamp = true)] public DateTime Time { get; set; }
        }
    }
}
```

#### By Data Point

Write Data point into specified bucket:

```c#
using System;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace Examples
{
    public static class WriteDataPoint
    {
        private static readonly string Token = "";

        public static void Main()
        {
            using var client = new InfluxDBClient("http://localhost:8086", Token);

            //
            // Write Data
            //
            using (var writeApi = client.GetWriteApi())
            {
                //
                // Write by Data Point
                
                var point = PointData.Measurement("temperature")
                    .Tag("location", "west")
                    .Field("value", 55D)
                    .Timestamp(DateTime.UtcNow.AddSeconds(-10), WritePrecision.Ns);
                
                writeApi.WritePoint(point, "bucket_name", "org_id");
            }
        }
    }
}
```

DataPoint Builder Immutability:
The builder is immutable therefore won't have side effect when using for building
multiple point with single builder.

```c#
using System;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace Examples
{
    public static class WriteDataPoint
    {
        private static readonly string Token = "";

        public static void Main()
        {
            using var client = new InfluxDBClient("http://localhost:8086", Token);

            //
            // Write Data
            //
            using (var writeApi = client.GetWriteApi())
            {
                //
                // Write by Data Point
                
                var builder = PointData.Measurement("temperature")
                    .Tag("location", "west");
                
                var pointA = builder
                    .Field("value", 55D)
                    .Timestamp(DateTime.UtcNow.AddSeconds(-10), WritePrecision.Ns);
                
                writeApi.WritePoint(pointA, "bucket_name", "org_id");
                
                var pointB = builder
                    .Field("age", 32)
                    .Timestamp(DateTime.UtcNow, WritePrecision.Ns);
                
                writeApi.WritePoint(pointB, "bucket_name", "org_id");
            }
        }
    }
}
```


#### By LineProtocol

Write Line Protocol record into specified bucket:

```c#
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;

namespace Examples
{
    public static class WriteLineProtocol
    {
        private static readonly string Token = "";

        public static void Main()
        {
            using var client = new InfluxDBClient("http://localhost:8086", Token);

            //
            // Write Data
            //
            using (var writeApi = client.GetWriteApi())
            {
                //
                //
                // Write by LineProtocol
                //
                writeApi.WriteRecord("temperature,location=north value=60.0", WritePrecision.Ns,"bucket_name", "org_id");
            }
        }
    }
}
```

#### Using WriteApiAsync
```c#
using System;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;

namespace Examples
{
    public static class WriteApiAsyncExample
    {   
        [Measurement("temperature")]
        private class Temperature
        {
            [Column("location", IsTag = true)] public string Location { get; set; }

            [Column("value")] public double Value { get; set; }

            [Column(IsTimestamp = true)] public DateTime Time { get; set; }
        }
        
        public static async Task Main()
        {
            using var client = new InfluxDBClient("http://localhost:8086", 
                            "my-user", "my-password");

            //
            // Write Data
            //
            var writeApiAsync = client.GetWriteApiAsync();

            //
            //
            // Write by LineProtocol
            //
            await writeApiAsync.WriteRecordAsync("temperature,location=north value=60.0", WritePrecision.Ns,
                "my-bucket", "my-org");

            //
            //
            // Write by Data Point
            //               
            var point = PointData.Measurement("temperature")
                            .Tag("location", "west")
                            .Field("value", 55D)
                            .Timestamp(DateTime.UtcNow.AddSeconds(-10), WritePrecision.Ns);

            await writeApiAsync.WritePointAsync(point, "my-bucket", "my-org");

            //
            // Write by POCO
            //
            var temperature = new Temperature {Location = "south", Value = 62D, Time = DateTime.UtcNow};

            await writeApiAsync.WriteMeasurementAsync(temperature, WritePrecision.Ns, "my-bucket", "my-org");

            //
            // Check written data
            //
            var tables = await influxDbClient.GetQueryApi()
                            .QueryAsync("from(bucket:\"my-bucket\") |> range(start: 0)", "my-org");
            
            tables.ForEach(table =>
            {
                var fluxRecords = table.Records;
                fluxRecords.ForEach(record =>
                {
                    Console.WriteLine($"{record.GetTime()}: {record.GetValue()}");
                });
            });
        }
    }
}
```

#### Default Tags

Sometimes is useful to store same information in every measurement e.g. `hostname`, `location`, `customer`. 
The client is able to use static value, app settings or env variable as a tag value.

The expressions:
- `California Miner` - static value
- `${version}` - application settings
- `${env.hostname}` - environment property

##### Via Configuration file

In a [configuration file](#client-configuration-file) you are able to specify default tags by `tags` element.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <configSections>
        <section name="influx2" type="InfluxDB.Client.Configurations.Influx2, InfluxDB.Client" />
    </configSections>
    <appSettings>
        <add key="SensorVersion" value="v1.00"/>
    </appSettings>
    <influx2 url="http://localhost:8086"
             org="my-org"
             bucket="my-bucket"
             token="my-token"
             logLevel="BODY"
             timeout="10s">
        <tags>
            <tag name="id" value="132-987-655"/>
            <tag name="customer" value="California Miner"/>
            <tag name="hostname" value="${env.Hostname}"/>
            <tag name="sensor-version" value="${SensorVersion}"/>
        </tags>
    </influx2>
</configuration>
```

##### Via API

```c#
var options = new InfluxDBClientOptions(Url)
{
    Token = token,
    DefaultTags = new Dictionary<string, string>
    {
        {"id", "132-987-655"},
        {"customer", "California Miner"},
    }
};   
options.AddDefaultTag("hostname", "${env.Hostname}")
options.AddDefaultTags(new Dictionary<string, string>{{ "sensor-version", "${SensorVersion}" }})
```

Both of configurations will produce the Line protocol:

```
mine-sensor,id=132-987-655,customer="California Miner",hostname=example.com,sensor-version=v1.00 altitude=10
```

### Handle the Events

Events that can be handle by WriteAPI EventHandler are:
- `WriteSuccessEvent` - for success response from server
- `WriteErrorEvent` - for unhandled exception from server
- `WriteRetriableErrorEvent` - for retriable error from server
- `WriteRuntimeExceptionEvent` - for runtime exception in background batch processing

Number of events depends on number of data points to collect in batch. The batch size is configured by `BatchSize` option (default size is `1000`) - in case
of one data point, event is handled for each point, independently on used writing method (even for mass writing of data like
`WriteMeasurements`, `WritePoints` and `WriteRecords`).

Events can be handled by register `writeApi.EventHandler` or by creating custom `EventListener`:

#### Register EventHandler

```c#
writeApi.EventHandler += (sender, eventArgs) =>
{
    switch (eventArgs)
    {
        case WriteSuccessEvent successEvent:
            string data = @event.LineProtocol;
            //
            // handle success response from server
            // Console.WriteLine($"{data}");
            //
            break;
        case WriteErrorEvent error:
            string data = @error.LineProtocol;
            string errorMessage = @error.Exception.Message;
            //
            // handle unhandled exception from server
            //
            // Console.WriteLine($"{data}");
            // throw new Exception(errorMessage);
            //
            break;
        case WriteRetriableErrorEvent error:
            string data = @error.LineProtocol;
            string errorMessage = @error.Exception.Message;
            //
            // handle retrievable error from server
            //
            // Console.WriteLine($"{data}");
            // throw new Exception(errorMessage);
            //
            break;
        case WriteRuntimeExceptionEvent error:
            string errorMessage = @error.Exception.Message;
            //
            // handle runtime exception in background batch processing
            // throw new Exception(errorMessage);
            //
            break;
    }
};

//
// Write by LineProtocol
//
writeApi.WriteRecord("influxPoint,writeType=lineProtocol value=11.11" +
    $" {DateTime.UtcNow.Subtract(EpochStart).Ticks * 100}", WritePrecision.Ns, "my-bucket", "my-org");
```

#### Custom EventListener

Advantage of using custom Event Listener is possibility of waiting on handled event between different writings - for more info see [EventListener](/Examples/WriteEventHandlerExample.cs#L234).

## Delete Data

Delete data from specified bucket:

```c#
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;

namespace Examples
{
    public static class WriteLineProtocol
    {
        private static readonly string Token = "";

        public static void Main()
        {
            using var client = new InfluxDBClient("http://localhost:8086", Token);

            //
            // Delete data
            //
            await client.GetDeleteApi().Delete(DateTime.UtcNow.AddMinutes(-1), DateTime.Now, "", "bucket", "org");
        }
    }
}
```

## Filter trace verbose

You can filter out verbose messages from `InfluxDB.Client` by using TraceListener.

```cs
using System;
using System.Diagnostics;
using InfluxDB.Client.Core;

namespace Examples
{
  public static class MyProgram
  {
    public static void Main()
    {
      TraceListener ConsoleOutListener = new TextWriterTraceListener(Console.Out)
      {
        Filter = CategoryTraceFilter.SuppressInfluxVerbose(),
      };
      Trace.Listeners.Add(ConsoleOutListener);

      // My code ...
    }
  }
}
````

## Management API

The client has following management API:

| API endpoint | Description | Implementation |
| --- | --- | --- |
| **/api/v2/authorizations** | Managing authorization data | [AuthorizationsApi](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/AuthorizationsApi.cs#L1) |
| **/api/v2/buckets** | Managing bucket data | [BucketsApi](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/BucketsApi.cs#L1) |
| **/api/v2/orgs** | Managing organization data | [OrganizationsApi](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/OrganizationsApi.cs#L1) |
| **/api/v2/users** | Managing user data | [UsersApi](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/UsersApi.cs#L1) |
| **/api/v2/sources** | Managing sources | [SourcesApi](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/SourcesApi.cs#L1) |
| **/api/v2/tasks** | Managing one-off and recurring tasks | [TasksApi](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/TasksApi.cs#L1) |
| **/api/v2/scrapers** | Managing ScraperTarget data | [ScraperTargetsApi](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/ScraperTargetsApi.cs#L1) |
| **/api/v2/labels** | Managing resource labels | [LabelsApi](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/LabelsApi.cs#L1) |
| **/api/v2/telegrafs** | Managing telegraf config data | [TelegrafsApi](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/TelegrafsApi.cs#L1) |
| **/api/v2/setup** | Managing onboarding setup | [InfluxDBClient#OnBoarding()](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/InfluxDBClient.cs#L191-) |
| **/ready** | Get the readiness of a instance at startup| [InfluxDBClient#Ready()](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/InfluxDBClient.cs#L169--) |
| **/health** | Get the health of an instance anytime during execution | [InfluxDBClient#Health()](https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/InfluxDBClient.cs#L160--) |

The following example demonstrates how to use a InfluxDB 2.x Management API. For further information see endpoints implementation.

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
        public static async Task Main()
        {
            const string url = "http://localhost:8086";
            const string token = "my-token";
            const string org = "my-org";
            
            using var client = new InfluxDBClient(url, token);

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
            var resource = new PermissionResource(PermissionResource.TypeBuckets, bucket.Id, null,
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

If there is no API implementation for particular service you could create the service by:

```c#
var dbrpService = _client.CreateService<DBRPsService>(typeof(DBRPsService));
```

## Advanced Usage

### Monitoring & Alerting

The example below show how to create a check for monitoring a stock price. A Slack notification is created if the price is lesser than `35`.

##### Create Threshold Check

The Check set status to `Critical` if the `current` value for a `stock` measurement is lesser than `35`.

```c#   
var org = ...;

var query = "from(bucket: \"my-bucket\") "
        + "|> range(start: v.timeRangeStart, stop: v.timeRangeStop)  "
        + "|> filter(fn: (r) => r._measurement == \"stock\")  "
        + "|> filter(fn: (r) => r.company == \"zyz\")  "
        + "|> aggregateWindow(every: 5s, fn: mean)  "
        + "|> filter(fn: (r) => r._field == \"current\")  "
        + "|> yield(name: \"mean\")";

var threshold = new LesserThreshold(value: 35F, level: CheckStatusLevel.CRIT,
                type: LesserThreshold.TypeEnum.Lesser);

var message = "The Stock price for XYZ is on: ${ r._level } level!";

await Client
    .GetChecksApi()
    .CreateThresholdCheckAsync("XYZ Stock value", query, "5s", message, threshold, org.Id);
```  

##### Create Slack Notification endpoint

```c#
var url = "https://hooks.slack.com/services/x/y/z"; 

var endpoint = await Client
    .GetNotificationEndpointsApi()
    .CreateSlackEndpointAsync("Slack Endpoint", url, org.Id);
```

##### Create Notification Rule

```c#
await Client
    .GetNotificationRulesApi()
    .CreateSlackRuleAsync("Critical status to Slack", "10s", "${ r._message }", RuleStatusLevel.CRIT, endpoint, org.Id);
```

### Custom mapping of DomainObject to/from InfluxDB

The [default mapper](/Client/Internal/DefaultDomainObjectMapper.cs) uses [Column](#by-poco) attributes to define how the DomainObject will be mapped `to` and `from` the InfluxDB.
The our APIs also allow to specify custom mapper. For more information see following example:

```c#
using System;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Writes;

namespace Examples
{
    public static class CustomDomainMapping
    {
        /// <summary>
        /// Define Domain Object
        /// </summary>
        private class Sensor
        {
            /// <summary>
            /// Type of sensor.
            /// </summary>
            public String Type { get; set; }
            
            /// <summary>
            /// Version of sensor.
            /// </summary>
            public String Version { get; set; }

            /// <summary>
            /// Measured value.
            /// </summary>
            public double Value { get; set; }

            public DateTimeOffset Timestamp { get; set; }

            public override string ToString()
            {
                return $"{Timestamp:MM/dd/yyyy hh:mm:ss.fff tt} {Type}, {Version} value: {Value}";
            }
        }

        /// <summary>
        /// Define Custom Domain Object Converter
        /// </summary>
        private class DomainEntityConverter : IDomainObjectMapper
        {
            /// <summary>
            /// Convert to DomainObject.
            /// </summary>
            public object ConvertToEntity(FluxRecord fluxRecord, Type type)
            {
                if (type != typeof(Sensor))
                {
                    throw new NotSupportedException($"This converter doesn't supports: {type}");
                }

                var customEntity = new Sensor
                {
                    Type = Convert.ToString(fluxRecord.GetValueByKey("type")),
                    Version = Convert.ToString(fluxRecord.GetValueByKey("version")),
                    Value = Convert.ToDouble(fluxRecord.GetValueByKey("data")),
                    Timestamp = fluxRecord.GetTime().GetValueOrDefault().ToDateTimeUtc(),
                };
                
                return Convert.ChangeType(customEntity, type);
            }
            
            /// <summary>
            /// Convert to DomainObject.
            /// </summary>
            public T ConvertToEntity<T>(FluxRecord fluxRecord)
            {
                return (T)ConvertToEntity(fluxRecord, typeof(T));
            }

            /// <summary>
            /// Convert to Point
            /// </summary>
            public PointData ConvertToPointData<T>(T entity, WritePrecision precision)
            {
                if (!(entity is Sensor sensor))
                {
                    throw new NotSupportedException($"This converter doesn't supports: {entity}");
                }

                var point = PointData
                    .Measurement("sensor")
                    .Tag("type", sensor.Type)
                    .Tag("version", sensor.Version)
                    .Field("data", sensor.Value)
                    .Timestamp(sensor.Timestamp, precision);

                return point;
            }
        }

        public static async Task Main(string[] args)
        {
            const string host = "http://localhost:9999";
            const string token = "my-token";
            const string bucket = "my-bucket";
            const string organization = "my-org";
            var options = new InfluxDBClientOptions(host)
            {
                Token = token,
                Org = organization,
                Bucket = bucket
            };

            var converter = new DomainEntityConverter();
            using var client = new InfluxDBClient(options);

            //
            // Prepare data to write
            //
            var time = new DateTimeOffset(2020, 11, 15, 8, 20, 15,
                new TimeSpan(3, 0, 0));

            var entity1 = new Sensor
            {
                Timestamp = time,
                Type = "temperature",
                Version = "v0.0.2",
                Value = 15
            };
            var entity2 = new Sensor
            {
                Timestamp = time.AddHours(1),
                Type = "temperature",
                Version = "v0.0.2",
                Value = 15
            };
            var entity3 = new Sensor
            {
                Timestamp = time.AddHours(2),
                Type = "humidity",
                Version = "v0.13",
                Value = 74
            };
            var entity4 = new Sensor
            {
                Timestamp = time.AddHours(3),
                Type = "humidity",
                Version = "v0.13",
                Value = 82
            };

            //
            // Write data
            //
            await client.GetWriteApiAsync(converter)
                .WriteMeasurementsAsync(new []{entity1, entity2, entity3, entity4}, WritePrecision.S);

            //
            // Query Data to Domain object
            //
            var queryApi = client.GetQueryApiSync(converter);

            //
            // Select ALL
            //
            var query = $"from(bucket:\"{bucket}\") " +
                        "|> range(start: 0) " +
                        "|> filter(fn: (r) => r[\"_measurement\"] == \"sensor\")" +
                        "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";
           
            var sensors = queryApi.QuerySync<Sensor>(query);
            //
            // Print result
            //
            sensors.ForEach(it => Console.WriteLine(it.ToString()));
        }
    }
}
```

- sources: [CustomDomainMapping.cs](/Examples/CustomDomainMapping.cs)

### Client configuration file

A client can be configured via `App.config` file.

The following options are supported:

| Property name      | default  | description                                             |
|--------------------|----------|---------------------------------------------------------| 
| Url                | -        | the url to connect to InfluxDB                          |
| Org                | -        | default destination organization for writes and queries |
| Bucket             | -        | default destination bucket for writes                   |
| Token              | -        | the token to use for the authorization                  |
| LogLevel           | NONE     | rest client verbosity level                             |
| Timeout            | 10000 ms | The timespan to wait before the HTTP request times out  |
| AllowHttpRedirects | false    | Configure automatically following HTTP 3xx redirects    |
| VerifySsl          | true     | Ignore Certificate Validation Errors when false         |

The `Timeout` supports `ms`, `s` and `m` as unit. Default is milliseconds.


##### Configuration example

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <configSections>
        <section name="influx2" type="InfluxDB.Client.Configurations.Influx2, InfluxDB.Client" />
    </configSections>

    <influx2 url="http://localhost:8086"
             org="my-org"
             bucket="my-bucket"
             token="my-token"
             logLevel="BODY"
             timeout="10s">
    </influx2>
</configuration>
```

and then:

```c#
var client = InfluxDBClientFactory.Create();
```

### Client connection string

A client can be constructed using a connection string that can contain the InfluxDBClientOptions parameters encoded into the URL.  
 
```c#
var client = new InfluxDBClient("http://localhost:8086?timeout=5000&logLevel=BASIC");
```
The following options are supported:

| Property name      | default  | description                                              |
|--------------------|----------|----------------------------------------------------------| 
| org                | -        | default destination organization for writes and queries  |
| bucket             | -        | default destination bucket for writes                    |
| token              | -        | the token to use for the authorization                   |
| logLevel           | NONE     | rest client verbosity level                              |
| timeout            | 10000 ms | The timespan to wait before the HTTP request times out.  |
| allowHttpRedirects | false    | Configure automatically following HTTP 3xx redirects     |
| verifySsl          | true     | Ignore Certificate Validation Errors when `false`        |

The `timeout` supports `ms`, `s` and `m` as unit. Default is milliseconds.

### Gzip support
`InfluxDBClient` does not enable gzip compress for http requests by default. If you want to enable gzip to reduce transfer data's size, you can call:

```c#
influxDBClient.EnableGzip();
```

### How to use WebProxy

You can configure the client to tunnel requests through an HTTP proxy. The `WebProxy` could be
configured via `InfluxDBClientOptions` parameter `WebProxy`:

```c#
var options = new InfluxDBClientOptions("http://localhost:8086")
{
    Token = "my-token",
    WebProxy = new WebProxy("http://proxyserver:80/", true)
};

var client = new InfluxDBClient(options);
```

### Redirects configuration

Client automatically **doesn't** follows HTTP redirects. You can enable redirects by `AllowRedirects` configuration option:

```csharp
var options = new InfluxDBClientOptions("http://localhost:8086")
{
    Token = "my-token",
    AllowRedirects = true
};

using var client = new InfluxDBClient(options);
```

> :warning: Due to a security reason `Authorization` header is not forwarded when redirect leads to a different domain.
> You can create custom `Authenticator` which change this behaviour - [see more](https://stackoverflow.com/a/28285735/1953325).

#### Log HTTP Request and Response

The Requests and Responses can be logged by changing the LogLevel. LogLevel values are None, Basic, Headers, Body. Note that 
applying the `Body` LogLevel will disable chunking while streaming and will load the whole response into memory.  

```c#
client.SetLogLevel(LogLevel.Body)
```

#### Check the server status and version

Server availability can be checked using the `influxDBClient.PingAsync()` endpoint.
 
## Version

The latest package for .NET CLI:
```bash
dotnet add package InfluxDB.Client
```
  
Or when using with Package Manager:
```bash
Install-Package InfluxDB.Client
```