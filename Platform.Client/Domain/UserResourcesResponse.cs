using System.Collections.Generic;

namespace Platform.Client.Domain
{
    public class UserResourcesResponse
    {
        public List<UserResourceMapping> UserResourceMappings { get; set; } = new List<UserResourceMapping>();
    }
}