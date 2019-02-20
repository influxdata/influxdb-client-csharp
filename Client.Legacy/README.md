# InfluxDB.Client.Flux

> This library is under development and no stable version has been released yet.  
> The API can change at any moment.

The reference C# library for the InfluxDB 1.7+ `/api/v2/query` REST API using the [Flux language](http://bit.ly/flux-spec). 

## How To Use

### Create client

The `FluxClientFactory` creates an instance of a `FluxClient` client that can be customized with `FluxConnectionOptions`.

`FluxConnectionOptions` parameters:
 
- `url` -  the url to connect to InfluxDB 
- `okHttpClient` - custom HTTP client to use for communications with InfluxDB (optional)

```java
// client creation
var options = new FluxConnectionOptions("http://127.0.0.1:8086");

var fluxClient = FluxClientFactory.Create(options);

fluxClient.Query(...)
...
```

### Query using the Flux language

The library supports an asynchronous queries. 

The asynchronous query API allows streaming of `FluxRecord`s with the possibility of implementing custom
error handling and `OnComplete` callback notification. 

A `Cancellable` object is used for aborting a query while processing. 

A query example:   

```c#
string fluxQuery = "from(bucket: \"telegraf\")\n" +
    " |> filter(fn: (r) => (r[\"_measurement\"] == \"cpu\" AND r[\"_field\"] == \"usage_system\"))" +
    " |> range(start: -1d)" +
    " |> sample(n: 5, pos: 1)";

fluxClient.Query(fluxQuery, (cancellable, record) =>
            {
                // process the flux query records
                Console.WriteLine(record.GetTime() + ": " + record.GetValue());
                
                if (some condition) 
                {
                    // abort processing
                    cancellable.cancel();
                }
            },
            (error) =>
            {
                // error handling while processing result
                Console.WriteLine("Error occured: "+ error.TString());
            }, 
            () =>
            {
                // on complete
                Console.WriteLine("Query completed");
            }).GetAwaiter().GetResult();
```

#### Raw query response

It is possible to parse a result line-by-line using the `QueryRaw` method.  

```c#
void QueryRaw(string query,
              Action<ICancellable, string> onResponse,
              Action<Exception> onError,
              Action onComplete);
```

### Advanced Usage

#### Check the server status and version

Server availability can be checked using the `FluxClient.Ping()` endpoint.  Server version can be obtained using `FluxClient.Version()`.
 
#### Log HTTP Request and Response

The Requests and Responses can be logged by changing the LogLevel. LogLevel values are None, Basic, Headers, Body. Note that 
applying the `Body` LogLevel will disable chunking while streaming and will load the whole response into memory.  

```c#
fluxClient.SetLogLevel(LogLevel.Body)
```
 
## Version

The latest package for .NET CLI:
```bash
dotnet add package InfluxDB.Client.Flux  --version 1.0-alpha --source https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/
```
  
Or when using with Package Manager:
```bash
Install-Package InfluxDB.Client.Flux -Version 1.0-alpha -Source https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/
```