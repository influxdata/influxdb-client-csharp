# InfluxDB.Client.Linq

The library supports to use a LINQ expression to query the InfluxDB.

#### Disclaimer: This library is a work in progress and should not be considered production ready yet.

## How to start

First, add the library as a dependency for your project:

```bash
# For actual version please check: https://www.nuget.org/packages/InfluxDB.Client.Linq/

dotnet add package InfluxDB.Client.Linq --version 1.15.0-dev.linq.1
```

Next, you should add additional using statement to your program:

```c#
using InfluxDB.Client.Linq;
```

The LINQ query depends on `QueryApi`, you could create an instance of `QueryApi` by:

```c#
var client = InfluxDBClientFactory.Create("http://localhost:8086", "my-token");
var queryApi = client.GetQueryApi();
```

In the following examples we assume that the `Sensor` entity is defined as:

```c#
class Sensor
{
    [Column("sensor_id", IsTag = true)] public string SensorId { get; set; }

    /// <summary>
    /// "production" or "testing"
    /// </summary>
    [Column("deployment", IsTag = true)]
    public string Deployment { get; set; }

    /// <summary>
    /// Value measured by sensor
    /// </summary>
    [Column("data")]
    public float Value { get; set; }

    [Column(IsTimestamp = true)] public DateTime Timestamp { get; set; }
}
```

## Time Series

The InfluxDB uses concept of TimeSeries - a collection of data that shares a measurement, tag set, and bucket. 
You always operate on each time-series, if you querying data with Flux. 

Imagine that you have following data:

```
sensor,deployment=production,sensor_id=id-1 data=15
sensor,deployment=testing,sensor_id=id-1 data=28
sensor,deployment=testing,sensor_id=id-1 data=12
sensor,deployment=production,sensor_id=id-1 data=89
```

The corresponding time series are:
- `sensor,deployment=production,sensor_id=id-1`
- `sensor,deployment=testing,sensor_id=id-1`

If you query your data with following Flux:

```flux
from(bucket: "my-bucket")
  |> range(start: 0)
  |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
  |> limit(n:1)
```

The result will be one item for each time-series:

```
sensor,deployment=production,sensor_id=id-1 data=15
sensor,deployment=testing,sensor_id=id-1 data=28
```

and this is also way how following LINQ operators works.

- [series](https://docs.influxdata.com/influxdb/v2.0/reference/glossary/#series)
- [Flux](https://docs.influxdata.com/influxdb/v2.0/reference/glossary/#flux)
- [Query data with Flux](https://docs.influxdata.com/influxdb/v2.0/query-data/flux/)

## Perform Query

The LINQ query requires `bucket` and `organization` as a source of data. Both of them could be name or ID.

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    select s;

var sensors = query.ToList();
```

## Supported LINQ operators

### Take

```c#
var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    select s)
    .Take(10);
```

Flux Query:

```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> limit(n: 10)
```

### Skip

```c#
var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    select s)
    .Take(10)
    .Skip(50);
```

Flux Query:

```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> limit(n: 10, offset: 50)
```

### Equality

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.SensorId == "id-1"
    select s;
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> filter(fn: (r) => (r["sensor_id"] == "id-1")) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
```