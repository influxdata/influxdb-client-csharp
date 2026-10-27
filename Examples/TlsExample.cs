using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using InfluxDB.Client;

namespace Examples
{
    public static class TlsExample
    {
        private const string ServerCertificatePath = "path/to/influxdb.p12";
        private const string ClientCertificatePath = "path/to/client.p12";
        private const string Password = "changeit";

        public static async Task Main()
        {
            var option = new InfluxDBClientOptions("https://127.0.0.1:433")
            {
                VerifySslCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    var expectedCert = new X509Certificate2(ServerCertificatePath, Password);
                    if (certificate == null)
                    {
                        return false;
                    }

                    var receivedCert = new X509Certificate2(certificate);
                    return receivedCert.Thumbprint == expectedCert.Thumbprint;
                },
                // Config `ClientCertificates` only needed when server requires client certificate in mTLS.
                ClientCertificates = new X509CertificateCollection
                {
                    new X509Certificate2(ClientCertificatePath, Password)
                }
            };
            using var client = new InfluxDBClient(option);
            await client.GetQueryApi()
                .QueryAsync("import \"experimental\"; experimental.sleep(duration: 1s)", "my-org");
        }
    }
}