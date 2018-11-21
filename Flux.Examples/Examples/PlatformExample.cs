using System;
using System.Collections.Generic;
using System.Globalization;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NodaTime;
using Task = System.Threading.Tasks.Task;

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
            OrganizationClient organizationClient = platform.CreateOrganizationClient();
            
            Organization medicalGmbh = await organizationClient
                            .CreateOrganization("Medical Corp" + 
                                                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                                CultureInfo.InvariantCulture));

            Organization org = await organizationClient.FindOrganizationById(medicalGmbh.Id);

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
            
            await platform.Close();
        }
        
        public static void Run()
        {
            PlatformClient platform = PlatformClientFactory.Create("http://localhost:9999",
                            "my-user", "my-password".ToCharArray());

            Example(platform).Wait();
        }
    }
}