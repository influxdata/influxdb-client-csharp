# InfluxDB.Client.Linq

The library supports to use a LINQ expression to query the InfluxDB.

#### Disclaimer: This library is a work in progress and should not be considered production ready yet.

#### How to start

First, add the library as a dependency for your project:

```bash
# For actual version please check: 

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

#### Perform Query

The LINQ query requires defined `bucket` and `organization` as a source of data. Both of them could be name or ID.

```c#
var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    select s;

var sensors = query.ToList();
```

#### Supported LINQ operators

##### Take

```c#
var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    select s)
    .Take(10);
```

##### Skip

```c#
var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", queryApi)
    select s)
    .Take(10)
    .Skip(50);
```