using System;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    public class ItSourceClientTest : AbstractItClientTest
    {
        private SourceClient _sourceClient;

        [SetUp]
        public new void SetUp()
        {
            _sourceClient = PlatformClient.CreateSourceClient();
        }

        [Test]
        public async Task CreateSource()
        {
            Source source = new Source
            {
                OrganizationId = "02cebf26d7fc1000",
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


            Source createdSource = await _sourceClient.CreateSource(source);

            Assert.IsNotEmpty(createdSource.Id);
            Assert.AreEqual(createdSource.OrganizationId, source.OrganizationId);
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
            Source source = NewSource();

            source = await _sourceClient.CreateSource(source);
            source.InsecureSkipVerify = false;

            source = await _sourceClient.UpdateSource(source);

            Assert.IsFalse(source.InsecureSkipVerify);
        }

        [Test]
        public async Task DeleteSource()
        {
            Source createdSource = await _sourceClient.CreateSource(NewSource());
            Assert.IsNotNull(createdSource);
            
            Source foundSource = await _sourceClient.FindSourceById(createdSource.Id);
            Assert.IsNotNull(foundSource);

            // delete source
            await _sourceClient.DeleteSource(createdSource);

            foundSource = await _sourceClient.FindSourceById(createdSource.Id);
            Assert.IsNull(foundSource);
        }
        
        [Test]
        public async Task FindSourceById() {

            Source source = await _sourceClient.CreateSource(NewSource());

            Source sourceById = await _sourceClient.FindSourceById(source.Id);

            Assert.IsNotNull(sourceById);
            Assert.AreEqual(source.Id, sourceById.Id);
            Assert.AreEqual(source.Name, sourceById.Name);
            Assert.AreEqual(source.OrganizationId, sourceById.OrganizationId);
            Assert.AreEqual(source.Type, sourceById.Type);
            Assert.AreEqual(source.Url, sourceById.Url);
            Assert.AreEqual(source.InsecureSkipVerify, sourceById.InsecureSkipVerify);
        }

        [Test]
        public async Task FindSourceByIdNull() {

            Source source =  await _sourceClient.FindSourceById("020f755c3d082000");

            Assert.IsNull(source);
        }

        private Source NewSource()
        {
            Source source = new Source
            {
                Name = GenerateName("Source"),
                OrganizationId = "02cebf26d7fc1000",
                Type = Source.SourceType.V1SourceType,
                Url = "http://influxdb:8086",
                InsecureSkipVerify = true
            };

            return source;
        }
    }
}