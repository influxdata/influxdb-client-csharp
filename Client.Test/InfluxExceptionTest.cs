using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using InfluxDB.Client.Core.Exceptions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class InfluxExceptionTest
    {
        [Test]
        public void ExceptionHeadersTest()
        {
            try
            {
                throw HttpException.Create(
                    JObject.Parse("{\"callId\": \"123456789\", \"message\": \"error in content object\"}"),
                    new List<HeaderParameter>
                    {
                        new HeaderParameter("Trace-Id", "123456789ABCDEF0"),
                        new HeaderParameter("X-Influx-Version", "1.0.0"),
                        new HeaderParameter("X-Platform-Error-Code", "unavailable"),
                        new HeaderParameter("Retry-After", "60000"),
                    },
                    null,
                    HttpStatusCode.ServiceUnavailable);
            }
            catch (HttpException he)
            {
                // Assert.AreEqual("error in content object", he?.Message);
                Console.WriteLine("DEBUG he.Message {0}", he.Message);
            
            
                Assert.AreEqual(4, he?.Headers.Count());
                Dictionary<String, String> headers = new Dictionary<String, String>();
                foreach (HeaderParameter header in he?.Headers)
                {
                    headers.Add(header.Name, header.Value);
                }
                Assert.AreEqual("123456789ABCDEF0", headers["Trace-Id"]);
                Assert.AreEqual("1.0.0", headers["X-Influx-Version"]);
                Assert.AreEqual("unavailable", headers["X-Platform-Error-Code"]);
                Assert.AreEqual("60000", headers["Retry-After"]);
            }
        }
    }
}