using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using InfluxDB.Client.Api.Client;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class InfluxDbClientFactoryTest
    {
        [SetUp]
        public void SetUp()
        {
        }
        
        [Test]
        public void CreateInstance() 
        {
            var client = InfluxDBClientFactory.Create("http://localhost:9999");

            Assert.IsNotNull(client);
        }

        [Test]
        public void CreateInstanceUsername() {

            var client = InfluxDBClientFactory.Create("http://localhost:9999", "user", "secret".ToCharArray());

            Assert.IsNotNull(client);
        }

        [Test]
        public void CreateInstanceToken() {

            var client = InfluxDBClientFactory.Create("http://localhost:9999", "xyz");

            Assert.IsNotNull(client);
        }

        [Test]
        public void LoadFromConnectionString()
        {
            var client = InfluxDBClientFactory.Create("http://localhost:9999?" +
                                                      "timeout=1000&readWriteTimeout=3000&logLevel=HEADERS&token=my-token&bucket=my-bucket&org=my-org");

            var options = GetDeclaredField<InfluxDBClientOptions>(client.GetType(), client, "_options");
            Assert.AreEqual("http://localhost:9999/", options.Url);
            Assert.AreEqual("my-org", options.Org);
            Assert.AreEqual("my-bucket", options.Bucket);
            Assert.AreEqual("my-token".ToCharArray(), options.Token);
            Assert.AreEqual(LogLevel.Headers, options.LogLevel);
            Assert.AreEqual(LogLevel.Headers, client.GetLogLevel());

            var apiClient = GetDeclaredField<ApiClient>(client.GetType(), client, "_apiClient");
            Assert.AreEqual(1_000, apiClient.Configuration.Timeout);
            Assert.AreEqual(3_000, apiClient.Configuration.ReadWriteTimeout);
        }

        [Test]
        public void LoadFromConnectionStringUnitsMillisecondsSeconds()
        {
            var client = InfluxDBClientFactory.Create("http://localhost:9999?" +
                                                      "timeout=1ms&readWriteTimeout=3s&logLevel=Headers&token=my-token&bucket=my-bucket&org=my-org");
            
            var options = GetDeclaredField<InfluxDBClientOptions>(client.GetType(), client, "_options");
            Assert.AreEqual("http://localhost:9999/", options.Url);
            Assert.AreEqual("my-org", options.Org);
            Assert.AreEqual("my-bucket", options.Bucket);
            Assert.AreEqual("my-token".ToCharArray(), options.Token);
            Assert.AreEqual(LogLevel.Headers, options.LogLevel);
            Assert.AreEqual(LogLevel.Headers, client.GetLogLevel());

            var apiClient = GetDeclaredField<ApiClient>(client.GetType(), client, "_apiClient");
            Assert.AreEqual(1, apiClient.Configuration.Timeout);
            Assert.AreEqual(3_000, apiClient.Configuration.ReadWriteTimeout);
        }
        
        [Test]
        public void LoadFromConnectionStringUnitsMinutes()
        {
            var client = InfluxDBClientFactory.Create("http://localhost:9999?" +
                                                      "timeout=1ms&readWriteTimeout=3m&logLevel=Headers&token=my-token&bucket=my-bucket&org=my-org");
            
            var options = GetDeclaredField<InfluxDBClientOptions>(client.GetType(), client, "_options");
            Assert.AreEqual("http://localhost:9999/", options.Url);
            Assert.AreEqual("my-org", options.Org);
            Assert.AreEqual("my-bucket", options.Bucket);
            Assert.AreEqual("my-token".ToCharArray(), options.Token);
            Assert.AreEqual(LogLevel.Headers, options.LogLevel);
            Assert.AreEqual(LogLevel.Headers, client.GetLogLevel());

            var apiClient = GetDeclaredField<ApiClient>(client.GetType(), client, "_apiClient");
            Assert.AreEqual(1, apiClient.Configuration.Timeout);
            Assert.AreEqual(180_000, apiClient.Configuration.ReadWriteTimeout);
        }

        [Test]
        public void LoadFromConnectionNotValidDuration()
        {
            var ioe = Assert.Throws<InfluxException>(() => InfluxDBClientFactory.Create("http://localhost:9999?" +
                                                                                        "timeout=x&readWriteTimeout=3m&logLevel=Headers&token=my-token&bucket=my-bucket&org=my-org"));

            Assert.AreEqual("'x' is not a valid duration", ioe.Message);
        }

        [Test]
        public void LoadFromConnectionUnknownUnit()
        {
            var ioe = Assert.Throws<InfluxException>(() => InfluxDBClientFactory.Create("http://localhost:9999?" +
                                                                                        "timeout=1y&readWriteTimeout=3m&logLevel=Headers&token=my-token&bucket=my-bucket&org=my-org"));

            Assert.AreEqual("unknown unit for '1y'", ioe.Message);
        }

        [Test]
        public void LoadFromConfiguration()
        {
            // copy App.config to assemble format
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            File.Copy(Directory.GetCurrentDirectory() + "/../../../App.config", config.FilePath, true);

            var client = InfluxDBClientFactory.Create();
            
            var options = GetDeclaredField<InfluxDBClientOptions>(client.GetType(), client, "_options");
            Assert.AreEqual("http://localhost:9999", options.Url);
            Assert.AreEqual("my-org", options.Org);
            Assert.AreEqual("my-bucket", options.Bucket);
            Assert.AreEqual("my-token".ToCharArray(), options.Token);
            Assert.AreEqual(LogLevel.Body, options.LogLevel);
            Assert.AreEqual(LogLevel.Body, client.GetLogLevel());

            var apiClient = GetDeclaredField<ApiClient>(client.GetType(), client, "_apiClient");
            Assert.AreEqual(10_000, apiClient.Configuration.Timeout);
            Assert.AreEqual(5_000, apiClient.Configuration.ReadWriteTimeout);

            var defaultTags = GetDeclaredField<SortedDictionary<string, string>>(options.PointSettings.GetType(), options.PointSettings, "_defaultTags");
            
            Assert.AreEqual(4, defaultTags.Count);
            Assert.AreEqual("132-987-655", defaultTags["id"]);
            Assert.AreEqual("California Miner", defaultTags["customer"]);
            Assert.AreEqual("${SensorVersion}", defaultTags["version"]);
        }
        
        [Test]
        public void V1Configuration()
        {
            var client = InfluxDBClientFactory.CreateV1("http://localhost:8086", "my-username", "my-password".ToCharArray(), "database", "week");

            var options = GetDeclaredField<InfluxDBClientOptions>(client.GetType(), client, "_options");
            Assert.AreEqual("http://localhost:8086", options.Url);
            Assert.AreEqual("-", options.Org);
            Assert.AreEqual("database/week", options.Bucket);
            Assert.AreEqual("my-username:my-password".ToCharArray(), options.Token);
            
            client = InfluxDBClientFactory.CreateV1("http://localhost:8086", null, null, "database", null);

            options = GetDeclaredField<InfluxDBClientOptions>(client.GetType(), client, "_options");
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
                .ReadWriteTimeOut(TimeSpan.FromSeconds(30))
                .Build();

            var client = InfluxDBClientFactory.Create(options);
            
            var apiClient = GetDeclaredField<ApiClient>(client.GetType(), client, "_apiClient");
            Assert.AreEqual(20_000, apiClient.Configuration.Timeout);
            Assert.AreEqual(30_000, apiClient.Configuration.ReadWriteTimeout);
        }
        
        private static T GetDeclaredField<T>(IReflect type, object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                           | BindingFlags.Static | BindingFlags.DeclaredOnly;
            var field = type.GetField(fieldName, bindFlags);
            return (T) field.GetValue(instance);
        }
    }
}