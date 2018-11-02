# flux-csharp

[![Build Status](https://travis-ci.org/bonitoo-io/flux-csharp.svg?branch=master)](https://travis-ci.org/bonitoo-io/flux-csharp)
[![codecov](https://codecov.io/gh/bonitoo-io/flux-csharp/branch/master/graph/badge.svg)](https://codecov.io/gh/bonitoo-io/flux-csharp)
[![License](https://img.shields.io/github/license/bonitoo-io/flux-csharp.svg)](https://github.com/bonitoo-io/flux-csharp/blob/master/LICENSE)
[![GitHub issues](https://img.shields.io/github/issues-raw/bonitoo-io/flux-csharp.svg)](https://github.com/bonitoo-io/flux-csharp/issues)
[![GitHub pull requests](https://img.shields.io/github/issues-pr-raw/bonitoo-io/flux-csharp.svg)](https://github.com/bonitoo-io/flux-csharp/pulls)

This repository contains the reference C# client for the InfluxData Platform.

> This library is under development and no stable version has been released yet.  
> The API can change at any moment.

### Features

- Supports querying using the Flux language over the InfluxDB 1.7+ REST API (`/api/v2/query endpoint`) 
- InfluxData Platform OSS 2.0 client
    - Querying data using the Flux language

### Documentation

- **[Flux.Client](./Flux.Client)** - The reference c# client that allows you to perform Flux queries against InfluxDB 1.7+.

### Flux queries in InfluxDB 1.7+

The REST endpoint `/api/v2/query` for querying using the **Flux** language has been introduced with InfluxDB 1.7.

The following example demonstrates querying using the Flux language: 

```c#
using System;
using Flux.Client;

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
