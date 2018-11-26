using System.Collections.Generic;

namespace InfluxData.Platform.Client.Domain
{
    public class UsersResponse : AbstractHasLinks
    {
        public List<User> Users { get; set; } = new List<User>();
    }
}