using System.Collections.Generic;

namespace InfluxDB.Client.Domain
{
    public class ResourceMembers
    {
        public List<ResourceMember> Users { get; set; } = new List<ResourceMember>();
    }
}