using System.Diagnostics;
using System.Globalization;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Linq;
using InfluxDB.Client.Writes;

namespace ExampleBlazor.Data;

public static class InfluxModel
{
    public static readonly Client Client = new()
    {
        Url = "http://localhost:8086",
        Token = "my-token",
        Org = "my-org"
    };

    public static async Task<bool> CheckClient(Client? client)
    {
        using var influxDbClient = client?.GetClient();
        return await influxDbClient?.PingAsync()!;
    }

    public static async Task<string> GetOrganizationId(Client client)
    {
        using var influxDbClient = client.GetClient();
        try
        {
            return (await influxDbClient.GetOrganizationsApi().FindOrganizationsAsync(org: client.Org))
                .First().Id;
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to load organization Id." + e);
            return "";
        }
    }

    #region ---------------------------------- Buckets ----------------------------------

    public static async Task<string> GetBucketId(string bucketName)
    {
        using var influxDbClient = Client.GetClient();
        try
        {
            return (await influxDbClient.GetBucketsApi().FindBucketByNameAsync(bucketName)).Id;
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to load bucket Id.");
            return "";
        }
    }

    public static async Task DeleteBucket(Bucket bucket)
    {
        using var influxDbClient = Client.GetClient();
        try
        {
            await influxDbClient.GetBucketsApi().DeleteBucketAsync(bucket);
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to delete bucket.");
        }
    }

    public static async Task CloneBucket(Bucket bucket, string name)
    {
        using var influxDbClient = Client.GetClient();
        try
        {
            await influxDbClient.GetBucketsApi().CloneBucketAsync(name, bucket);
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to clone bucket.");
        }
    }

    public static async Task<Bucket?> CreateBucket(string name)
    {
        var orgId = await GetOrganizationId(Client);
        using var influxDbClient = Client.GetClient();
        try
        {
            // Create bucket "iot_bucket" with data retention set to 3,600 seconds
            var retention = new BucketRetentionRules(BucketRetentionRules.TypeEnum.Expire, 3600);

            var bucket = await influxDbClient.GetBucketsApi().CreateBucketAsync(name, retention, orgId);

            // Create access token to "iot_bucket"
            var resource = new PermissionResource(PermissionResource.TypeBuckets, bucket.Id, null,
                orgId);

            // Read permission
            var read = new Permission(Permission.ActionEnum.Read, resource);

            // Write permission
            var write = new Permission(Permission.ActionEnum.Write, resource);

            await influxDbClient.GetAuthorizationsApi()
                .CreateAuthorizationAsync(orgId, new List<Permission> { read, write });

            return bucket;
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to create bucket.");
            return null;
        }
    }

    public static async Task FindBucketByName(string bucketName)
    {
        using var influxDbClient = Client.GetClient();

        await influxDbClient.GetBucketsApi().FindBucketByNameAsync(bucketName);
    }

    public static async Task FindBucketById(string bucketId)
    {
        using var influxDbClient = Client.GetClient();

        await influxDbClient.GetBucketsApi().FindBucketByIdAsync(bucketId);
    }

    public static async Task FindBucketsByOrgName(string orgName)
    {
        using var influxDbClient = Client.GetClient();

        await influxDbClient.GetBucketsApi().FindBucketsByOrgNameAsync(orgName);
    }

    public static async Task<List<Bucket>> FetchBuckets()
    {
        using var influxDbClient = Client.GetClient();

        return await influxDbClient.GetBucketsApi().FindBucketsAsync();
    }

    #endregion ---------------------------------- Buckets ----------------------------------

    #region ---------------------------------- Devices ----------------------------------

    public static async Task<List<FluxTable>?> FetchDeviceList(string bucket)
    {
        var orgId = await GetOrganizationId(Client);
        using var influxDbClient = Client.GetClient();
        try
        {
            var query = $"from(bucket: \"{bucket}\")" +
                        " |> range(start: -30d)" +
                        "|> filter(fn: (r) => r[\"_measurement\"] == \"deviceauth\"" +
                        "and r[\"_field\"] == \"key\")" +
                        "|> last()" +
                        "|> filter(fn: (r) => r[\"_value\"] != \"\")";
            return await influxDbClient.GetQueryApi().QueryAsync(query, orgId);
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to load devices.");
            return null;
        }
    }

