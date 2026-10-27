using System;
using System.Security.Cryptography.X509Certificates;
using InfluxDB.Client;
using NodaTime;

namespace Examples
{
    public static class TlsExample
    {
        private const string
            ServerCertificatePath =
                "path/to/influxdb.p12"; // Change this to your real server .p12 file with certificate and key bundled inside.

        private const string
            ClientCertificatePath =
                "path/to/client.p12"; // Change this to your real client .p12 file with certificate and key bundled inside.

        private const string Password = "changeit";

        public static void Main()
        {
            // Change Host, Token, Org, Bucket,... to you real values.
            var option = new InfluxDBClientOptions("https://127.0.0.1:8443")
            {
                Token = "MyInitialAdminToken0==",
                Org = "my-org",
                Bucket = "my-data",
                VerifySslCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    // Validate the server certificate send from the server.
                    var expectedCert = new X509Certificate2(ServerCertificatePath, Password);
                    if (certificate == null)
                    {
                        return false;
                    }

                    var receivedCert = new X509Certificate2(certificate);
                    return receivedCert.Thumbprint == expectedCert.Thumbprint;
                }
                // Optional. Config `ClientCertificates` send to the server when server requires client certificate in mTLS.
                // ClientCertificates = new X509CertificateCollection
                // {
                //     new X509Certificate2(ClientCertificatePath, Password)
                // }
            };
            using var client = new InfluxDBClient(option);

            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            var cpu = new Cpu()
            {
                Time = now,
                Usage = 56.7,
                Host = "darkstar"
            };

            Console.WriteLine("Write a record to the database.");


            // Write a record to the database.
            // var writeApi = client.GetWriteApi();
            // writeApi.WriteRecord($"cpu,host={cpu.Host} usage={cpu.Usage} {cpu.Time.ToUnixTimeMilliseconds()}",
            //     WritePrecision.Ms);
            // writeApi.Flush();
        }
    }
}