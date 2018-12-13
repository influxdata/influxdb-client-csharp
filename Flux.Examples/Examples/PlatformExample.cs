using System;
using System.Globalization;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Client;
using NodaTime;

namespace Flux.Examples.Examples
{
    public static class PlatformExample
    {
        private class Temperature 
        {
            public string Location { get; set; }

            public double Value { get; set; }

            public Instant Time { get; set; }
        }

        public static async Task Example(PlatformClient platform)
        {
            var organizationClient = platform.CreateOrganizationClient();
            
            var medicalGmbh = await organizationClient
                            .CreateOrganization("Medical Corp" + 
                                                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                                CultureInfo.InvariantCulture));

            var org = await organizationClient.FindOrganizationById(medicalGmbh.Id);

            Console.WriteLine(org.ToString());
            Console.WriteLine("------");
            
            foreach (var organization in await organizationClient.FindOrganizations())
            {
                Console.WriteLine();
                Console.WriteLine(organization.ToString());
            }
            
            Console.WriteLine("------");
            
            await organizationClient.DeleteOrganization(org);

            foreach (var organization in await organizationClient.FindOrganizations())
            {
                Console.WriteLine();
                Console.WriteLine(organization.ToString());
            }
            
            platform.Dispose();
        }
        
        public static void Run()
        {
            var platform = PlatformClientFactory.Create("http://localhost:9999",
                            "my-user", "my-password".ToCharArray());

            Example(platform).Wait();
        }
    }
}