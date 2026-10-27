using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using InfluxDB.Client.Test.utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InfluxDB.Client.Test
{
    public class TestWebFactory : IDisposable
    {
        private readonly IHost _host;
        private readonly int _port;

        private static readonly string ServerCertificate = TestUtils.ResolveCertificatePath("influxdb.p12");
        private static readonly string ClientCertificate = TestUtils.ResolveCertificatePath("client.p12");
        private const string Password = "changeit";

        public TestWebFactory()
        {
            _host = WebHostBuilder().Build();
            _host.Start();
            var addressInfo = _host.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
            if (addressInfo == null)
            {
                throw new Exception("No address info could be found for configured server");
            }

            var address = addressInfo.Addresses.First();
            var addressUri = new Uri(address);
            _port = addressUri.Port;
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        private static IHostBuilder WebHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(c =>
                        {
                            c.ListenAnyIP(443, l =>
                            {
                                l.Protocols = HttpProtocols.Http1AndHttp2;
                                l.UseHttps(httpsOptions =>
                                {
                                    httpsOptions.ServerCertificate = new X509Certificate2(ServerCertificate, Password);
                                    httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                                    httpsOptions.ClientCertificateValidation = (certificate, chain, errors) =>
                                    {
                                        if (certificate == null)
                                        {
                                            return false;
                                        }

                                        var expectedCert = new X509Certificate2(ClientCertificate, Password);
                                        var receivedCert = new X509Certificate2(certificate);
                                        return receivedCert.Thumbprint == expectedCert.Thumbprint;
                                    };
                                });
                            });
                        })
                        .UseStartup<Startup>();
                });
        }

        public string GetAddress()
        {
            return $"https://127.0.0.1:{_port}";
        }

        private void Stop()
        {
            _host.StopAsync().Wait();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}