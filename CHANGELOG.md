## 1.9.0 [unreleased]

### API
1. [#94](https://github.com/influxdata/influxdb-client-csharppull/94): Update swagger to latest version

### Bug Fixes
1. [#100](https://github.com/influxdata/influxdb-client-csharp/pull/100): Thread-safety disposing of clients 
1. [#101](https://github.com/influxdata/influxdb-client-csharp/pull/101/): Use Trace output when disposing WriteApi 

## 1.8.0 [2020-05-15]

### Features
1. [#84](https://github.com/influxdata/influxdb-client-csharp/issues/84): Add possibility to authenticate by Basic Authentication or the URL query parameters
2. [#91](https://github.com/influxdata/influxdb-client-csharp/pull/91): Added support "inf" in Duration
2. [#92](https://github.com/influxdata/influxdb-client-csharp/pull/92): Remove trailing slash from connection URL

### Bug Fixes
1. [#81](https://github.com/influxdata/influxdb-client-csharp/pull/81): Fixed potentially hangs on of WriteApi.Dispose()
1. [#83](https://github.com/influxdata/influxdb-client-csharp/pull/83): Fixed parsing error response for 1.x

## 1.7.0 [2020-04-17]

### Features
1. [#70](https://github.com/influxdata/influxdb-client-csharp/pull/70): Optimized mapping of measurements into `PointData`

### Bug Fixes
1. [#69](https://github.com/influxdata/influxdb-client-csharp/pull/69): Write buffer uses correct flush interval and batch size under heavy load

### Documentation
1. [#77](https://github.com/influxdata/influxdb-client-csharp/pull/77): Clarify how to use a client with InfluxDB 1.8

### Dependencies
1. [#74](https://github.com/influxdata/influxdb-client-csharp/pull/74): update CsvHelper to [15.0.4,16.0)

## 1.6.0 [2020-03-13]

### Features
1. [#61](https://github.com/influxdata/influxdb-client-csharp/issues/61): Set User-Agent to influxdb-client-csharp/VERSION for all requests
1. [#64](https://github.com/influxdata/influxdb-client-csharp/issues/64): Add authentication with Username and Password for Client.Legacy

### Bug Fixes
1. [#63](https://github.com/influxdata/influxdb-client-csharp/pull/63): Correctly parse CSV where multiple results include multiple tables

## 1.5.0 [2020-02-14]

### Features
1. [#57](https://github.com/influxdata/influxdb-client-csharp/pull/57): LogLevel Header also contains query parameters

### CI
1. [#58](https://github.com/influxdata/influxdb-client-csharp/pull/58): CircleCI builds over dotnet 2.2, 3.0 and 3.1; Added build on Windows Server 2019
1. [#60](https://github.com/influxdata/influxdb-client-csharp/pull/60): Deploy dev version to Nuget repository

## 1.4.0 [2020-01-17]

### API
1. [#52](https://github.com/influxdata/influxdb-client-csharp/pull/52): Updated swagger to latest version

### CI
1. [#54](https://github.com/influxdata/influxdb-client-csharp/pull/54): Added beta release to continuous integration

### Bug Fixes
1. [#56](https://github.com/influxdata/influxdb-client-csharp/issues/56): WriteApi is disposed after a buffer is fully processed

## 1.3.0 [2019-12-06]

### Performance

1. [#49](https://github.com/influxdata/influxdb-client-csharp/pull/49): Optimized serialization to LineProtocol

### API
1. [#46](https://github.com/influxdata/influxdb-client-csharp/pull/46): Updated swagger to latest version

### Bug Fixes
1. [#45](https://github.com/influxdata/influxdb-client-csharp/issues/45): Assemblies are strong-named
2. [#48](https://github.com/influxdata/influxdb-client-csharp/pull/48): Packing library icon into a package

## 1.2.0 [2019-11-08]

### Features
1. [#43](https://github.com/influxdata/influxdb-client-csharp/issues/43) Added DeleteApi

### API
1. [#42](https://github.com/influxdata/influxdb-client-csharp/pull/42): Updated swagger to latest version

## 1.1.0 [2019-10-11]

### Breaking Changes
1. [#34](https://github.com/influxdata/influxdb-client-csharp/issues/34): Renamed Point class to PointData and Task class to TaskType (improving the usability of this library)
1. [#40](https://github.com/influxdata/influxdb-client-csharp/pull/40): Added `Async` suffix into asynchronous methods

### Features
1. [#59](https://github.com/influxdata/influxdb-client-csharp/pull/41): Added support for Monitoring & Alerting

### API
1. [#36](https://github.com/influxdata/influxdb-client-csharp/issues/36): Updated swagger to latest version

### Bug Fixes
1. [#31](https://github.com/influxdata/influxdb-client-csharp/issues/31): Drop NaN and infinity values from fields when writing to InfluxDB
1. [#39](https://github.com/influxdata/influxdb-client-csharp/pull/39): FluxCSVParser uses a CultureInfo for parsing string to double

## 1.0.0 [2019-08-23]

### Features
1. [#29](https://github.com/influxdata/influxdb-client-csharp/issues/29): Added support for gzip compression of query response and write body 

### Bug Fixes
1. [#27](https://github.com/influxdata/influxdb-client-csharp/issues/27): The org parameter takes either the ID or Name interchangeably

### API
1. [#25](https://github.com/influxdata/influxdb-client-csharp/issues/25): Updated swagger to latest version

## 1.0.0.M2 [2019-08-01]

### Features
1. [#18](https://github.com/influxdata/influxdb-client-csharp/issues/18): Auto-configure client from configuration file
1. [#20](https://github.com/influxdata/influxdb-client-csharp/issues/19): Possibility to specify default tags

### Bug Fixes
1. [#24](https://github.com/influxdata/influxdb-client-csharp/issues/24): The data point without field should be ignored

## 1.0.0.M1

### Features
1. [Client](https://github.com/influxdata/influxdb-client-csharp/tree/master/Client#influxdbclient): The reference C# client that allows query, write and InfluxDB 2.0 management
1. [Client.Legacy](https://github.com/influxdata/influxdb-client-csharp/tree/master/Client.Legacy#influxdbclientflux): The reference C# client that allows you to perform Flux queries against InfluxDB 1.7+
