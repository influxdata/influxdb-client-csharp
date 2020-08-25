using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItDeleteApiTest : AbstractItClientTest
    {
        [SetUp]
        public new async Task SetUp()
        {
            _organization = await FindMyOrg();

            var retention = new BucketRetentionRules(BucketRetentionRules.TypeEnum.Expire, 3600);

            _bucket = await Client.GetBucketsApi()
                            .CreateBucketAsync(GenerateName("h2o"), retention, _organization);

            //
            // Add Permissions to read and write to the Bucket
            //
            var resource =
                            new PermissionResource(PermissionResource.TypeEnum.Buckets, _bucket.Id, null,
                                            _organization.Id);

            var readBucket = new Permission(Permission.ActionEnum.Read, resource);
            var writeBucket = new Permission(Permission.ActionEnum.Write, resource);

            var loggedUser = await Client.GetUsersApi().MeAsync();
            Assert.IsNotNull(loggedUser);

            var authorization = await Client.GetAuthorizationsApi()
                            .CreateAuthorizationAsync(await FindMyOrg(),
                                            new List<Permission> {readBucket, writeBucket});

            _token = authorization.Token;

            Client.Dispose();
            var options = new InfluxDBClientOptions.Builder().Url(InfluxDbUrl).AuthenticateToken(_token)
                            .Org(_organization.Id).Bucket(_bucket.Id).Build();
            Client = InfluxDBClientFactory.Create(options);
            _queryApi = Client.GetQueryApi();
        }

        [TearDown]
        protected new void After()
        {
            _writeApi.Dispose();
        }

        private Bucket _bucket;
        private QueryApi _queryApi;
        private WriteApi _writeApi;
        private DeleteApi _deleteApi;
        private Organization _organization;
        private string _token;

        [Measurement("h2o")]
        private class H20Measurement
        {
            [Column("location", IsTag = true)] public string Location { get; set; }

            [Column("level")] public double? Level { get; set; }

            [Column(IsTimestamp = true)] public DateTime Time { get; set; }
        }

        [Test]
        public async Task Delete()
        {
            Client.Dispose();

            WriteData();

            string query = "from(bucket:\"" + _bucket.Name +
                           "\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";

            _queryApi = Client.GetQueryApi();
            var tables = await _queryApi.QueryAsync(query, _organization.Id);

            Assert.AreEqual(2, tables.Count);
            Assert.AreEqual(1, tables[0].Records.Count);
            Assert.AreEqual(5, tables[1].Records.Count);

            _deleteApi = Client.GetDeleteApi();

            await _deleteApi.Delete(DateTime.Now.AddHours(-1), DateTime.Now, "", _bucket, _organization);

            var tablesAfterDelete1 = await _queryApi.QueryAsync(query, _organization.Id);

            Assert.AreEqual(0, tablesAfterDelete1.Count);
        }

        [Test]
        public async Task DeleteWithPredicate()
        {
            Client.Dispose();

            WriteData();
            
            string query = "from(bucket:\"" + _bucket.Name +
                           "\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";

            _queryApi = Client.GetQueryApi();
            var tables = await _queryApi.QueryAsync(query, _organization.Id);

            Assert.AreEqual(2, tables.Count);
            Assert.AreEqual(1, tables[0].Records.Count);
            Assert.AreEqual(5, tables[1].Records.Count);

            var location = "east";
            var field = "water_level";
            var predicate = $"location=\"{location}\" AND _field=\"{field}\"";

            _deleteApi = Client.GetDeleteApi();
            await _deleteApi.Delete(DateTime.Now.AddHours(-1), DateTime.Now, predicate, _bucket, _organization);
            
            var tablesAfterDelete = await _queryApi.QueryAsync(query, _organization.Id);
            
            Assert.AreEqual(1, tablesAfterDelete.Count);
            Assert.AreEqual(5, tablesAfterDelete[0].Records.Count);
            
            await _deleteApi.Delete(DateTime.Now.AddHours(-1), DateTime.Now, "location = \"west\"", _bucket, _organization);
            
            var tablesAfterDelete2 = await _queryApi.QueryAsync(query, _organization.Id);
            
            Assert.AreEqual(0, tablesAfterDelete2.Count);
        }

        private void WriteData()
        {
            Environment.SetEnvironmentVariable("point-datacenter", "LA");
            ConfigurationManager.AppSettings["point-sensor.version"] = "1.23a";

            var options = new InfluxDBClientOptions.Builder().Url(InfluxDbUrl)
                            .AuthenticateToken(_token)
                            .AddDefaultTag("id", "132-987-655")
                            .AddDefaultTag("customer", "California Miner")
                            .AddDefaultTag("env-var", "${env.point-datacenter}")
                            .AddDefaultTag("sensor-version", "${point-sensor.version}")
                            .Build();

            Client = InfluxDBClientFactory.Create(options);

            var point = PointData.Measurement("h2o_feet").Tag("location", "east").Field("water_level", 1);
            var point2 = PointData.Measurement("h2o_feet").Tag("location", "west").Field("water_level", 2);
            var point3 = PointData.Measurement("h2o_feet").Tag("location", "west").Field("water_level", 3);
            var point4 = PointData.Measurement("h2o_feet").Tag("location", "west").Field("water_level", 4);
            var point5 = PointData.Measurement("h2o_feet").Tag("location", "west").Field("water_level", 5);
            var point6 = PointData.Measurement("h2o_feet").Tag("location", "west").Field("water_level", 6);

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WritePoint(_bucket.Name, _organization.Id, point);
            _writeApi.Flush();

            listener.WaitToSuccess();

            _writeApi.WritePoint(_bucket.Name, _organization.Id, point2);
            _writeApi.Flush();

            listener.WaitToSuccess();

            _writeApi.WritePoint(_bucket.Name, _organization.Id, point3);
            _writeApi.Flush();

            listener.WaitToSuccess();

            _writeApi.WritePoint(_bucket.Name, _organization.Id, point4);
            _writeApi.Flush();

            listener.WaitToSuccess();

            _writeApi.WritePoint(_bucket.Name, _organization.Id, point5);
            _writeApi.Flush();

            listener.WaitToSuccess();

            _writeApi.WritePoint(_bucket.Name, _organization.Id, point6);
            _writeApi.Flush();

            listener.WaitToSuccess();
        }
    }
}