    public static async Task<DateTime> FetchDeviceCreatedAt(string bucket, string deviceId)
    {
        var orgId = await GetOrganizationId(Client);
        using var influxDbClient = Client.GetClient();
        try
        {
            var query = $"from(bucket: \"{bucket}\")" +
                        " |> range(start: -30d)" +
                        " |> filter(fn: (r) => r[\"_measurement\"] == \"deviceauth\"" +
                        " and r[\"_field\"] == \"createdAt\"" +
                        $"and r.deviceId == \"{deviceId}\")" +
                        "|> keep(columns: [\"_field\", \"_value\"])" +
                        "|> last()";
            var result = await influxDbClient.GetQueryApi().QueryAsync(query, orgId);

            if (result.Count > 0)
            {
                var value = result.FirstOrDefault()!.Records.FirstOrDefault()!.GetValueByKey("_value");
                return Convert.ToDateTime(value.ToString());
            }

            return DateTime.MinValue;
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to load device. - " + e);
            return DateTime.MinValue;
        }
    }

    public static async Task CreateDevice(string deviceId, string deviceType, string bucket)
    {
        var orgId = await GetOrganizationId(Client);
        using var influxDbClient = Client.GetClient();
        try
        {
            var createdAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz",
                CultureInfo.InvariantCulture);
            var point = PointData.Measurement("deviceauth")
                .Tag("deviceId", deviceId)
                .Tag("device", deviceType)
                .Field("createdAt", createdAt);

            using var writeApi = influxDbClient.GetWriteApi();
            writeApi.WritePoint(point, bucket, orgId);
            await _createDeviceAuthorization(deviceId, bucket, orgId);
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to created device.");
        }
    }

    private static async Task _createDeviceAuthorization(string deviceId, string bucket, string orgId)
    {
        var authorization = await _createIoTAuthorization(deviceId, bucket, orgId);
        using var influxDbClient = Client.GetClient();

        if (authorization != null)
        {
            try
            {
                var point = PointData.Measurement("deviceauth")
                    .Tag("deviceId", deviceId)
                    .Field("key", authorization.Id)
                    .Field("token", authorization.Token);
                using var writeApi = influxDbClient.GetWriteApi();
                writeApi.WritePoint(point, bucket, orgId);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to create authorization. - " + e);
            }
        }
    }

    private static async Task<Authorization?> _createIoTAuthorization(string deviceId, string bucket, string orgId)
    {
        var bucketId = await GetBucketId(bucket);
        using var influxDbClient = Client.GetClient();
        var authorizationApi = influxDbClient.GetAuthorizationsApi();

        var permissions = new List<Permission>
        {
            new(
                Permission.ActionEnum.Read,
                new PermissionResource(
                    "buckets",
                    bucketId,
                    orgID: orgId,
                    org: Client.Org)),
            new(
                Permission.ActionEnum.Write,
                new PermissionResource(
                    "buckets",
                    bucketId,
                    orgID: orgId,
                    org: Client.Org))
        };

        try
        {
            var request = new AuthorizationPostRequest(
                orgId,
                description: "IoTCenterDevice: " + deviceId,
                permissions: permissions);
            return await authorizationApi.CreateAuthorizationAsync(request);
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to post authorization request. - " + e);
            return null;
        }
    }

    public static async Task<bool> RemoveDevice(FluxRecord device, string bucket)
    {
        var orgId = await GetOrganizationId(Client);
        using var influxDbClient = Client.GetClient();
        try
        {
            var deviceId = device.Values.First(rec => rec.Key == "deviceId").Value.ToString();
            var query = $"from(bucket: \"{bucket}\")" +
                        "|> range(start: -30d)" +
                        "|> filter(fn: (r) => r[\"_measurement\"] == \"deviceauth\"" +
                        $"and r.deviceId == \"{deviceId}\")" +
                        "|> last()";
            var result = await influxDbClient.GetQueryApi().QueryAsync(query, orgId);

            var key = result.FirstOrDefault(table =>
                    table.Records.Exists(rec => rec.GetValueByKey("_field").ToString() == "key"))!.Records
                .FirstOrDefault()!.Values.First(rec => rec.Key == "_value").Value.ToString();
            return await _removeDeviceAuthorization(deviceId!, key!, bucket, orgId);
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to delete device.");
            return false;
        }
    }

    private static async Task<bool> _removeDeviceAuthorization(string deviceId, string key, string bucket, string orgId)
    {
        if (!string.IsNullOrEmpty(key))
        {
            await _deleteIoTAuthorization(key);

            using var influxDbClient = Client.GetClient();
            try
            {
                var point = PointData.Measurement("deviceauth")
                    .Tag("deviceId", deviceId)
                    .Field("key", "")
                    .Field("token", "");
                using var writeApi = influxDbClient.GetWriteApi();
                writeApi.WritePoint(point, bucket, orgId);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to delete authorization. - " + e);
            }
        }

        return false;
    }

    private static async Task _deleteIoTAuthorization(string key)
    {
        using var influxDbClient = Client.GetClient();
        var authorizationApi = influxDbClient.GetAuthorizationsApi();

        try
        {
            await authorizationApi.DeleteAuthorizationAsync(key);
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to delete authorization. - " + e);
        }
    }

    #endregion ---------------------------------- Devices ----------------------------------

    public static async Task<List<FluxTable>?> FetchData(string bucket, string timeRange, string measurement)
    {
        var orgId = await GetOrganizationId(Client);
        using var influxDbClient = Client.GetClient();
        try
        {
            var fluxQuery = $"from(bucket: \"{bucket}\")"
                            + $" |> range(start: -{timeRange})"
                            + $" |> filter(fn: (r) => (r[\"_measurement\"] == \"{measurement}\"))";

            return await influxDbClient.GetQueryApi().QueryAsync(fluxQuery, orgId);
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to load data.");
            return null;
        }
    }

    public static async Task<List<FluxTable?>> FetchData(string? bucket, string? deviceId, string timeRange,
        string measurement, string field)
    {
        var orgId = await GetOrganizationId(Client);
        using var influxDbClient = Client.GetClient(100);
        try
        {
            var aggregate = "1s";

            switch (timeRange)
            {
                case "1h":
                    aggregate = "30s";
                    break;
                case "6h":
                case "1d":
                    aggregate = "5m";
                    break;
                case "3d":
                case "7d":
                    aggregate = "30m";
                    break;
                case "30d":
                    aggregate = "2h";
                    break;
            }

            var fluxQuery = $"from(bucket: \"{bucket}\")"
                            + $" |> range(start: -{timeRange})"
                            + $" |> filter(fn: (r) => (r[\"_measurement\"] == \"{measurement}\"))"
                            + $" |> filter(fn: (r) => (r.clientId == \"{deviceId}\"))"
                            + $" |> filter(fn: (r) => (r[\"_field\"] == \"{field}\"))"
                            + " |> keep(columns: [\"_value\", \"_time\", \"_field\", \"clientId\"])"
                            + $" |> aggregateWindow(column: \"_value\", every: {aggregate}, fn: mean)";

            return await influxDbClient.GetQueryApi().QueryAsync(fluxQuery, orgId);
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to load data. - " + e);
            return new List<FluxTable?>();
        }
    }

    public static async Task<FluxTable?> FetchDataMean(string? bucket, string? deviceId, string timeRange,
        string measurement, string field)
    {
        var orgId = await GetOrganizationId(Client);
        using var influxDbClient = Client.GetClient();
        try
        {
            var fluxQuery = $"from(bucket: \"{bucket}\")"
                            + $" |> range(start: -{timeRange})"
                            + $" |> filter(fn: (r) => (r[\"_measurement\"] == \"{measurement}\"))"
                            + $" |> filter(fn: (r) => (r.clientId == \"{deviceId}\"))"
                            + $" |> filter(fn: (r) => (r[\"_field\"] == \"{field}\"))"
                            + " |> mean()";

            return (await influxDbClient.GetQueryApi().QueryAsync(fluxQuery, orgId)).FirstOrDefault();
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to load data.");
            return new FluxTable();
        }
    }

    public static async Task<List<string?>?> FetchMeasurements(string bucket)
    {
        var orgId = await GetOrganizationId(Client);
        using var influxDbClient = Client.GetClient();
        try
        {
            var query = "import \"influxdata/influxdb/schema\""
                        + $" schema.measurements(bucket: \"{bucket}\")";

            var result = await influxDbClient.GetQueryApi().QueryAsync(query, orgId);
            return result.First().Records.Select(item => item.GetValue().ToString()).ToList();
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to load measurements.");
            return null;
        }
    }

    public static async Task<List<FluxTable>?> FetchMeasurements(string bucket, string? deviceId)
    {
        var orgId = await GetOrganizationId(Client);
        using var influxDbClient = Client.GetClient(100);
        try
        {
            var fluxQuery =
                $"deviceData = from(bucket: \"{bucket}\")"
                + "     |> range(start: -30d)"
                + "     |> filter(fn: (r) => (r[\"_measurement\"] == \"environment\"))"
                + $"    |> filter(fn: (r) => (r.clientId == \"{deviceId}\"))"
                + "measurements = deviceData"
                + "     |> keep(columns: [\"_field\", \"_value\", \"_time\"])"
                + "     |> group(columns: [\"_field\"])"
                + "counts = measurements |> count()"
                + "     |> keep(columns: [\"_field\", \"_value\"])"
                + "     |> rename(columns: {_value: \"count\"   })"
                + "maxValues = measurements |> max  ()"
                + "     |> toFloat()"
                + "     |> keep(columns: [\"_field\", \"_value\"])"
                + "     |> rename(columns: {_value: \"maxValue\"})"
                + "minValues = measurements |> min  ()"
                + "     |> toFloat()"
                + "     |> keep(columns: [\"_field\", \"_value\"])"
                + "     |> rename(columns: {_value: \"minValue\"})"
                + "maxTimes  = measurements |> max  (column: \"_time\")"
                + "     |> keep(columns: [\"_field\", \"_time\" ])"
                + "     |> rename(columns: {_time : \"maxTime\" })"
                + "j = (tables=<-, t) => join(tables: {tables, t}, on:[\"_field\"])"
                + "counts"
                + "|> j(t: maxValues)"
                + "|> j(t: minValues)"
                + "|> j(t: maxTimes)"
                + "|> yield(name: \"measurements\")";

            Debug.WriteLine("Loading measurements.");
            return await influxDbClient.GetQueryApi().QueryAsync(fluxQuery, orgId);
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to load data. -" + e);
            return new List<FluxTable>();
        }
    }

    public static async void WritePoint(PointData point, string? bucket)
    {
        var orgId = await GetOrganizationId(Client);
        var influxDbClient = Client.GetClient();
        try
        {
            using var writeApi = influxDbClient.GetWriteApi();
            writeApi.WritePoint(point, bucket, orgId);
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to write point.");
        }
    }

    #region ---------------------------------- Linq ----------------------------------

    [Measurement("measurement")]
    public class MeasurementPoint
    {
        [Column("clientId", IsTag = true)] public string? DeviceId { get; private set; }

        [Column("TemperatureSensor", IsTag = true)]
        public string? TemperatureSensor { get; set; }

        [Column("HumiditySensor", IsTag = true)]
        public string? HumiditySensor { get; set; }

        [Column("PressureSensor", IsTag = true)]
        public string? PressureSensor { get; set; }

        [Column("CO2Sensor", IsTag = true)] public string? Co2Sensor { get; set; }

        [Column("TVOCSensor", IsTag = true)] public string? TvocSensor { get; set; }

        [Column("GPSSensor", IsTag = true)] public string? GpsSensor { get; set; }

        [Column("Temperature")] public double Temperature { get; set; }

        [Column("Humidity")] public double Humidity { get; set; }

        [Column("Pressure")] public double Pressure { get; set; }

        [Column("CO2")] public int Co2 { get; set; }

        [Column("TVOC")] public int Tvoc { get; set; }

        [Column(IsTimestamp = true)] public DateTime Timestamp { get; private set; }
    }

    public static Task<List<MeasurementPoint>> FetchDataLinq(string? bucket, string? deviceId, string timeRange)
    {
        using var influxDbClient = Client.GetClient(100);
        try
        {
            DateTime timestamp;
            var aggregate = 60;
            switch (timeRange)
            {
                case "5m":
                case "15m":
                    var minutes = Convert.ToInt32(timeRange.Remove(timeRange.Length - 1));
                    timestamp = DateTime.UtcNow.AddMinutes(-minutes);
                    break;
                case "1h":
                case "6h":
                    var hours = Convert.ToInt32(timeRange.Remove(timeRange.Length - 1));
                    timestamp = DateTime.UtcNow.AddHours(-hours);
                    aggregate = 60 * 5;
                    break;
                case "1d":
                case "3d":
                case "7d":
                case "30d":
                    var days = Convert.ToInt32(timeRange.Remove(timeRange.Length - 1));
                    timestamp = DateTime.UtcNow.AddDays(-days);
                    aggregate = days > 7 ? 60 * 60 : 60 * 10;
                    break;
                default:
                    timestamp = DateTime.UtcNow;
                    break;
            }

            var settings = new QueryableOptimizerSettings
            {
                DropMeasurementColumn = false,
                AlignFieldsWithPivot = true
            };

            var query = from s in InfluxDBQueryable<MeasurementPoint>.Queryable(bucket,
                    Client.Org, influxDbClient.GetQueryApiSync(), settings)
                where s.DeviceId == deviceId
                where s.Timestamp > timestamp
                where s.Timestamp.AggregateWindow(TimeSpan.FromSeconds(aggregate), null, "mean")
                orderby s.Timestamp
                select s;

            return Task.FromResult(query.ToList());
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to load data. - " + e);
            return Task.FromResult(new List<MeasurementPoint>());
        }
    }

    #endregion ---------------------------------- Linq ----------------------------------

    #region Write emulated data

    public static async Task WriteEmulatedData(string? deviceId, string? bucket)
    {
        var orgId = await GetOrganizationId(Client);
        using var influxDbClient = Client.GetClient(100);
        try
        {
            var toTime = Math.Truncate((double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 60000) * 60000;
            var lastTime = Math.Truncate((double)DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds() / 60000) *
                           60000;

            var totalPoints = (toTime - lastTime) / 60000;

            if (totalPoints > 0)
            {
                var writeApi = influxDbClient.GetWriteApi();

                while (lastTime < toTime)
                {
                    lastTime += 60000; // emulate next minute

                    var point = PointData.Measurement("environment")
                        .Tag("clientId", deviceId)
                        .Field("Temperature", _generateValue(30, 0, 40, lastTime))
                        .Field("Humidity", _generateValue(90, 0, 99, lastTime))
                        .Field("Pressure", _generateValue(20, 970, 1050, lastTime))
                        .Field("CO2", Convert.ToInt32(_generateValue(1, 400, 3000, lastTime)))
                        .Field("TVOC", Convert.ToInt32(_generateValue(1, 250, 2000, lastTime)))
                        .Tag("TemperatureSensor", "virtual_TemperatureSensor")
                        .Tag("HumiditySensor", "virtual_HumiditySensor")
                        .Tag("PressureSensor", "virtual_PressureSensor")
                        .Tag("CO2Sensor", "virtual_CO2Sensor")
                        .Tag("TVOCSensor", "virtual_TVOCSensor")
                        .Tag("GPSSensor", "virtual_GPSSensor")
                        .Timestamp(Convert.ToInt64(lastTime), WritePrecision.Ms);

                    writeApi.WritePoint(point, bucket, orgId);
                }
            }
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to write emulated data.");
        }
    }

    private const int DayMillis = 24 * 60 * 60 * 1000;
    private static readonly Random Rnd = new();

    private static double _generateValue(int period, double min, double max, double time)
    {
        var dif = max - min;
        var periodValue =
            dif / 4 * Math.Sin(time / DayMillis % period / period * 2 * Math.PI);
        var dayValue =
            dif / 4 * Math.Sin(time % DayMillis / DayMillis * 2 * Math.PI - Math.PI / 2);
        var result = min +
                     dif / 2 +
                     periodValue +
                     dayValue +
                     Rnd.NextDouble() * 10;

        return result;
    }

    #endregion Write emulated data
}