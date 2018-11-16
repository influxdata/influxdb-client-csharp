using System.Collections.Generic;

namespace InfluxData.Platform.Client.Domain
{
    public class Users : AbstractHasLinks
    {
        public List<User> UserList { get; set; } = new List<User>();
    }
}