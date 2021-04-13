using System;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

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
        public async Task CloneSource()
        {
            var source = await _sourcesApi.CreateSourceAsync(NewSource());

            var name = GenerateName("cloned");

            var cloned = await _sourcesApi.CloneSourceAsync(name, source);

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
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _sourcesApi.CloneSourceAsync(GenerateName("bucket"), "020f755c3d082000"));

            Assert.AreEqual("source not found", ioe.Message);
            Assert.AreEqual(typeof(NotFoundException), ioe.GetType());
        }

        [Test]
        public async Task CreateSource()
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


            var createdSource = await _sourcesApi.CreateSourceAsync(source);

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
        public async Task DeleteSource()
        {
            var createdSource = await _sourcesApi.CreateSourceAsync(NewSource());
            Assert.IsNotNull(createdSource);

            var foundSource = await _sourcesApi.FindSourceByIdAsync(createdSource.Id);
            Assert.IsNotNull(foundSource);

            // delete source
            await _sourcesApi.DeleteSourceAsync(createdSource);

            var nfe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _sourcesApi.FindSourceByIdAsync(createdSource.Id));

            Assert.IsNotNull(nfe);
            Assert.AreEqual("source not found", nfe.Message);
        }

        [Test]
        public async Task FindBucketsBySource()
        {
            var source = await _sourcesApi.CreateSourceAsync(NewSource());

            var buckets = await _sourcesApi.FindBucketsBySourceAsync(source);

            Assert.IsNotNull(buckets);
            Assert.IsTrue(buckets.Count > 0);
        }

        [Test]
        public void FindBucketsBySourceByUnknownSource()
        {
            var nfe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _sourcesApi.FindBucketsBySourceIdAsync("020f755c3d082000"));

            Assert.IsNotNull(nfe);
            Assert.AreEqual("source not found", nfe.Message);
            Assert.AreEqual(typeof(NotFoundException), nfe.GetType());
        }

        [Test]
        public async Task FindSourceById()
        {
            var source = await _sourcesApi.CreateSourceAsync(NewSource());

            var sourceById = await _sourcesApi.FindSourceByIdAsync(source.Id);

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
            var nfe = Assert.ThrowsAsync<NotFoundException>(
                async () => await _sourcesApi.FindSourceByIdAsync("020f755c3d082000"));

            Assert.IsNotNull(nfe);
            Assert.AreEqual("source not found", nfe.Message);
        }

        [Test]
        public async Task FindSources()
        {
            var size = (await _sourcesApi.FindSourcesAsync()).Count;

            await _sourcesApi.CreateSourceAsync(NewSource());

            var sources = await _sourcesApi.FindSourcesAsync();
            Assert.AreEqual(size + 1, sources.Count);
        }

        [Test]
        public async Task SourceHealth()
        {
            var source = await _sourcesApi.CreateSourceAsync(NewSource());

            var health = await _sourcesApi.HealthAsync(source);

            Assert.IsNotNull(health);
            Assert.AreEqual(HealthCheck.StatusEnum.Pass, health.Status);
            Assert.AreEqual("source is healthy", health.Message);
        }


        [Test]
        public async Task UpdateSource()
        {
            var source = NewSource();
            source.InsecureSkipVerify = false;

            source = await _sourcesApi.CreateSourceAsync(source);
            Assert.IsNull(source.InsecureSkipVerify);

            source.InsecureSkipVerify = true;
            source = await _sourcesApi.UpdateSourceAsync(source);

            Assert.IsTrue(source.InsecureSkipVerify);
        }
    }
}