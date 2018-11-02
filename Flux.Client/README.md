# flux-csharp

The reference C# library for the InfluxDB 1.7 `/api/v2/query` REST API using the [Flux language](https://github.com/influxdata/flux/blob/master/docs/SPEC.md). 



> This library is under development and no stable version has been released yet.  
> The API can change at any moment.

[![Build Status](https://travis-ci.org/bonitoo-io/flux-csharp.svg?branch=master)](https://travis-ci.org/bonitoo-io/flux-csharp)
[![codecov](https://codecov.io/gh/bonitoo-io/flux-csharp/branch/master/graph/badge.svg)](https://codecov.io/gh/bonitoo-io/flux-csharp)
[![License](https://img.shields.io/github/license/bonitoo-io/flux-csharp.svg)](https://github.com/bonitoo-io/flux-csharp/blob/master/LICENSE)
[![GitHub issues](https://img.shields.io/github/issues-raw/bonitoo-io/flux-csharp.svg)](https://github.com/bonitoo-io/flux-csharp/issues)
[![GitHub pull requests](https://img.shields.io/github/issues-pr-raw/bonitoo-io/flux-csharp.svg)](https://github.com/bonitoo-io/flux-csharp/pulls)

### Create client

The `FluxClientFactory` creates an instance of a `FluxClient` client that can be customized with `FluxConnectionOptions`.

`FluxConnectionOptions` parameters:
 
- `url` -  the url to connect to Platform
- `okHttpClient` - custom HTTP client to use for communications with Platform (optional)

```java
    // client creation
    var options = new FluxConnectionOptions("http://127.0.0.1:8086");
    
    var fluxClient = FluxClientFactory.Create(options);
    
    fluxClient.Query(...)
     ...
```

## Query using the Flux language

The library supports an asynchronous queries. 

The asynchronous query API allows streaming of `FluxRecord`s with the possibility of implementing custom
error handling and `onComplete` callback notification. 

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

