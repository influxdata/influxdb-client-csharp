using System;
using System.Globalization;
using InfluxData.Platform.Client;
using InfluxData.Platform.Client.Impl;
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
        
        public static void Run()
        {
            PlatformClient platform = PlatformClientFactory.Create("http://localhost:9999",
                            "my-user", "my-password".ToCharArray());
            
            /*Organization medicalGmbh = platform.CreateOrganizationClient()
                            .createOrganization("Medical Corp" + 
                                                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                                CultureInfo.InvariantCulture));*/
        }
    }
}