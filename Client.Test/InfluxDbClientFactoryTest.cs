using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using InfluxDB.Client.Api.Client;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class InfluxDbClientFactoryTest
    {
        private InfluxDBClient _client;

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
        }

        [Test]
        public void CreateInstance()
        {
            _client = InfluxDBClientFactory.Create("http://localhost:9999");

            Assert.IsNotNull(_client);

            var options = GetDeclaredField<InfluxDBClientOptions>(_client.GetType(), _client, "_options");
            Assert.AreEqual(false, options.AllowHttpRedirects);
        }

        [Test]
        public void CreateInstanceVerifySsl()
        {
            _client = InfluxDBClientFactory.Create("http://localhost:9999");

            Assert.IsNotNull(_client);

            var options = GetDeclaredField<InfluxDBClientOptions>(_client.GetType(), _client, "_options");
            Assert.AreEqual(true, options.VerifySsl);
        }

        [Test]
        public void CreateInstanceUsername()
        {
            _client = InfluxDBClientFactory.Create("http://localhost:9999", "user", "secret".ToCharArray());

            Assert.IsNotNull(_client);
        }

        [Test]
        public void CreateInstanceToken()
        {
            _client = InfluxDBClientFactory.Create("http://localhost:9999", "xyz");

            Assert.IsNotNull(_client);
        }

        [Test]
        public void CreateInstanceEmptyToken()
        {
            var empty = Assert.Throws<ArgumentException>(() =>
                InfluxDBClientFactory.Create("http://localhost:9999?", ""));
            Assert.NotNull(empty);
            Assert.AreEqual("Expecting a non-empty string for token", empty.Message);
        }

        [Test]
        public void LoadFromConnectionString()
        {
            _client = InfluxDBClientFactory.Create("http://localhost:9999?" +
                                                   "timeout=1000&logLevel=HEADERS&token=my-token&bucket=my-bucket&org=my-org&allowHttpRedirects=true");

            var options = GetDeclaredField<InfluxDBClientOptions>(_client.GetType(), _client, "_options");
            Assert.AreEqual("http://localhost:9999/", options.Url);
            Assert.AreEqual("my-org", options.Org);
            Assert.AreEqual("my-bucket", options.Bucket);
            Assert.AreEqual("my-token".ToCharArray(), options.Token);
            Assert.AreEqual(true, options.AllowHttpRedirects);
            Assert.AreEqual(LogLevel.Headers, options.LogLevel);
            Assert.AreEqual(LogLevel.Headers, _client.GetLogLevel());

            var apiClient = GetDeclaredField<ApiClient>(_client.GetType(), _client, "_apiClient");
            Assert.AreEqual(1_000, apiClient.RestClientOptions.Timeout);
        }

        [Test]
        public void LoadFromConnectionStringUnitsMillisecondsSeconds()
        {
            _client = InfluxDBClientFactory.Create("http://localhost:9999?" +
                                                   "timeout=1ms&logLevel=Headers&token=my-token&bucket=my-bucket&org=my-org");

            var options = GetDeclaredField<InfluxDBClientOptions>(_client.GetType(), _client, "_options");
            Assert.AreEqual("http://localhost:9999/", options.Url);
            Assert.AreEqual("my-org", options.Org);
            Assert.AreEqual("my-bucket", options.Bucket);
            Assert.AreEqual("my-token".ToCharArray(), options.Token);
            Assert.AreEqual(LogLevel.Headers, options.LogLevel);
            Assert.AreEqual(LogLevel.Headers, _client.GetLogLevel());

            var apiClient = GetDeclaredField<ApiClient>(_client.GetType(), _client, "_apiClient");
            Assert.AreEqual(1, apiClient.RestClientOptions.Timeout);
        }

        [Test]
        public void LoadFromConnectionStringUnitsMinutes()
        {
            _client = InfluxDBClientFactory.Create("http://localhost:9999?" +
                                                   "timeout=1ms&logLevel=Headers&token=my-token&bucket=my-bucket&org=my-org");

            var options = GetDeclaredField<InfluxDBClientOptions>(_client.GetType(), _client, "_options");
            Assert.AreEqual("http://localhost:9999/", options.Url);
            Assert.AreEqual("my-org", options.Org);
            Assert.AreEqual("my-bucket", options.Bucket);
            Assert.AreEqual("my-token".ToCharArray(), options.Token);
            Assert.AreEqual(LogLevel.Headers, options.LogLevel);
            Assert.AreEqual(LogLevel.Headers, _client.GetLogLevel());

            var apiClient = GetDeclaredField<ApiClient>(_client.GetType(), _client, "_apiClient");
            Assert.AreEqual(1, apiClient.RestClientOptions.Timeout);
        }

        [Test]
        public void LoadFromConnectionNotValidDuration()
        {
            var ioe = Assert.Throws<InfluxException>(() => InfluxDBClientFactory.Create("http://localhost:9999?" +
                "timeout=x&logLevel=Headers&token=my-token&bucket=my-bucket&org=my-org"));

            Assert.NotNull(ioe);
            Assert.AreEqual("'x' is not a valid duration", ioe.Message);
        }

        [Test]
        public void LoadFromConnectionUnknownUnit()
        {
            var ioe = Assert.Throws<InfluxException>(() => InfluxDBClientFactory.Create("http://localhost:9999?" +
                "timeout=1y&logLevel=Headers&token=my-token&bucket=my-bucket&org=my-org"));

            Assert.NotNull(ioe);
            Assert.AreEqual("unknown unit for '1y'", ioe.Message);
        }

        [Test]
        public void LoadFromConfiguration()
        {
            CopyAppConfig();

            _client = InfluxDBClientFactory.Create();

            var options = GetDeclaredField<InfluxDBClientOptions>(_client.GetType(), _client, "_options");
            Assert.AreEqual("http://localhost:9999", options.Url);
            Assert.AreEqual("my-org", options.Org);
            Assert.AreEqual("my-bucket", options.Bucket);
            Assert.AreEqual("my-token".ToCharArray(), options.Token);
            Assert.AreEqual(LogLevel.Body, options.LogLevel);
            Assert.AreEqual(LogLevel.Body, _client.GetLogLevel());

            var apiClient = GetDeclaredField<ApiClient>(_client.GetType(), _client, "_apiClient");
            Assert.AreEqual(10_000, apiClient.RestClientOptions.Timeout);

            var defaultTags = GetDeclaredField<SortedDictionary<string, string>>(options.PointSettings.GetType(),
                options.PointSettings, "_defaultTags");

            Assert.AreEqual(4, defaultTags.Count);
            Assert.AreEqual("132-987-655", defaultTags["id"]);
            Assert.AreEqual("California Miner", defaultTags["customer"]);
            Assert.AreEqual("${SensorVersion}", defaultTags["version"]);
        }

        [Test]
        public void LoadFromConfigurationWithoutUrl()
        {
            CopyAppConfig();

            var ce = Assert.Throws<ConfigurationErrorsException>(() => InfluxDBClientOptions.Builder
                .CreateNew()
                .LoadConfig("influx2-without-url"));

            Assert.NotNull(ce);
            StringAssert.StartsWith("Required attribute 'url' not found.", ce.Message);
        }

        [Test]
        public void LoadFromConfigurationNotExist()
        {
            var ce = Assert.Throws<ConfigurationErrorsException>(() => InfluxDBClientOptions.Builder
                .CreateNew()
                .LoadConfig("influx2-not-exits"));

            Assert.NotNull(ce);
            StringAssert.StartsWith("The configuration doesn't contains a 'influx2'", ce.Message);
        }

        [Test]
        public void V1Configuration()
        {
            _client = InfluxDBClientFactory.CreateV1("http://localhost:8086", "my-username",
                "my-password".ToCharArray(), "database", "week");

            var options = GetDeclaredField<InfluxDBClientOptions>(_client.GetType(), _client, "_options");
            Assert.AreEqual("http://localhost:8086", options.Url);
            Assert.AreEqual("-", options.Org);
            Assert.AreEqual("database/week", options.Bucket);
            Assert.AreEqual("my-username:my-password".ToCharArray(), options.Token);

            _client.Dispose();
            _client = InfluxDBClientFactory.CreateV1("http://localhost:8086", null, null, "database", null);

            options = GetDeclaredField<InfluxDBClientOptions>(_client.GetType(), _client, "_options");
            Assert.AreEqual("http://localhost:8086", options.Url);
            Assert.AreEqual("-", options.Org);
            Assert.AreEqual("database/", options.Bucket);
            Assert.AreEqual(":".ToCharArray(), options.Token);
        }

        [Test]
        public void Timeout()
        {
            var options = new InfluxDBClientOptions.Builder()
                .Url("http://localhost:8086")
                .AuthenticateToken("my-token".ToCharArray())
                .TimeOut(TimeSpan.FromSeconds(20))
                .Build();

            _client = InfluxDBClientFactory.Create(options);

            var apiClient = GetDeclaredField<ApiClient>(_client.GetType(), _client, "_apiClient");
            Assert.AreEqual(20_000, apiClient.RestClientOptions.Timeout);
        }

        [Test]
        public void AnonymousSchema()
        {
            var options = new InfluxDBClientOptions.Builder()
                .Url("http://localhost:9999")
                .Build();
            Assert.AreEqual(InfluxDBClientOptions.AuthenticationScheme.Anonymous, options.AuthScheme);
        }

        [Test]
        public void Certificates()
        {
            const string testingPem = @"MIIFZjCCA04CCQCMEn5e+4xmLTANBgkqhkiG9w0BAQsFADB1MQswCQYDVQQGEwJV
UzEQMA4GA1UECAwHTmV3WW9yazEQMA4GA1UEBwwHTmV3WW9yazEdMBsGA1UECgwU
SW5mbHV4REJQeXRob25DbGllbnQxDzANBgNVBAsMBkNsaWVudDESMBAGA1UEAwwJ
bG9jYWxob3N0MB4XDTIwMDcyMDA2MzA0OVoXDTQ3MTIwNjA2MzA0OVowdTELMAkG
A1UEBhMCVVMxEDAOBgNVBAgMB05ld1lvcmsxEDAOBgNVBAcMB05ld1lvcmsxHTAb
BgNVBAoMFEluZmx1eERCUHl0aG9uQ2xpZW50MQ8wDQYDVQQLDAZDbGllbnQxEjAQ
BgNVBAMMCWxvY2FsaG9zdDCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIB
AMoqURng8JwLYe4IHyIAGlI80utBLq6XbDETY93Pr6ZdHePr2jM+UIfFtdkpqdJw
56ZxnJPtM1kDQJTsGfkf0/ePKZpnunNk+lkz9l5uQPVcujydplhJgJeHEj49s3Yy
mWYetcR1Oejnqxgh+9Ev79r1Napu3s80SACPgvTP45CLp1hOGFySRaW7jcG3i+V4
ljQWVAEse9Vy3e7E1EY2p6z/Zvj2UVOMqdHsivR1XLy5hts5ubIqOqvOCPocJ+0/
m0AjwCXO4QPy7pLAAa7DA9rWDpzx8jpdfe54NOHuj4SVP45+PPsWvvkN2ZOkC/vb
zz4DcYVwIqtqej8mvO2kkPIFLdRSKUc5N3xmdvF5awBGfHhb4l/KIDlhRle+L9kF
LxRgkmBb2FFfL0/GtQlpH0bHHwPij4jPcOY+ueLKmAMgwWdqYae0HS01F7nYeZuP
StDG+YuCjglOH8xugcV9GBXrRTijyjuml4st3Wl4tPpQClmdoZ2LXp5h/6Zq1aoc
QlraKjwuTuzQBBHIFh9KXLZANLtMLpGGepFSMqE6YIWl17gi/2NruP8MGXNk+7GM
ylczKu/Ny67qQ8JCnRLSNUXPg18LjU2voLuzgXWtuTUgRnQBdir6ZB5Bwc2zi0vx
DNl0yzDhGNFdR5Rng5lAcmclA4QWi7Oc4h/OLN0ma0UzAgMBAAEwDQYJKoZIhvcN
AQELBQADggIBADsWOWIMvynE2Iwtpv471kROYdqrj6Zj5P8Ye5/0lqedRxIYWDsc
XDii+ePem+cMhnty8tAqCeHIdBUN86ibP+oqlwySbvdvW121RfedsWpa+TPC+Rnj
8n5w0urVNpnYuep2f8SOpQ1WdXFMLIsKqcnV5KK3/rxOAUY9cNVumA55/terQMOZ
RSGfjtoKVkMSOxNlaj4frLNy+I7nyWYrZ9UmUirvGLce5LJ1nrmo2I46FA0XDwu8
xJqe4mB3GT/t9kFujd3Q/MtgD4J/MjWBfSYV0vlzI+VuoRctikw2SWQckQWNlIhs
LPafo6D+lOxJtH58WksCxdb8C8sBbRl+irv/ZAlvIiOkmcpHcOQ7AbLTtosZs6nX
p0ptWENlTM3lkt/Xma8txWXfe29tlf/9oheqXKdYunRyvFPL/gBjjR/VWzIS5sT5
T6z0Vdk7uW9/wzv45vzjES8a8AAFvEkaRS4JBoTCW69mc8RFR89Vp9axRHY/3ohQ
8pS9K00FLMTObb8qlW31LfKpCUSxHmU00BhGPduMYQF28Xj02zQ5UaeGOnSO5EjU
pG0N7yqaVwGv9jYQfmnnD7M5LYVweEZ3OzCbfZuNJ4+EHNdZKcJiu2TaOsyxK25q
AJvDAFTSr5A9GSjJ3OyIeKoI8Q6xuaQBitpZR90P/Ah/Ymg490rpXavk";

            var certificateCollection = new X509CertificateCollection
                { new X509Certificate2(Convert.FromBase64String(testingPem)) };

            var options = new InfluxDBClientOptions.Builder()
                .Url("http://localhost:8086")
                .AuthenticateToken("my-token".ToCharArray())
                .ClientCertificates(certificateCollection)
                .Build();

            _client = InfluxDBClientFactory.Create(options);

            var apiClient = GetDeclaredField<ApiClient>(_client.GetType(), _client, "_apiClient");
            Assert.AreEqual(certificateCollection, apiClient.RestClientOptions.ClientCertificates);
        }

        private static T GetDeclaredField<T>(IReflect type, object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                           | BindingFlags.Static | BindingFlags.DeclaredOnly;
            var field = type.GetField(fieldName, bindFlags);
            return (T)field?.GetValue(instance);
        }

        private static void CopyAppConfig()
        {
            // copy App.config to assemble format
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            File.Copy(Directory.GetCurrentDirectory() + "/../../../App.config", config.FilePath, true);
        }
    }
}