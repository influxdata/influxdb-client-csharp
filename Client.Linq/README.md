# InfluxDB.Client.Linq

The library supports to use a LINQ expression to query the InfluxDB.

#### Disclaimer: This library is a work in progress and should not be considered production ready yet.

- [How to start](#how-to-start)
- [Time Series](#time-series)
- [Perform Query](#perform-query)
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
    - [Skip](#skip)
    - [OrderBy](#orderby)
- [Domain Converter](#domain-converter)

## How to start

First, add the library as a dependency for your project:

```bash
# For actual version please check: https://www.nuget.org/packages/InfluxDB.Client.Linq/

dotnet add package InfluxDB.Client.Linq --version 1.15.0-dev.linq.5
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
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
    |> filter(fn: (r) => (r["sensor_id"] == "id-1") and (r["data"] > 12)) 
    |> sort(columns: ["_time"], desc: false) 
    |> limit(n: 2, offset: 2)
```

## Time Range Filtering

The time filtering expressions are mapped to Flux `range()` function. 
This function has `start` and `stop` parameters with following behaviour: `start <= _time < stop`:
> Results include records with `_time` values greater than or equal to the specified `start` time and less than the specified `stop` time.
 
This means that doesn't matter if filtering expression has `less than` or `less than or equal` operator (same for `greater`), because `range()` function has different behaviour.
As a solution you could shift your time with one nanoseconds. 

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
from(bucket: "my-bucket") 
    |> range(start: 2019-11-16T08:20:15Z, stop: 2021-01-10T05:10:00Z) 
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
from(bucket: "my-bucket") 
    |> range(start: 2019-11-16T08:20:15Z, stop: 2021-01-10T05:10:00Z) 
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
from(bucket: "my-bucket") 
    |> range(start: 0, stop: 2021-01-10T05:10:00Z) 
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
from(bucket: "my-bucket") 
    |> range(start: 2019-11-16T08:20:15Z, stop: 2019-11-16T08:20:15Z) 
```

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
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
    |> filter(fn: (r) => (r["sensor_id"] == "id-1")) 
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
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
    |> filter(fn: (r) => (r["sensor_id"] != "id-1")) 
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
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> filter(fn: (r) => ((r["data"] >= 28) and (r["sensor_id"] != "id-1")))
```

### Or

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    where s.Value >= 28 || s.SensorId != "id-1"
    select s;
```

Flux Query:
```flux
from(bucket: "my-bucket") 
    |> range(start: 0) 
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> filter(fn: (r) => ((r["data"] >= 28) or (r["sensor_id"] != "id-1")))
```

### Any

The following code demonstrates how to use the `Any` operator to determine whether a collection contains any elements.
By default the `InfluxDB.Client` doesn't supports to store a subcollection in your `DomainObject`.

Imagine that you have following entities:

```c#
class SensorCustom
{
    public Guid Id { get; set; }
    
    public float Value { get; set; }
    
    public DateTimeOffset Time { get; set; }
    
    public virtual ICollection<SensorAttribute> Attributes { get; set; }
}

class SensorAttribute
{
    public string Name { get; set; }
    public string Value { get; set; }
}
```

To be able to store `SensorCustom` entity in InfluxDB and retrieve it from database you should implement [IInfluxDBEntityConverter](/Client/IInfluxDBEntityConverter.cs). 
The converter tells to the Client how to map `DomainObject` into [PointData](/Client/Writes/PointData.cs) and how to map [FluxRecord](/Client.Core/Flux/Domain/FluxRecord.cs) to `DomainObject`.

Entity Converter:

```c#
private class SensorEntityConverter : IInfluxDBEntityConverter
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
            Value = Convert.ToDouble(fluxRecord.GetValueByKey("data")),
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
            .Field("data", ce.Value)
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

The `Converter` could be passed to [QueryApi](/Client/QueryApi.cs) or [WriteApi](/Client/WriteApi.cs) by:

```c#
// Create Converter
var converter = new SensorEntityConverter();

// Get Query and Write API
var queryApi = client.GetQueryApi(converter);
var writeApi = client.GetWriteApi(converter);
```

Name Resolver:

```c#
private class MemberNameResolver: IMemberNameResolver
{
    public MemberType ResolveMemberType(MemberInfo memberInfo)
    {
        if (memberInfo.DeclaringType == typeof(SensorAttribute))
        {
            return memberInfo.Name switch
            {
                "Name" => MemberType.NamedField,
                "Value" => MemberType.NamedFieldValue,
                _ => MemberType.Field
            };
        }

        return memberInfo.Name switch
        {
            "Time" => MemberType.Timestamp,
            "Id" => MemberType.Tag,
            _ => MemberType.Field
        };
    }

    public string GetColumnName(MemberInfo memberInfo)
    {
        return memberInfo.Name;
    }

    public string GetNamedFieldName(MemberInfo memberInfo, object value)
    {
        return "attribute_" + Convert.ToString(value);
    }
}
```

LINQ:
```c#
var query = from s in InfluxDBQueryable<SensorCustom>.Queryable("my-bucket", "my-org", queryApi, memberResolver)
    where s.Attributes.Any(a => a.Name == "quality" && a.Value == "good")
    select s;
```

Flux Query:

```flux
from(bucket: "my-bucket")
    |> range(start: 0)
    |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value") 
    |> filter(fn: (r) => (r["attribute_quality"] == "good"))
```

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
    |> sort(columns: ["_time"], desc: true)
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

you could create own instance of `IInfluxDBEntityConverter` and use it with `QueryApi` and `WriteApi`.

```c#
var converter = new DomainEntityConverter();
var queryApi = client.GetQueryApi(converter)
```

To satisfy LINQ Query Provider you have to implement `IMemberNameResolver`:

```c#
var resolver = new MemberNameResolver();

var query = from s in InfluxDBQueryable<SensorCustom>.Queryable("my-bucket", "my-org", queryApi, nameResolver)
    where s.Attributes.Any(a => a.Name == "quality" && a.Value == "good")
    select s;
```

for more details see [Any](#any) operator and for full example see: [CustomDomainConverter](/Examples/CustomDomainConverter.cs#L54).