using System.Collections.Generic;
using System.Text;

namespace Platform.Client.Domain
{
    /**
     * Authorization
     */
    public class Authorization : AbstractHasLinks
    {
        public string Id { get; set; }
        
        public string Token { get; set; }
        
        public string UserId { get; set; }
        
        public string UserName { get; set; }

        public Status Status { get; set; }
        
        public List<Permission> Permissions { get; set; }

        public override string ToString() 
        {
            return new StringBuilder(GetType().Name + "[")
                            .Append("id='" + Id + "'")
                            .Append(", token='" + Token + "'")
                            .Append(", userID='" + UserId + "'")
                            .Append(", userName='" + UserName + "'")
                            .Append(", status=" + Status)
                            .Append(", permissions=" + Permissions)
                            .Append("]").ToString();
        }
    }
}