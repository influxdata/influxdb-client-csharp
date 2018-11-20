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
            /*Organization medicalGmbh = await platform.CreateOrganizationClient()
                            .CreateOrganization("Medical Corp" + 
                                                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                                CultureInfo.InvariantCulture));*/

            List<Organization> organizations = await platform.CreateOrganizationClient().FindOrganizations();

            organizations.ForEach(o => Console.Write(o.ToString()));
            
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