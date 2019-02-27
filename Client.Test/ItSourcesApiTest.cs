using System;
using InfluxDB.Client.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItSourcesApiTest : AbstractItClientTest
    {
        private SourcesApi _sourcesApi;

        [SetUp]
        public new void SetUp()
        {
            _sourcesApi = Client.GetSourcesApi();
        }

        [Test]
        public async Task CreateSource()
        {
            var source = new Source
            {
                OrgId = "02cebf26d7fc1000",
                DefaultSource = false,
                Name = GenerateName("Source"),
                Type = Source.SourceType.V1SourceType,
                Url = "http://localhost:8086",
                InsecureSkipVerify = true,
                Telegraf = "telegraf",
                Token = Guid.NewGuid().ToString(),
                UserName = "admin",
                Password = "password",
                SharedSecret = Guid.NewGuid().ToString(),
                MetaUrl = "/usr/local/var/influxdb/meta",
                DefaultRp = "autogen"
            };


            var createdSource = await _sourcesApi.CreateSource(source);

            Assert.IsNotEmpty(createdSource.Id);
            Assert.AreEqual(createdSource.OrgId, source.OrgId);
            Assert.AreEqual(createdSource.DefaultSource, source.DefaultSource);
            Assert.AreEqual(createdSource.Name, source.Name);
            Assert.AreEqual(createdSource.Type, source.Type);
            Assert.AreEqual(createdSource.Url, source.Url);
            Assert.AreEqual(createdSource.InsecureSkipVerify, source.InsecureSkipVerify);
            Assert.AreEqual(createdSource.Telegraf, source.Telegraf);
            Assert.AreEqual(createdSource.Token, source.Token);
            Assert.AreEqual(createdSource.UserName, source.UserName);
            Assert.IsNull(createdSource.Password);
            Assert.IsNull(createdSource.SharedSecret);
            Assert.AreEqual(createdSource.MetaUrl, source.MetaUrl);
            Assert.AreEqual(createdSource.DefaultRp, source.DefaultRp);
        }


        [Test]
        public async Task UpdateSource()
        {
            var source = NewSource();

            source = await _sourcesApi.CreateSource(source);
            source.InsecureSkipVerify = false;

            source = await _sourcesApi.UpdateSource(source);

            Assert.IsFalse(source.InsecureSkipVerify);
        }

        [Test]
        public async Task DeleteSource()
        {
            var createdSource = await _sourcesApi.CreateSource(NewSource());
            Assert.IsNotNull(createdSource);
            
            var foundSource = await _sourcesApi.FindSourceById(createdSource.Id);
            Assert.IsNotNull(foundSource);

            // delete source
            await _sourcesApi.DeleteSource(createdSource);

            foundSource = await _sourcesApi.FindSourceById(createdSource.Id);
            Assert.IsNull(foundSource);
        }
        
        [Test]
        public async Task FindSourceById() {

            var source = await _sourcesApi.CreateSource(NewSource());

            var sourceById = await _sourcesApi.FindSourceById(source.Id);

            Assert.IsNotNull(sourceById);
            Assert.AreEqual(source.Id, sourceById.Id);
            Assert.AreEqual(source.Name, sourceById.Name);
            Assert.AreEqual(source.OrgId, sourceById.OrgId);
            Assert.AreEqual(source.Type, sourceById.Type);
            Assert.AreEqual(source.Url, sourceById.Url);
            Assert.AreEqual(source.InsecureSkipVerify, sourceById.InsecureSkipVerify);
        }

        [Test]
        public async Task FindSourceByIdNull() {

            var source =  await _sourcesApi.FindSourceById("020f755c3d082000");

            Assert.IsNull(source);
        }
        
        [Test]
        public async Task FindSources() {

            var size = (await _sourcesApi.FindSources()).Count;

            await _sourcesApi.CreateSource(NewSource());

            var sources = await _sourcesApi.FindSources();
            Assert.AreEqual(size + 1, sources.Count);
        }
        
        [Test]
        public async Task FindBucketsBySource() {

            var source = await _sourcesApi.CreateSource(NewSource());

            var buckets = await _sourcesApi.FindBucketsBySource(source);

            Assert.IsNotNull(buckets);
            Assert.IsTrue(buckets.Count > 0);
        }

        [Test]
        public async Task FindBucketsBySourceByUnknownSource() {

            var buckets = await _sourcesApi.FindBucketsBySourceId("020f755c3d082000");

            Assert.IsNull(buckets);
        }
        
        [Test]
        public async Task SourceHealth() {

            var source = await _sourcesApi.CreateSource(NewSource());

            var health = await _sourcesApi.Health(source);

            Assert.IsNotNull(health);
            Assert.IsTrue(health.IsHealthy());
            Assert.AreEqual("source is healthy", health.Message);
        }
        
        [Test]
        public async Task CloneSource()
        {
            var source = await _sourcesApi.CreateSource(NewSource());

            var name = GenerateName("cloned");
            
            var cloned = await _sourcesApi.CloneSource(name, source);
            
            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual(source.OrgId, cloned.OrgId);
            Assert.AreEqual(source.DefaultRp, cloned.DefaultRp);
            Assert.AreEqual(source.Type, cloned.Type);
            Assert.AreEqual(source.Url, cloned.Url);
            Assert.AreEqual(source.InsecureSkipVerify, cloned.InsecureSkipVerify);
            Assert.AreEqual(source.Telegraf, cloned.Telegraf);
            Assert.AreEqual(source.Token, cloned.Token);
            Assert.AreEqual(source.UserName, cloned.UserName);
            Assert.AreEqual(source.Password, cloned.Password);
            Assert.AreEqual(source.SharedSecret, cloned.SharedSecret);
            Assert.AreEqual(source.MetaUrl, cloned.MetaUrl);
            Assert.AreEqual(source.DefaultSource, cloned.DefaultSource);
        }

        [Test]
        public void CloneSourceNotFound()
        {
            var ioe = Assert.ThrowsAsync<InvalidOperationException>(async () => await _sourcesApi.CloneSource(GenerateName("bucket"),"020f755c3d082000"));
            
            Assert.AreEqual("NotFound Source with ID: 020f755c3d082000", ioe.Message);
        }

        private Source NewSource()
        {
            var source = new Source
            {
                Name = GenerateName("Source"),
                OrgId = "02cebf26d7fc1000",
                Type = Source.SourceType.V1SourceType,
                Url = "http://influxdb:8086",
                InsecureSkipVerify = true
            };

            return source;
        }
    }
}