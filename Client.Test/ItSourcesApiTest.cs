using System;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItSourcesApiTest : AbstractItClientTest
    {
        [SetUp]
        public new void SetUp()
        {
            _sourcesApi = Client.GetSourcesApi();
        }

        private SourcesApi _sourcesApi;

        private Source NewSource()
        {
            var source = new Source
            {
                Name = GenerateName("Source"),
                OrgID = "02cebf26d7fc1000",
                Type = Source.TypeEnum.V1,
                Url = "http://influxdb:8086",
                InsecureSkipVerify = true
            };

            return source;
        }

        [Test]
        public void CloneSource()
        {
            var source = _sourcesApi.CreateSource(NewSource());

            var name = GenerateName("cloned");

            var cloned = _sourcesApi.CloneSource(name, source);

            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual(source.OrgID, cloned.OrgID);
            Assert.AreEqual(source.DefaultRP, cloned.DefaultRP);
            Assert.AreEqual(source.Type, cloned.Type);
            Assert.AreEqual(source.Url, cloned.Url);
            Assert.AreEqual(source.InsecureSkipVerify, cloned.InsecureSkipVerify);
            Assert.AreEqual(source.Telegraf, cloned.Telegraf);
            Assert.AreEqual(source.Token, cloned.Token);
            Assert.AreEqual(source.Username, cloned.Username);
            Assert.AreEqual(source.Password, cloned.Password);
            Assert.AreEqual(source.SharedSecret, cloned.SharedSecret);
            Assert.AreEqual(source.MetaUrl, cloned.MetaUrl);
            Assert.AreEqual(source.Default, cloned.Default);
        }

        [Test]
        public void CloneSourceNotFound()
        {
            var ioe = Assert.Throws<HttpException>(() =>
                _sourcesApi.CloneSource(GenerateName("bucket"), "020f755c3d082000"));

            Assert.AreEqual("source not found", ioe.Message);
        }

        [Test]
        public void CreateSource()
        {
            var source = new Source
            {
                OrgID = "02cebf26d7fc1000",
                Default = false,
                Name = GenerateName("Source"),
                Type = Source.TypeEnum.V1,
                Url = "http://localhost:8086",
                InsecureSkipVerify = true,
                Telegraf = "telegraf",
                Token = Guid.NewGuid().ToString(),
                Username = "admin",
                Password = "password",
                SharedSecret = Guid.NewGuid().ToString(),
                MetaUrl = "/usr/local/var/influxdb/meta",
                DefaultRP = "autogen"
            };


            var createdSource = _sourcesApi.CreateSource(source);

            Assert.IsNotEmpty(createdSource.Id);
            Assert.AreEqual(createdSource.OrgID, source.OrgID);
            Assert.AreEqual(createdSource.Default, source.Default);
            Assert.AreEqual(createdSource.Name, source.Name);
            Assert.AreEqual(createdSource.Type, source.Type);
            Assert.AreEqual(createdSource.Url, source.Url);
            Assert.AreEqual(createdSource.InsecureSkipVerify, source.InsecureSkipVerify);
            Assert.AreEqual(createdSource.Telegraf, source.Telegraf);
            Assert.AreEqual(createdSource.Token, source.Token);
            Assert.AreEqual(createdSource.Username, source.Username);
            Assert.IsNull(createdSource.Password);
            Assert.IsNull(createdSource.SharedSecret);
            Assert.AreEqual(createdSource.MetaUrl, source.MetaUrl);
            Assert.AreEqual(createdSource.Default, source.Default);
        }

        [Test]
        public void DeleteSource()
        {
            var createdSource = _sourcesApi.CreateSource(NewSource());
            Assert.IsNotNull(createdSource);

            var foundSource = _sourcesApi.FindSourceById(createdSource.Id);
            Assert.IsNotNull(foundSource);

            // delete source
            _sourcesApi.DeleteSource(createdSource);

            var nfe = Assert.Throws<HttpException>(() =>
                _sourcesApi.FindSourceById(createdSource.Id));

            Assert.IsNotNull(nfe);
            Assert.AreEqual("source not found", nfe.Message);
        }

        [Test]
        public void FindBucketsBySource()
        {
            var source = _sourcesApi.CreateSource(NewSource());

            var buckets = _sourcesApi.FindBucketsBySource(source);

            Assert.IsNotNull(buckets);
            Assert.IsTrue(buckets.Count > 0);
        }

        [Test]
        public void FindBucketsBySourceByUnknownSource()
        {
            var nfe = Assert.Throws<HttpException>(() => _sourcesApi.FindBucketsBySourceId("020f755c3d082000"));

            Assert.IsNotNull(nfe);
            Assert.AreEqual("source not found", nfe.Message);
        }

        [Test]
        public void FindSourceById()
        {
            var source = _sourcesApi.CreateSource(NewSource());

            var sourceById = _sourcesApi.FindSourceById(source.Id);

            Assert.IsNotNull(sourceById);
            Assert.AreEqual(source.Id, sourceById.Id);
            Assert.AreEqual(source.Name, sourceById.Name);
            Assert.AreEqual(source.OrgID, sourceById.OrgID);
            Assert.AreEqual(source.Type, sourceById.Type);
            Assert.AreEqual(source.Url, sourceById.Url);
            Assert.AreEqual(source.InsecureSkipVerify, sourceById.InsecureSkipVerify);
        }

        [Test]
        public void FindSourceByIdNull()
        {
            var nfe = Assert.Throws<HttpException>(() => _sourcesApi.FindSourceById("020f755c3d082000"));

            Assert.IsNotNull(nfe);
            Assert.AreEqual("source not found", nfe.Message);
        }

        [Test]
        public void FindSources()
        {
            var size = (_sourcesApi.FindSources()).Count;

            _sourcesApi.CreateSource(NewSource());

            var sources = _sourcesApi.FindSources();
            Assert.AreEqual(size + 1, sources.Count);
        }

        [Test]
        public void SourceHealth()
        {
            var source = _sourcesApi.CreateSource(NewSource());

            var health = _sourcesApi.Health(source);

            Assert.IsNotNull(health);
            Assert.AreEqual(Check.StatusEnum.Pass, health.Status);
            Assert.AreEqual("source is healthy", health.Message);
        }


        [Test]
        public void UpdateSource()
        {
            var source = NewSource();
            source.InsecureSkipVerify = false;

            source = _sourcesApi.CreateSource(source);
            Assert.IsNull(source.InsecureSkipVerify);
            
            source.InsecureSkipVerify = true;
            source = _sourcesApi.UpdateSource(source);

            Assert.IsTrue(source.InsecureSkipVerify);
        }
    }
}