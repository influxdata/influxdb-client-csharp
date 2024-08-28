using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItErrorEventsTest : AbstractItClientTest
    {
        private Organization _org;
        private Bucket _bucket;
        private string _token;
        private InfluxDBClientOptions _options;

        [SetUp]
        public new async Task SetUp()
        {
            _org = await FindMyOrg();
            _bucket = await Client.GetBucketsApi()
                .CreateBucketAsync(GenerateName("fliers"), null, _org);

            //
            // Add Permissions to read and write to the Bucket
            //
            var resource = new PermissionResource(PermissionResource.TypeBuckets, _bucket.Id, null,
                _org.Id);

            var readBucket = new Permission(Permission.ActionEnum.Read, resource);
            var writeBucket = new Permission(Permission.ActionEnum.Write, resource);

            var loggedUser = await Client.GetUsersApi().MeAsync();
            Assert.IsNotNull(loggedUser);

            var authorization = await Client.GetAuthorizationsApi()
                .CreateAuthorizationAsync(_org,
                    new List<Permission> { readBucket, writeBucket });

            _token = authorization.Token;

            Client.Dispose();

            _options = new InfluxDBClientOptions(InfluxDbUrl)
            {
                Token = _token,
                Org = _org.Id,
                Bucket = _bucket.Id
            };

            Client = new InfluxDBClient(_options);
        }


        [Test]
        public void HandleEvents()
        {
            using (var writeApi = Client.GetWriteApi())
            {
                writeApi.EventHandler += (sender, eventArgs) =>
                {
                    switch (eventArgs)
                    {
                        case WriteSuccessEvent successEvent:
                            Assert.Fail("Call should not succeed");
                            break;
                        case WriteErrorEvent errorEvent:
                            Assert.AreEqual("unable to parse 'velocity,unit=C3PO mps=': missing field value",
                                errorEvent.Exception.Message);
                            var eventHeaders = errorEvent.GetHeaders();
                            if (eventHeaders == null)
                            {
                                Assert.Fail("WriteErrorEvent must return headers.");
                            }

                            var headers = new Dictionary<string, string> { };
                            foreach (var hp in eventHeaders)
                            {
                                Console.WriteLine("DEBUG {0}: {1}", hp.Name, hp.Value);
                                headers.Add(hp.Name, hp.Value);
                            }

                            Assert.AreEqual(4, headers.Count);
                            Assert.AreEqual("OSS", headers["X-Influxdb-Build"]);
                            Assert.True(headers["X-Influxdb-Version"].StartsWith('v'));
                            Assert.AreEqual("invalid", headers["X-Platform-Error-Code"]);
                            Assert.AreNotEqual("missing", headers.GetValueOrDefault("Date", "missing"));
                            break;
                        case WriteRetriableErrorEvent retriableErrorEvent:
                            Assert.Fail("Call should not be retriable.");
                            break;
                        case WriteRuntimeExceptionEvent runtimeExceptionEvent:
                            Assert.Fail("Call should not result in runtime exception. {0}", runtimeExceptionEvent);
                            break;
                    }
                };

                writeApi.WriteRecord("velocity,unit=C3PO mps=", WritePrecision.S, _bucket.Name, _org.Name);
            }
        }
    }
}