using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Test;
using InfluxDB.Client.Writes;
using NodaTime;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItWriteApiAsyncTest : AbstractItClientTest
    {
        [SetUp]
        public new async Task SetUp()
        {
            _organization = await FindMyOrg();

            //bug: https://github.com/influxdata/influxdb/issues/19518
            var retention = new BucketRetentionRules(BucketRetentionRules.TypeEnum.Expire, 3600);
            _bucket = await Client.GetBucketsApi()
                .CreateBucketAsync(GenerateName("h2o"), null, _organization);

            //
            // Add Permissions to read and write to the Bucket
            //
            var resource = new PermissionResource(PermissionResource.TypeBuckets, _bucket.Id, null,
                _organization.Id);

            var readBucket = new Permission(Permission.ActionEnum.Read, resource);
            var writeBucket = new Permission(Permission.ActionEnum.Write, resource);

            var loggedUser = await Client.GetUsersApi().MeAsync();
            Assert.IsNotNull(loggedUser);

            var authorization = await Client.GetAuthorizationsApi()
                .CreateAuthorizationAsync(_organization,
                    new List<Permission> { readBucket, writeBucket });

            _token = authorization.Token;

            Client.Dispose();
            var options = new InfluxDBClientOptions.Builder().Url(InfluxDbUrl).AuthenticateToken(_token)
                .Org(_organization.Id).Bucket(_bucket.Id).Build();
            Client = InfluxDBClientFactory.Create(options);

            _writeApi = Client.GetWriteApiAsync();
        }

        private Bucket _bucket;
        private Organization _organization;
        private string _token;

        private WriteApiAsync _writeApi;

        [Measurement("h2o")]
        private class H20Measurement
        {
            [Column("location", IsTag = true)] public string Location { get; set; }

            [Column("water_level")] public double? Level { get; set; }

            [Column(IsTimestamp = true)] public DateTime Time { get; set; }
        }

        [Test]
        public async Task Write()
        {
            // By LineProtocol
            await _writeApi.WriteRecordAsync("h2o,location=coyote_creek water_level=1.0 1", WritePrecision.S);
            await _writeApi.WriteRecordAsync("h2o,location=coyote_creek water_level=2.0 2", WritePrecision.S,
                _bucket.Name, _organization.Name);
            await _writeApi.WriteRecordsAsync(new List<string>
            {
                "h2o,location=coyote_creek water_level=3.0 3",
                "h2o,location=coyote_creek water_level=4.0 4"
            }, WritePrecision.S);
            await _writeApi.WriteRecordsAsync(new[]
            {
                "h2o,location=coyote_creek water_level=5.0 5",
                "h2o,location=coyote_creek water_level=6.0 6"
            }, WritePrecision.S, _bucket.Name, _organization.Name);

            // By DataPoint
            await _writeApi.WritePointAsync(PointData.Measurement("h2o").Tag("location", "coyote_creek")
                .Field("water_level", 7.0D).Timestamp(7L, WritePrecision.S));
            await _writeApi.WritePointAsync(PointData.Measurement("h2o").Tag("location", "coyote_creek")
                .Field("water_level", 8.0D)
                .Timestamp(8L, WritePrecision.S), _bucket.Name, _organization.Name);
            await _writeApi.WritePointsAsync(new List<PointData>
            {
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 9.0D)
                    .Timestamp(9L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(10L, WritePrecision.S)
            });
            await _writeApi.WritePointsAsync(new[]
            {
                PointData.Measurement("h2o").Tag("location", "coyote_creek")
                    .Field("water_level", 11.0D)
                    .Timestamp(11L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek")
                    .Field("water_level", 12.0D)
                    .Timestamp(12L, WritePrecision.S)
            }, _bucket.Name, _organization.Name);

            // By Measurement
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            await _writeApi.WriteMeasurementAsync(
                new H20Measurement
                {
                    Location = "coyote_creek", Level = 13.0D, Time = dtDateTime.AddSeconds(13)
                }, WritePrecision.S);

            await _writeApi.WriteMeasurementAsync(new H20Measurement
            {
                Location = "coyote_creek", Level = 14.0D, Time = dtDateTime.AddSeconds(14)
            }, WritePrecision.S, _bucket.Name, _organization.Name);
            await _writeApi.WriteMeasurementsAsync(new List<H20Measurement>
            {
                new H20Measurement
                {
                    Location = "coyote_creek", Level = 15.0D, Time = dtDateTime.AddSeconds(15)
                },
                new H20Measurement
                {
                    Location = "coyote_creek", Level = 16.0D, Time = dtDateTime.AddSeconds(16)
                }
            }, WritePrecision.S);
            await _writeApi.WriteMeasurementsAsync(new[]
            {
                new H20Measurement
                {
                    Location = "coyote_creek", Level = 17.0D, Time = dtDateTime.AddSeconds(17)
                },
                new H20Measurement
                {
                    Location = "coyote_creek", Level = 18.0D, Time = dtDateTime.AddSeconds(18)
                }
            }, WritePrecision.S, _bucket.Name, _organization.Name);

            var query = await Client.GetQueryApi().QueryAsync(
                "from(bucket:\"" + _bucket.Name +
                "\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);

            Assert.AreEqual(1, query.Count);
            Assert.AreEqual(18, query[0].Records.Count);

            for (var ii = 1; ii <= 17; ii++)
            {
                var record = query[0].Records[ii - 1];
                Assert.AreEqual("h2o", record.GetMeasurement());
                Assert.AreEqual((double)ii, record.GetValue());
                Assert.AreEqual("water_level", record.GetField());
                Assert.AreEqual(Instant.FromDateTimeUtc(dtDateTime.AddSeconds(ii)), record.GetTime());
            }
        }

        [Test]
        public async Task WriteULongValues()
        {
            var pointData = PointData.Measurement("h2o")
                .Tag("location", "coyote_creek")
                .Field("max_ulong", ulong.MaxValue)
                .Timestamp(9L, WritePrecision.S);

            await _writeApi.WritePointAsync(pointData);

            var query = await Client.GetQueryApi().QueryAsync(
                "from(bucket:\"" + _bucket.Name +
                "\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);

            Assert.AreEqual(1, query.Count);
            Assert.AreEqual(1, query[0].Records.Count);

            Assert.AreEqual(ulong.MaxValue, query[0].Records[0].GetValue());
        }
    }
}