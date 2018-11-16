using System.Collections.Generic;

namespace InfluxData.Platform.Client.Domain
{
    public class UserResourcesResponse
    {
        public List<UserResourceMapping> UserResourceMappings { get; set; } = new List<UserResourceMapping>();
    }
}