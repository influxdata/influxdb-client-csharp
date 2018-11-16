using System.Text;

namespace InfluxData.Platform.Client.Domain
{
    /**
     * Organization
     */
    public class Organization : AbstractHasLinks
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return new StringBuilder(GetType().Name + "[")
                            .Append("id='" + Id + "'")
                            .Append(", name='" + Name + "'")
                            .Append(", links=" + Links)
                            .Append("]").ToString();
        }
    }
}