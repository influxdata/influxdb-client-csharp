using System.Collections.Generic;

namespace InfluxData.Platform.Client.Domain
{
    public class ResourceMembers
    {
        public List<ResourceMember> Users { get; set; } = new List<ResourceMember>();
    }
}