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
    }
}