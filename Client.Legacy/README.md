# InfluxDB.Client.Flux

[![Nuget](https://img.shields.io/nuget/v/InfluxDB.Client.Flux)](https://www.nuget.org/packages/InfluxDB.Client.Flux/)

The reference C# library for the InfluxDB 1.7+ `/api/v2/query` REST API using the [Flux language](http://bit.ly/flux-spec). 

## Documentation

This section contains links to the client library documentation.

* [Product documentation](https://docs.influxdata.com/influxdb/latest/api-guide/client-libraries/), [Getting Started](#how-to-use)
* [Examples](../Examples)
* [API Reference](https://influxdata.github.io/influxdb-client-csharp/api/InfluxDB.Client.Flux.FluxClient.html)
* [Changelog](../CHANGELOG.md)

## How To Use

### Create client

The `FluxClientFactory` creates an instance of a `FluxClient` client that can be customized with `FluxConnectionOptions`.

`FluxConnectionOptions` parameters:
 
- `url` -  the url to connect to InfluxDB 
- `okHttpClient` - custom HTTP client to use for communications with InfluxDB (optional)
- `username` - name of your InfluxDB user (optional)
- `password` - password of your InfluxDB user (optional)
- `authentication` - type of authentication (optional). There are two options for authenticating: Basic Authentication and the URL query parameters (default).

```c#
// client creation
var options = new FluxConnectionOptions("http://127.0.0.1:8086");

using var fluxClient = FluxClientFactory.Create(options);

fluxClient.QueryAsync(...)
...
```
#### Authenticate requests

##### URL query parameters
```c#
// client creation
var options = new FluxConnectionOptions("http://127.0.0.1:8086", "my-user", "my-password".ToCharArray());

using var fluxClient = FluxClientFactory.Create(options);

fluxClient.QueryAsync(...)
...
```

##### Basic authentication
```c#
// client creation
var options = new FluxConnectionOptions("http://127.0.0.1:8086", "my-user", "my-password".ToCharArray(),
    FluxConnectionOptions.AuthenticationType.BasicAuthentication);

using var fluxClient = FluxClientFactory.Create(options);

fluxClient.QueryAsync(...)
...
```

### Query using the Flux language

The library supports an asynchronous queries. 

The asynchronous query API allows streaming of `FluxRecord`s with the possibility of implementing custom
error handling and `OnComplete` callback notification. 

A `CancellationToken` object is used for aborting a query while processing. 

A query example:   

```c#
string fluxQuery = "from(bucket: \"telegraf\")\n" +
    " |> filter(fn: (r) => (r[\"_measurement\"] == \"cpu\" AND r[\"_field\"] == \"usage_system\"))" +
    " |> range(start: -1d)" +
    " |> sample(n: 5, pos: 1)";
    
var source = new CancellationTokenSource();

fluxClient.QueryAsync(fluxQuery, record =>
            {
                // process the flux query records
                Console.WriteLine(record.GetTime() + ": " + record.GetValue());
                
                if (some condition) 
                {
                    // abort processing
                    source.Cancel();
                }
            },
            (error) =>
            {
                // error handling while processing result
                Console.WriteLine($"Error occured: {error}");
            }, 
            () =>
            {
                // on complete
                Console.WriteLine("Query completed");
            }, source.Token).ConfigureAwait(false).GetAwaiter().GetResult();
```

#### Raw query response

It is possible to parse a result line-by-line using the `QueryRaw` method.  

```c#
void QueryRawAsync(string query, Action<string> onResponse, string dialect = null, Action<Exception> onError = null, Action onComplete = null, CancellationToken cancellationToken = default);
```

### Advanced Usage

#### Check the server status and version

Server availability can be checked using the `FluxClient.PingAsync()` endpoint.  Server version can be obtained using `FluxClient.VersionAsync()`.
 
#### Log HTTP Request and Response

The Requests and Responses can be logged by changing the LogLevel. LogLevel values are None, Basic, Headers, Body. Note that 
applying the `Body` LogLevel will disable chunking while streaming and will load the whole response into memory.  

```c#
fluxClient.SetLogLevel(LogLevel.Body)
```
 
## Version

The latest package for .NET CLI:
```bash
dotnet add package InfluxDB.Client.Flux
```
  
Or when using with Package Manager:
```bash
Install-Package InfluxDB.Client.Flux
```