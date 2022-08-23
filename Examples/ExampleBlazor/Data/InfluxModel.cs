using System.Diagnostics;
using System.Globalization;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Flux.Domain;
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
        var influxDbClient = client?.GetClient();
        return await influxDbClient?.PingAsync()!;
    }

    public static async Task<string> GetOrganizationId(Client client)
    {
        var influxDbClient = client.GetClient();
        try
        {
            var orgList = await influxDbClient.GetOrganizationsApi().FindOrganizationsAsync(org: client.Org);

            var tmp = orgList.First();
            return (await influxDbClient.GetOrganizationsApi().FindOrganizationsAsync(org: client.Org))
                .First().Id;
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to load organization Id." + e);
        }
        finally
        {
            influxDbClient.Dispose();
        }

        return "";
    }

    #region ---------------------------------- Buckets ----------------------------------

    public static async Task<string> GetBucketId(string bucketName)
    {
        var influxDbClient = Client.GetClient();
        try
        {
            return (await influxDbClient.GetBucketsApi().FindBucketByNameAsync(bucketName)).Id;
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to load bucket Id.");
        }
        finally
        {
            influxDbClient.Dispose();
        }

        return "";
    }

    public static async Task DeleteBucket(Bucket bucket)
    {
        var influxDbClient = Client.GetClient();
        try
        {
            await influxDbClient.GetBucketsApi().DeleteBucketAsync(bucket);
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to delete bucket.");
        }
        finally
        {
            influxDbClient.Dispose();
        }
    }

    public static async Task CloneBucket(Bucket bucket, string name)
    {
        var influxDbClient = Client.GetClient();
        try
        {
            await influxDbClient.GetBucketsApi().CloneBucketAsync(name, bucket);
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to clone bucket.");
        }
        finally
        {
            influxDbClient.Dispose();
        }
    }

    public static async Task<Bucket?> CreateBucket(string name)
    {
        var orgId = await GetOrganizationId(Client);
        var influxDbClient = Client.GetClient();
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
        }
        finally
        {
            influxDbClient.Dispose();
        }

        return null;
    }

    public static async Task FindBucketByName(string bucketName)
    {
        var influxDbClient = Client.GetClient();

        await influxDbClient.GetBucketsApi().FindBucketByNameAsync(bucketName);

        influxDbClient.Dispose();
    }

    public static async Task FindBucketById(string bucketId)
    {
        var influxDbClient = Client.GetClient();

        await influxDbClient.GetBucketsApi().FindBucketByIdAsync(bucketId);

        influxDbClient.Dispose();
    }

    public static async Task FindBucketsByOrgName(string orgName)
    {
        var influxDbClient = Client.GetClient();

        await influxDbClient.GetBucketsApi().FindBucketsByOrgNameAsync(orgName);

        influxDbClient.Dispose();
    }

    public static async Task<List<Bucket>> FetchBuckets()
    {
        var influxDbClient = Client.GetClient();

        var bucketList = await influxDbClient.GetBucketsApi().FindBucketsAsync();

        influxDbClient.Dispose();

        return bucketList;
    }

    #endregion ---------------------------------- Buckets ----------------------------------

    #region ---------------------------------- Devices ----------------------------------

    public static async Task<List<FluxTable>?> FetchDeviceList(string bucket)
    {
        var orgId = await GetOrganizationId(Client);
        var influxDbClient = Client.GetClient();
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
        }
        finally
        {
            influxDbClient.Dispose();
        }

        return null;
    }

    public static async Task<DateTime> FetchDeviceCreatedAt(string bucket, string deviceId)
    {
        var orgId = await GetOrganizationId(Client);
        var influxDbClient = Client.GetClient();
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
        finally
        {
            influxDbClient.Dispose();
        }
    }

    public static async Task CreateDevice(string deviceId, string deviceType, string bucket)
    {
        var orgId = await GetOrganizationId(Client);
        var influxDbClient = Client.GetClient();
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
        finally
        {
            influxDbClient.Dispose();
        }
    }

    private static async Task<Authorization?> _createDeviceAuthorization(string deviceId, string bucket, string orgId)
    {
        var authorization = await _createIoTAuthorization(deviceId, bucket, orgId);
        var influxDbClient = Client.GetClient();

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
                return authorization;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to create authorization. - " + e);
            }
            finally
            {
                influxDbClient.Dispose();
            }
        }

        return authorization;
    }

    private static async Task<Authorization?> _createIoTAuthorization(string deviceId, string bucket, string orgId)
    {
        var bucketId = await GetBucketId(bucket);
        var influxDbClient = Client.GetClient();
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
        }
        finally
        {
            influxDbClient.Dispose();
        }

        return null;
    }

    public static async Task<bool> RemoveDevice(FluxRecord device, string bucket)
    {
        var orgId = await GetOrganizationId(Client);
        var influxDbClient = Client.GetClient();
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
        }
        finally
        {
            influxDbClient.Dispose();
        }

        return false;
    }


    private static async Task<bool> _removeDeviceAuthorization(string deviceId, string key, string bucket, string orgId)
    {
        if (!string.IsNullOrEmpty(key))
        {
            await _deleteIoTAuthorization(key);

            var influxDbClient = Client.GetClient();
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
            finally
            {
                influxDbClient.Dispose();
            }
        }

        return false;
    }

    private static async Task _deleteIoTAuthorization(string key)
    {
        var influxDbClient = Client.GetClient();
        var authorizationApi = influxDbClient.GetAuthorizationsApi();

        try
        {
            await authorizationApi.DeleteAuthorizationAsync(key);
        }
        catch (Exception e)
        {
            Debug.WriteLine("Failed to delete authorization. - " + e);
        }
        finally
        {
            influxDbClient.Dispose();
        }
    }

    #endregion ---------------------------------- Devices ----------------------------------

    public static async Task<List<FluxTable>?> FetchData(string bucket, string timeRange, string measurement)
    {
        var orgId = await GetOrganizationId(Client);
        var influxDbClient = Client.GetClient();
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
        }
        finally
        {
            influxDbClient.Dispose();
        }

        return null;
    }

    public static async Task<List<FluxTable?>> FetchData(string? bucket, string? deviceId, string timeRange,
        string measurement, string field)
    {
        var orgId = await GetOrganizationId(Client);
        var influxDbClient = Client.GetClient(100);
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
        finally
        {
            influxDbClient.Dispose();
        }
    }

    public static async Task<FluxTable?> FetchDataMean(string? bucket, string? deviceId, string timeRange,
        string measurement, string field)
    {
        var orgId = await GetOrganizationId(Client);
        var influxDbClient = Client.GetClient();
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
        finally
        {
            influxDbClient.Dispose();
        }
    }

    public static async Task<List<string?>?> FetchMeasurements(string bucket)
    {
        var orgId = await GetOrganizationId(Client);
        var influxDbClient = Client.GetClient();
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
        }
        finally
        {
            influxDbClient.Dispose();
        }

        return null;
    }

    public static async Task<List<FluxTable>?> FetchMeasurements(string bucket, string? deviceId)
    {
        var orgId = await GetOrganizationId(Client);
        var influxDbClient = Client.GetClient(100);
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
        finally
        {
            influxDbClient.Dispose();
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
        finally
        {
            influxDbClient.Dispose();
        }
    }

    #region Write emulated data

    public static async Task WriteEmulatedData(string? deviceId, string? bucket)
    {
        var orgId = await GetOrganizationId(Client);
        var influxDbClient = Client.GetClient(100);
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
                        .Field("Humidity", _generateValue(60, 0, 99, lastTime))
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
        finally
        {
            influxDbClient.Dispose();
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
        if (result > max)
        {
            result -= (result - max) * 2;
        }
        else if (result < min)
        {
            result += (min - result) * 2;
        }

        return result;
    }

    #endregion Write emulated data
}