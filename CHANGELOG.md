## 1.0.0 [2019-08-23]

### Features
1. [#29](https://github.com/influxdata/influxdb-client-csharp/issues/29): Added support for gzip compression of query response and write body 

### Bugs
1. [#27](https://github.com/influxdata/influxdb-client-csharp/issues/27): The org parameter takes either the ID or Name interchangeably

### API
1. [#25](https://github.com/influxdata/influxdb-client-csharp/issues/25): Updated swagger to latest version

## 1.0.0.M2 [2019-08-01]

### Features
1. [#18](https://github.com/influxdata/influxdb-client-csharp/issues/18): Auto-configure client from configuration file
1. [#20](https://github.com/influxdata/influxdb-client-csharp/issues/19): Possibility to specify default tags

### Bugs
1. [#24](https://github.com/influxdata/influxdb-client-csharp/issues/24): The data point without field should be ignored

## 1.0.0.M1

### Features
1. [Client](https://github.com/influxdata/influxdb-client-csharp/tree/master/Client#influxdbclient): The reference C# client that allows query, write and InfluxDB 2.0 management
1. [Client.Legacy](https://github.com/influxdata/influxdb-client-csharp/tree/master/Client.Legacy#influxdbclientflux): The reference C# client that allows you to perform Flux queries against InfluxDB 1.7+
