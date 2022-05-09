# InfluxDB.Client.Linq

The library supports to use a LINQ expression to query the InfluxDB.

## Documentation

This section contains links to the client library documentation.

* [Product documentation](https://docs.influxdata.com/influxdb/latest/api-guide/client-libraries/), [Getting Started](#how-to-start)
* [Examples](../Examples)
* [API Reference](https://influxdata.github.io/influxdb-client-csharp/api/InfluxDB.Client.Linq.InfluxDBQueryable-1.html)
* [Changelog](../CHANGELOG.md)

## Usage

- [How to start](#how-to-start)
- [Time Series](#time-series)
    - [Enable querying multiple time-series](#enable-querying-multiple-time-series)
    - [Client Side Evaluation](#client-side-evaluation)
- [Perform Query](#perform-query)
- [Filtering](#filtering)
    - [Mapping LINQ filters](#mapping-linq-filters)
    - [Time Range Filtering](#time-range-filtering)
- [Supported LINQ operators](#supported-linq-operators)
    - [Equal](#equal)
    - [Not Equal](#not-equal)
    - [Less Than](#less-than)
    - [Less Than Or Equal](#less-than-or-equal)
    - [Greater Than](#greater-than)
    - [Greater Than Or Equal](#greater-than-or-equal)
    - [And](#and)
    - [Or](#or)
    - [Any](#any)
    - [Take](#take)
    - [TakeLast](#takelast)
    - [Skip](#skip)
    - [OrderBy](#orderby)
    - [Count](#count)
    - [LongCount](#longcount)
    - [Contains](#contains)
- [Custom LINQ operators](#custom-linq-operators)
    - [AggregateWindow](#aggregatewindow)
- [Domain Converter](#domain-converter)
- [How to debug output Flux Query](#how-to-debug-output-flux-query)
- [How to filter by Measurement](#how-to-filter-by-measurement)
- [Asynchronous Queries](#asynchronous-queries)

## How to start

First, add the library as a dependency for your project:

```bash
# For actual version please check: https://www.nuget.org/packages/InfluxDB.Client.Linq/

dotnet add package InfluxDB.Client.Linq --version 1.17.0-dev.linq.17
```

Next, you should add additional using statement to your program:

```c#
using InfluxDB.Client.Linq;
```

The LINQ query depends on `QueryApiSync`, you could create an instance of `QueryApiSync` by:

```c#
var client = InfluxDBClientFactory.Create("http://localhost:8086", "my-token");
var queryApi = client.GetQueryApiSync();
```

In the following examples we assume that the `Sensor` entity is defined as:

```c#
class Sensor
{
    [Column("sensor_id", IsTag = true)] 
    public string SensorId { get; set; }

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

    [Column(IsTimestamp = true)] 
    public DateTime Timestamp { get; set; }
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
  |> drop(columns: ["_start", "_stop", "_measurement"])
  |> limit(n:1)
```

The result will be one item for each time-series:

```
sensor,deployment=production,sensor_id=id-1 data=15
sensor,deployment=testing,sensor_id=id-1 data=28
```

and this is also way how this LINQ driver works. 

**The driver supposes that you are querying over one time-series.** 

There is a way how to change this configuration:

### Enable querying multiple time-series

```c#
var settings = new QueryableOptimizerSettings{QueryMultipleTimeSeries = true};
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi, settings)
    select s;
```

The [group()](https://docs.influxdata.com/influxdb/cloud/reference/flux/stdlib/built-in/transformations/group/) function is way how to query multiple time-series and gets correct results.

The following query works correctly:

```flux
from(bucket: "my-bucket")
  |> range(start: 0)
  |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
  |> drop(columns: ["_start", "_stop", "_measurement"])
  |> group()
  |> limit(n:1)
```

and corresponding result:

```
sensor,deployment=production,sensor_id=id-1 data=15
```

Do not used this functionality if it is not required because **it brings a performance** costs caused by sorting: 

#### Group does not guarantee sort order

The `group()` [does not guarantee sort order](https://docs.influxdata.com/influxdb/cloud/reference/flux/stdlib/built-in/transformations/group/#group-does-not-guarantee-sort-order) of output records. 
To ensure data is sorted correctly, use `orderby` expression.

### Client Side Evaluation

The library attempts to evaluate a query on the server as much as possible.
The client side evaluations is required for aggregation function **if there is more then one time series.**

If you want to count your data with following Flux:

```flux
from(bucket: "my-bucket")
  |> range(start: 0)
  |> drop(columns: ["_start", "_stop", "_measurement"])
  |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
  |> stateCount(fn: (r) => true, column: "linq_result_column") 
  |> last(column: "linq_result_column") 
  |> keep(columns: ["linq_result_column"])
```

The result will be one count for each time-series:

```csv
#group,false,false,false
#datatype,string,long,long
#default,_result,,
,result,table,linq_result_column
,,0,1
,,0,1

```

and client has to aggregate this multiple results into one scalar value.

Operators that could cause client side evaluation:

- `Count`
- `CountLong`

### TL;DR

- [series](https://docs.influxdata.com/influxdb/cloud/reference/glossary/#series)
- [Flux](https://docs.influxdata.com/influxdb/cloud/reference/glossary/#flux)
- [Query data with Flux](https://docs.influxdata.com/influxdb/cloud/query-data/flux/)

## Perform Query

The LINQ query requires `bucket` and `organization` as a source of data. Both of them could be name or ID.

```c#
var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.SensorId == "id-1"
    where s.Value > 12
    where s.Timestamp > new DateTime(2019, 11, 16, 8, 20, 15, DateTimeKind.Utc)
    where s.Timestamp < new DateTime(2021, 01, 10, 5, 10, 0, DateTimeKind.Utc)
    orderby s.Timestamp
    select s)
    .Take(2)
    .Skip(2);

var sensors = query.ToList();
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 2019-11-16T08:20:15Z, stop: 2021-01-10T05:10:00Z) 
    |> filter(fn: (r) => (r["sensor_id"] == "id-1")) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> filter(fn: (r) => (r["data"] > 12)) 
    |> limit(n: 2, offset: 2)
```

## Filtering 

The [range()](https://docs.influxdata.com/influxdb/cloud/reference/flux/stdlib/built-in/transformations/range/) and [filter()](https://docs.influxdata.com/influxdb/cloud/reference/flux/stdlib/built-in/transformations/filter/) are `pushdown functions`
that allow push their data manipulation down to the underlying data source rather than storing and manipulating data in memory. 
Using pushdown functions at the beginning of query we greatly reduce the amount of server memory necessary to run a query.

The LINQ provider needs to aligns fields within each input table that have the same timestamp to column-wise format:

###### From
|              _time             | _value | _measurement | _field |
|:------------------------------:|:------:|:------------:|:------:|
| 1970-01-01T00:00:00.000000001Z |   1.0  |     "m1"     |  "f1"  |
| 1970-01-01T00:00:00.000000001Z |   2.0  |     "m1"     |  "f2"  |
| 1970-01-01T00:00:00.000000002Z |   3.0  |     "m1"     |  "f1"  |
| 1970-01-01T00:00:00.000000002Z |   4.0  |     "m1"     |  "f2"  |

###### To
|              _time             | _measurement |  f1  |  f2  |
|:------------------------------:|:------------:|:----:|:----:|
| 1970-01-01T00:00:00.000000001Z |     "m1"     |  1.0 |  2.0 |
| 1970-01-01T00:00:00.000000002Z |     "m1"     |  3.0 |  4.0 |

For that reason we need to use the [pivot()](https://docs.influxdata.com/influxdb/cloud/reference/flux/stdlib/built-in/transformations/pivot/) function.
The `pivot` is heavy and should be used at the end of our Flux query.

There is an also possibility to disable appending `pivot` by:

```c#
var optimizerSettings =
    new QueryableOptimizerSettings
    {
        AlignFieldsWithPivot = false
    };
    
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi, optimizerSettings)
    select s;
```

### Mapping LINQ filters

For the best performance on the both side - `server`, `LINQ provider` we maps the LINQ expressions to FLUX query following way:

#### Filter by Timestamp

Mapped to [range()](https://docs.influxdata.com/influxdb/cloud/reference/flux/stdlib/built-in/transformations/range/).

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.Timestamp >= new DateTime(2019, 11, 16, 8, 20, 15, DateTimeKind.Utc)
    select s;

var sensors = query.ToList();
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 2019-11-16T08:20:15ZZ) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
    |> drop(columns: ["_start", "_stop", "_measurement"])
```

#### Filter by Tag

Mapped to [filter()](https://docs.influxdata.com/influxdb/cloud/reference/flux/stdlib/built-in/transformations/filter/) **before** `pivot()`.

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
    |> drop(columns: ["_start", "_stop", "_measurement"])
```

#### Filter by Field

The filter by field has to be **after** the `pivot()` because we want to select all fields from pivoted table.

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.Value < 28
    select s;
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 0)
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")  
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> filter(fn: (r) => (r["data"] < 28))
```

If we move the `filter()` for **fields** before the `pivot()` then we will gets wrong results:

##### Data

```
m1 f1=1,f2=2 1
m1 f1=3,f2=4 2
```

##### Without filter

```flux
from(bucket: "my-bucket") 
    |> range(start: 0)
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
```

Results:

|              _time             |  f1  |  f2  |
|:------------------------------:|:----:|:----:|
| 1970-01-01T00:00:00.000000001Z |  1.0 |  2.0 |
| 1970-01-01T00:00:00.000000002Z |  3.0 |  4.0 |

##### Filter before pivot()

> filter: `f1 > 0`

```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> filter(fn: (r) => (r["_field"] == "f1" and r["_value"] > 0))
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
```
Results:

|              _time             |  f1  |
|:------------------------------:|:----:|
| 1970-01-01T00:00:00.000000001Z |  1.0 |
| 1970-01-01T00:00:00.000000002Z |  3.0 |

### Time Range Filtering

The time filtering expressions are mapped to Flux `range()` function. 
This function has `start` and `stop` parameters with following behaviour: `start <= _time < stop`:
> Results include records with `_time` values greater than or equal to the specified `start` time and less than the specified `stop` time.
 
This means that we have to add one nanosecond to `start` if we want timestamp `greater than` and also add one nanosecond to `stop` if we want to timestamp `lesser or equal than`.

- [range() function](https://docs.influxdata.com/influxdb/cloud/reference/flux/stdlib/built-in/transformations/range/)

#### Example 1:

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.Timestamp > new DateTime(2019, 11, 16, 8, 20, 15, DateTimeKind.Utc)
    where s.Timestamp < new DateTime(2021, 01, 10, 5, 10, 0, DateTimeKind.Utc)
    select s;

var sensors = query.ToList();
```

Flux Query:
```flux
start_shifted = int(v: time(v: "2019-11-16T08:20:15Z")) + 1

from(bucket: "my-bucket") 
    |> range(start: time(v: start_shifted), stop: 2021-01-10T05:10:00Z)
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
```

#### Example 2:

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.Timestamp >= new DateTime(2019, 11, 16, 8, 20, 15, DateTimeKind.Utc)
    where s.Timestamp <= new DateTime(2021, 01, 10, 5, 10, 0, DateTimeKind.Utc)
    select s;

var sensors = query.ToList();
```

Flux Query:
```flux
stop_shifted = int(v: time(v: "2021-01-10T05:10:00Z")) + 1

from(bucket: "my-bucket") 
    |> range(start: 2019-11-16T08:20:15Z, stop: time(v: stop_shifted)) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
    |> drop(columns: ["_start", "_stop", "_measurement"])
```

#### Example 3:

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.Timestamp >= new DateTime(2019, 11, 16, 8, 20, 15, DateTimeKind.Utc)
    select s;

var sensors = query.ToList();
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 2019-11-16T08:20:15ZZ) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
    |> drop(columns: ["_start", "_stop", "_measurement"])
```

#### Example 4:

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.Timestamp <= new DateTime(2021, 01, 10, 5, 10, 0, DateTimeKind.Utc)
    select s;

var sensors = query.ToList();
```

Flux Query:
```flux
stop_shifted = int(v: time(v: "2021-01-10T05:10:00Z")) + 1

from(bucket: "my-bucket") 
    |> range(start: 0, stop: time(v: stop_shifted))
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
```

#### Example 5:

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.Timestamp == new DateTime(2019, 11, 16, 8, 20, 15, DateTimeKind.Utc)
    select s;

var sensors = query.ToList();
```

Flux Query:
```flux
stop_shifted = int(v: time(v: "2019-11-16T08:20:15Z")) + 1

from(bucket: "my-bucket") 
    |> range(start: 2019-11-16T08:20:15Z, stop: time(v: stop_shifted)) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
    |> drop(columns: ["_start", "_stop", "_measurement"])
```

There is also a possibility to specify the default value for `start` and `stop` parameter. This is useful when you need to include data with future timestamps when no time bounds are explicitly set.

```c#
var settings = new QueryableOptimizerSettings
{
    RangeStartValue = DateTime.UtcNow.AddHours(-24),
    RangeStopValue = DateTime.UtcNow.AddHours(1)
};
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi, settings)
    select s;
```

### TD;LR

- [Optimize Flux queries](https://docs.influxdata.com/influxdb/cloud/query-data/optimize-queries/)

## Supported LINQ operators

### Equal

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
    |> drop(columns: ["_start", "_stop", "_measurement"])
```

### Not Equal

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.SensorId != "id-1"
    select s;
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 0)
    |> filter(fn: (r) => (r["sensor_id"] != "id-1")) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
    |> drop(columns: ["_start", "_stop", "_measurement"])
```

### Less Than

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.Value < 28
    select s;
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> filter(fn: (r) => (r["data"] < 28))
```

### Less Than Or Equal

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.Value <= 28
    select s;
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> filter(fn: (r) => (r["data"] <= 28))
```

### Greater Than

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.Value > 28
    select s;
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> filter(fn: (r) => (r["data"] > 28))
```

### Greater Than Or Equal

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.Value >= 28
    select s;
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> filter(fn: (r) => (r["data"] >= 28))
```

### And

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.Value >= 28 && s.SensorId != "id-1"
    select s;
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> filter(fn: (r) => (r["sensor_id"] != "id-1"))
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> filter(fn: (r) => (r["data"] >= 28))
```

### Or

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.Value >= 28 || s.Value <= 5
    select s;
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> filter(fn: (r) => ((r["data"] >= 28) or (r["data"] <=> 28)))
```

### Any

The following code demonstrates how to use the `Any` operator to determine whether a collection contains any elements.
By default the `InfluxDB.Client` doesn't supports to store a subcollection in your `DomainObject`.

Imagine that you have following entities:

```c#
class SensorCustom
{
    public Guid Id { get; set; }
    
    public float Data { get; set; }
    
    public DateTimeOffset Time { get; set; }
    
    public virtual ICollection<SensorAttribute> Attributes { get; set; }
}

class SensorAttribute
{
    public string Name { get; set; }
    public string Value { get; set; }
}
```

To be able to store `SensorCustom` entity in InfluxDB and retrieve it from database you should implement [IDomainObjectMapper](/Client/IDomainObjectMapper.cs). 
The converter tells to the Client how to map `DomainObject` into [PointData](/Client/Writes/PointData.cs) and how to map [FluxRecord](/Client.Core/Flux/Domain/FluxRecord.cs) to `DomainObject`.

Entity Converter:

```c#
private class SensorEntityConverter : IDomainObjectMapper
{
    //
    // Parse incoming FluxRecord to DomainObject
    //
    public T ConvertToEntity<T>(FluxRecord fluxRecord)
    {
        if (typeof(T) != typeof(SensorCustom))
        {
            throw new NotSupportedException($"This converter doesn't supports: {typeof(SensorCustom)}");
        }

        //
        // Create SensorCustom entity and parse `SeriesId`, `Value` and `Time`
        //
        var customEntity = new SensorCustom
        {
            Id = Guid.Parse(Convert.ToString(fluxRecord.GetValueByKey("series_id"))!),
            Data = Convert.ToDouble(fluxRecord.GetValueByKey("data")),
            Time = fluxRecord.GetTime().GetValueOrDefault().ToDateTimeUtc(),
            Attributes = new List<SensorAttribute>()
        };
        
        foreach (var (key, value) in fluxRecord.Values)
        {
            //
            // Parse SubCollection values
            //
            if (key.StartsWith("property_"))
            {
                var attribute = new SensorAttribute
                {
                    Name = key.Replace("property_", string.Empty), Value = Convert.ToString(value)
                };
                
                customEntity.Attributes.Add(attribute);
            }
        }

        return (T) Convert.ChangeType(customEntity, typeof(T));
    }

    //
    // Convert DomainObject into PointData
    //
    public PointData ConvertToPointData<T>(T entity, WritePrecision precision)
    {
        if (!(entity is SensorCustom ce))
        {
            throw new NotSupportedException($"This converter doesn't supports: {typeof(SensorCustom)}");
        }

        //
        // Map `SeriesId`, `Value` and `Time` to Tag, Field and Timestamp
        //
        var point = PointData
            .Measurement("custom_measurement")
            .Tag("series_id", ce.Id.ToString())
            .Field("data", ce.Data)
            .Timestamp(ce.Time, precision);

        //
        // Map subattributes to Fields
        //
        foreach (var attribute in ce.Attributes ?? new List<SensorAttribute>())
        {
            point = point.Field($"property_{attribute.Name}", attribute.Value);
        }

        return point;
    }
}
```

The `Converter` could be passed to [QueryApiSync](/Client/QueryApiSync.cs), [QueryApi](/Client/QueryApi.cs) or [WriteApi](/Client/WriteApi.cs) by:

```c#
// Create Converter
var converter = new SensorEntityConverter();

// Get Query and Write API
var queryApi = client.GetQueryApiSync(converter);
var writeApi = client.GetWriteApi(converter);
```

The LINQ provider needs to know how properties of `DomainObject` are stored in InfluxDB - their name and type (tag, field, timestamp). 

If you use a [IDomainObjectMapper](/Client/IDomainObjectMapper.cs) instead of [InfluxDB Attributes](/Client.Core/Attributes.cs) you should implement [IMemberNameResolver](/Client.Linq/IMemberNameResolver.cs):

```c#
private class SensorMemberResolver: IMemberNameResolver
{
    //
    // Tell to LINQ providers how is property of DomainObject mapped - Tag, Field, Timestamp, ... ?
    //
    public MemberType ResolveMemberType(MemberInfo memberInfo)
    {
        //
        // Mapping of subcollection
        //
        if (memberInfo.DeclaringType == typeof(SensorAttribute))
        {
            return memberInfo.Name switch
            {
                "Name" => MemberType.NamedField,
                "Value" => MemberType.NamedFieldValue,
                _ => MemberType.Field
            };
        }

        //
        // Mapping of "root" domain
        //
        return memberInfo.Name switch
        {
            "Time" => MemberType.Timestamp,
            "Id" => MemberType.Tag,
            _ => MemberType.Field
        };
    }

    //
    // Tell to LINQ provider how is property of DomainObject named 
    //
    public string GetColumnName(MemberInfo memberInfo)
    {
        return memberInfo.Name switch
        {
            "Id" => "series_id",
            "Data" => "data",
            _ => memberInfo.Name
        };
    }

    //
    // Tell to LINQ provider how is named property that is flattened
    //
    public string GetNamedFieldName(MemberInfo memberInfo, object value)
    {
        return "attribute_" + Convert.ToString(value);
    }
}
```

Now We are able to provide a required information to the LINQ provider by `memberResolver` parameter:

```c#
var memberResolver = new SensorMemberResolver();

var query = from s in InfluxDBQueryable<SensorCustom>.Queryable("my-bucket", "my-org", queryApi, memberResolver)
    where s.Attributes.Any(a => a.Name == "quality" && a.Value == "good")
    select s;
```

Flux Query:

```flux
from(bucket: "my-bucket")
    |> range(start: 0)
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> filter(fn: (r) => (r["attribute_quality"] == "good"))
```

For more info see [CustomDomainMappingAndLinq](/Examples/CustomDomainMappingAndLinq.cs) example.

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
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> limit(n: 10)
```

**_Note:_** the `limit()` function can be align before `pivot()` function by:

```c#
var optimizerSettings =
    new QueryableOptimizerSettings
    {
        AlignLimitFunctionAfterPivot = false
    };
```

**_Performance:_** The `pivot()` is a [“heavy” function](https://docs.influxdata.com/influxdb/cloud/query-data/optimize-queries/#use-heavy-functions-sparingly). Using `limit()` before `pivot()` is much faster but works only if you have consistent data series. See [#318](https://github.com/influxdata/influxdb-client-csharp/issues/318) for more details.

### TakeLast

```c#
var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    select s)
    .TakeLast(10);
```

Flux Query:

```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> tail(n: 10)
```

**_Note:_** the `tail()` function can be align before `pivot()` function by:

```c#
var optimizerSettings =
    new QueryableOptimizerSettings
    {
        AlignLimitFunctionAfterPivot = false
    };
```
**_Performance:_** The `pivot()` is a [“heavy” function](https://docs.influxdata.com/influxdb/cloud/query-data/optimize-queries/#use-heavy-functions-sparingly). Using `tail()` before `pivot()` is much faster but works only if you have consistent data series. See [#318](https://github.com/influxdata/influxdb-client-csharp/issues/318) for more details.

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
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> limit(n: 10, offset: 50)
```

### OrderBy

#### Example 1:

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    orderby s.Deployment
    select s;
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> sort(columns: ["deployment"], desc: false)
```

#### Example 2:

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    orderby s.Timestamp descending 
    select s;
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> sort(columns: ["_time"], desc: true)
```

### Count

> Possibility of partial [client side evaluation](#client-side-evaluation)

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    select s;

var sensors = query.Count();
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> stateCount(fn: (r) => true, column: "linq_result_column") 
    |> last(column: "linq_result_column") 
    |> keep(columns: ["linq_result_column"])
```

### LongCount

> Possibility of partial [client side evaluation](#client-side-evaluation)

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    select s;

var sensors = query.LongCount();
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 0)
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> stateCount(fn: (r) => true, column: "linq_result_column") 
    |> last(column: "linq_result_column") 
    |> keep(columns: ["linq_result_column"])
```

### Contains

```c#
int[] values = {15, 28};

var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where values.Contains(s.Value)
    select s;

var sensors = query.Count();
```

Flux Query:
```flux
from(bucket: "my-bucket")
    |> range(start: 0)
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
    |> drop(columns: ["_start", "_stop", "_measurement"])
    |> filter(fn: (r) => contains(value: r["data"], set: [15, 28]))
```

## Custom LINQ operators

### AggregateWindow

The `AggregateWindow` applies an aggregate function to fixed windows of time. 
Can be used only for a field which is defined as `timestamp` - `[Column(IsTimestamp = true)]`. 
For more info about `aggregateWindow() function` see Flux's documentation - https://docs.influxdata.com/flux/v0.x/stdlib/universe/aggregatewindow/.

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.Timestamp.AggregateWindow(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(40), "mean")
    select s;
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> aggregateWindow(every: 20s, period: 40s, fn: mean) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> drop(columns: ["_start", "_stop", "_measurement"])
```

## Domain Converter

There is also possibility to use custom domain converter to transform data from/to your `DomainObject`.

Instead of following Influx attributes:

```c#
[Measurement("temperature")]
private class Temperature
{
    [Column("location", IsTag = true)] public string Location { get; set; }

    [Column("value")] public double Value { get; set; }

    [Column(IsTimestamp = true)] public DateTime Time { get; set; }
}
```

you could create own instance of `IDomainObjectMapper` and use it with `QueryApiSync`, `QueryApi` and `WriteApi`.

```c#
var converter = new DomainEntityConverter();
var queryApi = client.GetQueryApiSync(converter)
```

To satisfy LINQ Query Provider you have to implement `IMemberNameResolver`:

```c#
var resolver = new MemberNameResolver();

var query = from s in InfluxDBQueryable<SensorCustom>.Queryable("my-bucket", "my-org", queryApi, nameResolver)
    where s.Attributes.Any(a => a.Name == "quality" && a.Value == "good")
    select s;
```

for more details see [Any](#any) operator and for full example see: [CustomDomainMappingAndLinq](/Examples/CustomDomainMappingAndLinq.cs#L54).

## How to debug output Flux Query

```c#
var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
        where s.SensorId == "id-1"
        where s.Value > 12
        where s.Timestamp > new DateTime(2019, 11, 16, 8, 20, 15, DateTimeKind.Utc)
        where s.Timestamp < new DateTime(2021, 01, 10, 5, 10, 0, DateTimeKind.Utc)
        orderby s.Timestamp
        select s)
    .Take(2)
    .Skip(2);
    
Console.WriteLine("==== Debug LINQ Queryable Flux output ====");
var influxQuery = ((InfluxDBQueryable<Sensor>) query).ToDebugQuery();
foreach (var statement in influxQuery.Extern.Body)
{
    var os = statement as OptionStatement;
    var va = os?.Assignment as VariableAssignment;
    var name = va?.Id.Name;
    var value = va?.Init.GetType().GetProperty("Value")?.GetValue(va.Init, null);

    Console.WriteLine($"{name}={value}");
}
Console.WriteLine();
Console.WriteLine(influxQuery._Query);
```

## How to filter by Measurement

By default, as an optimization step, ***Flux queries generated by LINQ will automatically drop the Start, Stop and Measurement columns***:

```flux
from(bucket: "my-bucket")
  |> range(start: 0)
  |> drop(columns: ["_start", "_stop", "_measurement"])
  ...
```
This is because typical POCO classes do not include them:
```c#
[Measurement("temperature")]
private class Temperature
{
    [Column("location", IsTag = true)] public string Location { get; set; }
    [Column("value")] public double Value { get; set; }
    [Column(IsTimestamp = true)] public DateTime Time { get; set; }
}
```

It is, however, possible to utilize the Measurement column in LINQ queries by enabling it in the query optimization settings:

```c#
var optimizerSettings =
    new QueryableOptimizerSettings
    {
        DropMeasurementColumn = false,
        
        // Note we can also enable the start and stop columns
        //DropStartColumn = false,
        //DropStopColumn = false
    };

var queryable =
    new InfluxDBQueryable<InfluxPoint>("my-bucket", "my-org", queryApi, new DefaultMemberNameResolver(), optimizerSettings);

var latest =
    await queryable.Where(p => p.Measurement == "temperature")
                   .OrderByDescending(p => p.Time)
                   .ToInfluxQueryable()
                   .GetAsyncEnumerator()
                   .FirstOrDefaultAsync();

private class InfluxPoint
{
    [Column(IsMeasurement = true)] public string Measurement { get; set; }
    [Column("location", IsTag = true)] public string Location { get; set; }
    [Column("value")] public double Value { get; set; }
    [Column(IsTimestamp = true)] public DateTime Time { get; set; }
}
```

## Asynchronous Queries

The LINQ driver also supports asynchronous querying. For asynchronous queries you have to initialize `InfluxDBQueryable` with asynchronous version of [QueryApi](/Client/QueryApi.cs) and transform `IQueryable<T>` to `IAsyncEnumerable<T>`:

```c#
var client = InfluxDBClientFactory.Create("http://localhost:8086", "my-token");
var queryApi = client.GetQueryApi();

var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    select s;

IAsyncEnumerable<Sensor> enumerable = query
    .ToInfluxQueryable()
    .GetAsyncEnumerator();
```
