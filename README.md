# influxdb-client-csharp

[![Build Status](https://travis-ci.org/bonitoo-io/influxdb-client-csharp.svg?branch=master)](https://travis-ci.org/bonitoo-io/influxdb-client-csharp)
[![codecov](https://codecov.io/gh/bonitoo-io/influxdb-client-csharp/branch/master/graph/badge.svg)](https://codecov.io/gh/bonitoo-io/influxdb-client-csharp)
[![License](https://img.shields.io/github/license/bonitoo-io/influxdb-client-csharp.svg)](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/LICENSE)
[![GitHub issues](https://img.shields.io/github/issues-raw/bonitoo-io/influxdb-client-csharp.svg)](https://github.com/bonitoo-io/influxdb-client-csharp/issues)
[![GitHub pull requests](https://img.shields.io/github/issues-pr-raw/bonitoo-io/influxdb-client-csharp.svg)](https://github.com/bonitoo-io/influxdb-client-csharp/pulls)

This repository contains the reference C# client for the InfluxDB 2.0.

> This library is under development and no stable version has been released yet.  
> The API can change at any moment.

### Features

- Supports querying using the Flux language over the InfluxDB 1.7+ REST API (`/api/v2/query endpoint`) 
- InfluxDB 2.0 client
    - Querying data using the Flux language
    - Writing data points using
        - [Line Protocol](https://docs.influxdata.com/influxdb/v1.6/write_protocols/line_protocol_tutorial/) 
        - [Point object](https://github.com/bonitoo-io/influxdb-client-csharp/blob/master/InfluxDB.Client/Writes/Point.cs) 
        - POJO
    - InfluxDB 2.0 Management API client for managing
        - sources, buckets
        - tasks
        - authorizations
        - health check
        - ...
### Documentation

- **[Client.Legacy](./Client.Legacy)** - The reference c# client that allows you to perform Flux queries against InfluxDB 1.7+.

### Flux queries in InfluxDB 1.7+

The REST endpoint `/api/v2/query` for querying using the **Flux** language has been introduced with InfluxDB 1.7.

The following example demonstrates querying using the Flux language: 

```c#
using System;
using Client.Legacy;

namespace Flux.Examples
{
    public static class FluxExample
    {
        public static void Run()
        {
            var fluxClient = FluxClientFactory.Create("http://localhost:8086/");

            string fluxQuery = "from(bucket: \"telegraf\")\n"
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

## Version

The latest package for .NET CLI:
```bash
dotnet add package InfluxData.FluxClient --version 1.0-alpha --source https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/
```
  
Or when using with Package Manager:
```bash
Install-Package InfluxData.FluxClient -Version 1.0-alpha -Source https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/
```
