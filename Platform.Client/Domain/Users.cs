using System.Collections.Generic;

namespace Platform.Client.Domain
{
    public class Users : AbstractHasLinks
    {
        public List<User> UserList { get; set; } = new List<User>();
    }
}