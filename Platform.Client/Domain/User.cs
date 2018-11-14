using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;

namespace Platform.Client.Domain
{
    /**
     * User
     */
    public class User : AbstractHasLinks
    {
        public string Id { get; set; }
        
        public string Name { get; set; }

        public override string ToString() 
        {
            return new StringBuilder(GetType().Name + "[")
                            .Append("id='" + Id + "'")
                            .Append(", name='" + Name + "'")
                            .Append("]").ToString();
        }
    }
}