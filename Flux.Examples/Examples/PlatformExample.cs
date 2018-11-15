using NodaTime;
using Platform.Client;
using Platform.Client.Impl;

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
        }
    }
}