## 3.1.0 [unreleased]

### Features
1. [#239](https://github.com/influxdata/influxdb-client-csharp/pull/239): Add support for Asynchronous queries [LINQ]
1. [#240](https://github.com/influxdata/influxdb-client-csharp/pull/240): Add IsMeasurement option to Column attribute for dynamic measurement names in POCO classes
1. [#246](https://github.com/influxdata/influxdb-client-csharp/pull/246): Add support for deserialization of POCO column property types with a "Parse" method, such as Guid

## 3.0.0 [2021-09-17]

### Breaking Changes
Adds a `Type` overload for POCOs to `QueryAsync`. This will add `object ConvertToEntity(FluxRecord, Type)` to `IFluxResultMapper`

### Features
1. [#232](https://github.com/influxdata/influxdb-client-csharp/pull/232): Add a `Type` overload for POCOs to `QueryAsync`.
1. [#233](https://github.com/influxdata/influxdb-client-csharp/pull/233): Add possibility to follow HTTP redirects

### Bug Fixes
1. [#236](https://github.com/influxdata/influxdb-client-csharp/pull/236): Mapping `long` type into Flux AST [LINQ]

## 2.1.0 [2021-08-20]

### Bug Fixes
1. [#221](https://github.com/influxdata/influxdb-client-csharp/pull/221): Parsing infinite numbers
2. [#229](https://github.com/influxdata/influxdb-client-csharp/pull/229): Fix cookie handling in session mode

### Dependencies
1. [#222](https://github.com/influxdata/influxdb-client-csharp/pull/222): Update dependencies:
    - RestSharp to 106.12.0

## 2.0.0 [2021-07-09]

### Breaking Changes

This release introduces a support for new InfluxDB OSS API definitions - [oss.yml](https://github.com/influxdata/openapi/blob/master/contracts/oss.yml). The following breaking changes are in underlying API services and doesn't affect common apis such as - `WriteApi`, `QueryApi`, `BucketsApi`, `OrganizationsApi`...

- `UsersService` uses `PostUser` to create `User`
- `AuthorizationsService` uses `AuthorizationPostRequest` to create `Authorization`
- `BucketsService` uses `PatchBucketRequest` to update `Bucket`
- `OrganizationsService` uses `PostOrganizationRequest` to create `Organization`
- `OrganizationsService` uses `PatchOrganizationRequest` to update `Organization`
- `DashboardsService` uses `PatchDashboardRequest` to update `Dashboard`
- `DeleteService` is used to delete time series data instead of `DefaultService`
- `Run` contains list of `LogEvent` in `Log` property
- `DBRPs` contains list of `DBRP` in `Content` property
- `DBRPsService` uses `DBRPCreate` to create `DBRP`
- Inheritance structure:
  - `Check` <- `CheckDiscriminator` <- `CheckBase`
  - `NotificationEndpoint` <- `NotificationEndpointDiscriminator` <- `NotificationEndpointBase`
  - `NotificationRule` <- `NotificationRuleDiscriminator` <- `NNotificationRuleBase`
- Flux AST literals extends the AST `Expression` object 

### Deprecates
- `AuthorizationsApi.CreateAuthorizationAsync(Authorization)`: instead use `AuthorizationsApi.CreateAuthorizationAsync(AuthorizationPostRequest)`

### Features
1. [#206](https://github.com/influxdata/influxdb-client-csharp/pull/206): Use optional args to pass query parameters into API list call - useful for the ability to use pagination.

### API
1. [#206](https://github.com/influxdata/influxdb-client-csharp/pull/206), [#210](https://github.com/influxdata/influxdb-client-csharp/pull/210), [#211](https://github.com/influxdata/influxdb-client-csharp/pull/211): Use InfluxDB OSS API definitions to generated APIs

### Dependencies
1. [#209](https://github.com/influxdata/influxdb-client-csharp/pull/209): Update dependencies:
    - CsvHelper to 27.1.0
    - Newtonsoft.Json 13.0.1
    - NodaTime to 3.0.5
    - NodaTime.Serialization.JsonNet to 3.0.0
    - Microsoft.Extensions.ObjectPool to 5.0.7

### Documentation
1. [#213](https://github.com/influxdata/influxdb-client-csharp/pull/213): API documentation is deploy to [GitHub Pages](https://influxdata.github.io/influxdb-client-csharp/api/InfluxDB.Client.html) 

## 1.19.0 [2021-06-04]

### Features
1. [#194](https://github.com/influxdata/influxdb-client-csharp/pull/194): Add possibility to handle HTTP response from InfluxDB server [write]
1. [#197](https://github.com/influxdata/influxdb-client-csharp/pull/197): Optimize Flux Query for querying one time-series [LINQ]
1. [#205](https://github.com/influxdata/influxdb-client-csharp/pull/205): Exponential random retry [write]

### Bug Fixes
1. [#193](https://github.com/influxdata/influxdb-client-csharp/pull/193): Create services without API implementation
1. [#202](https://github.com/influxdata/influxdb-client-csharp/pull/202): Flux AST for Tag parameters which are not `String` [LINQ]

## 1.18.0 [2021-04-30]

### Features
1. [#184](https://github.com/influxdata/influxdb-client-csharp/pull/184): Add possibility to specify `WebProxy` for Client
1. [#185](https://github.com/influxdata/influxdb-client-csharp/pull/185): Use `group()` function in output Flux query. See details - [Group function](/Client.Linq/README.md#group-function) [LINQ]
1. [#186](https://github.com/influxdata/influxdb-client-csharp/pull/186): Produce a typed HTTP exception
1. [#188](https://github.com/influxdata/influxdb-client-csharp/pull/188): Switch `pivot()` and `drop()` function to achieve better performance

### Bug Fixes
1. [#183](https://github.com/influxdata/influxdb-client-csharp/pull/183): Propagate runtime exception to EventHandler

## 1.17.0 [2021-04-01]

### Features
1. [#146](https://github.com/influxdata/influxdb-client-csharp/pull/146): Add support for querying by `LINQ`
1. [#171](https://github.com/influxdata/influxdb-client-csharp/pull/171): Add `QueryApiSync` for synchronous querying
1. [#171](https://github.com/influxdata/influxdb-client-csharp/pull/171): Add `IDomainObjectMapper` for custom mapping DomainObject from/to InfluxDB
1. [#180](https://github.com/influxdata/influxdb-client-csharp/pull/180): Add a mutable `PointData.Builder` to optimize building of immutable `PointData`

### API
1. [#174](https://github.com/influxdata/influxdb-client-csharp/pull/174): Add possibility to use `CancellationToken` in REST API
1. [#179](https://github.com/influxdata/influxdb-client-csharp/pull/179): Add possibility to use `CancellationToken` in the async write API (WriteApiAsync)

### Bug Fixes
1. [#168](https://github.com/influxdata/influxdb-client-csharp/pull/168): DateTime is always serialized into UTC
1. [#169](https://github.com/influxdata/influxdb-client-csharp/pull/169): Fix domain structure for Flux AST
1. [#181](https://github.com/influxdata/influxdb-client-csharp/pull/181): Remove download overhead for Queries

### Dependencies
1. [#175](https://github.com/influxdata/influxdb-client-csharp/pull/175): Update dependencies of `InfluxDB.Client`:
    - JsonSubTypes to 1.8.0
    - Microsoft.Extensions.ObjectPool to 5.0.4
    - Microsoft.Net.Http.Headers to 2.2.8
    - System.Collections.Immutable to 5.0.0
    - System.Configuration.ConfigurationManager to 5.0.0
    - System.Reactive to 5.0.0
1. [#182](https://github.com/influxdata/influxdb-client-csharp/pull/182): Update test dependencies:
    - Microsoft.NET.Test.Sdk to 16.5.0

### CI
1. [#182](https://github.com/influxdata/influxdb-client-csharp/pull/182): Add build for `dotnet5`, Fix code coverage report


## 1.16.0 [2021-03-05]

### Bug Fixes
1. [#154](https://github.com/influxdata/influxdb-client-csharp/pull/154): Always use `ConfigureAwait(false)` to avoid unnecessary context switching and potential dead-locks. Avoid unnecessary await overhead.
1. [#158](https://github.com/influxdata/influxdb-client-csharp/pull/158): Remove unnecessary dependencies: `System.Net.Http` and `Microsoft.Bcl.AsyncInterfaces`

### CI
1. [#165](https://github.com/influxdata/influxdb-client-csharp/pull/165): Updated stable image to `influxdb:latest` and nightly to `quay.io/influxdb/influxdb:nightly`

## 1.15.0 [2021-01-29]

### Bug Fixes
1. [#143](https://github.com/influxdata/influxdb-client-csharp/pull/143): Added validation that a configuration is present when is client configured via file
1. [#150](https://github.com/influxdata/influxdb-client-csharp/pull/150): The unsigned numbers are serialized with `u` postfix

### Dependencies
1. [#145](https://github.com/influxdata/influxdb-client-csharp/pull/145): Updated RestSharp to 106.11.7
1. [#148](https://github.com/influxdata/influxdb-client-csharp/pull/148): Updated CsvHelper to 18.0.0

### CI
1. [#140](https://github.com/influxdata/influxdb-client-csharp/pull/140): Updated default docker image to v2.0.3

## 1.14.0 [2020-12-04]

### Features
1. [#136](https://github.com/influxdata/influxdb-client-csharp/pull/136): CSV parser is able to parse export from UI

### CI
1. [#138](https://github.com/influxdata/influxdb-client-csharp/pull/138): Updated default docker image to v2.0.2

## 1.13.0 [2020-10-30]

### Features
1. [#121](https://github.com/influxdata/influxdb-client-csharp/pull/121): Added IAsyncEnumerable&lt;T&gt; query overloads to QueryAPI
1. [#127](https://github.com/influxdata/influxdb-client-csharp/pull/127): Added exponential backoff strategy for batching writes. Default value for `RetryInterval` is 5_000 milliseconds.
1. [#128](https://github.com/influxdata/influxdb-client-csharp/pull/128): Improved logging message for retries

## 1.12.0 [2020-10-02]

### Features
1. [#117](https://github.com/influxdata/influxdb-client-csharp/issues/117): Added support for string token
1. [#121](https://github.com/influxdata/influxdb-client-csharp/pull/121): Added IAsyncEnumerable&lt;T&gt; query overloads to QueryAPI

### API
1. [#122](https://github.com/influxdata/influxdb-client-csharp/issues/122): Default port changed from 9999 to 8086
1. [#124](https://github.com/influxdata/influxdb-client-csharp/pull/124): Removed labels in organization API, removed Pkg* structure and package service

### Bug Fixes
1. [#119](https://github.com/influxdata/influxdb-client-csharp/issues/119): No timestamp returned via POCO based QueryAsync&lt;T&gt;

## 1.11.0 [2020-08-14]

### Features
1. [#97](https://github.com/influxdata/influxdb-client-csharp/pull/97): Improved WriteApi performance
1. [#116](https://github.com/influxdata/influxdb-client-csharp/pull/116): Moved api generator to separate module influxdb-clients-apigen

### Bug Fixes
1. [#113](https://github.com/influxdata/influxdb-client-csharp/pull/113): Fixed unnecessary API call when writing collection of DataPoints

## 1.10.0 [2020-07-17]

### Features
1. [#102](https://github.com/influxdata/influxdb-client-csharp/pull/102): Added WriteApiAsync for asynchronous write without batching

### Bug Fixes
1. [#106](https://github.com/influxdata/influxdb-client-csharp/pull/106): Fixed serialization of `\n`, `\r` and `\t` to Line Protocol, `=` is valid sign for measurement name  
1. [#108](https://github.com/influxdata/influxdb-client-csharp/issues/108): Replaced useless .ContinueWith in Api by direct call

## 1.9.0 [2020-06-19]

### Features
1. [#96](https://github.com/influxdata/influxdb-client-csharp/pull/96): The PointData builder is now immutable

### API
1. [#94](https://github.com/influxdata/influxdb-client-csharp/pull/94): Update swagger to latest version
1. [#103](https://github.com/influxdata/influxdb-client-csharp/pull/103): Removed log system from Bucket, Dashboard, Organization, Task and Users API - [influxdb#18459](https://github.com/influxdata/influxdb/pull/18459)

### CI
1. [#104](https://github.com/influxdata/influxdb-client-csharp/pull/104): Upgraded InfluxDB 1.7 to 1.8

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
