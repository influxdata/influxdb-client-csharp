namespace Platform.Client.Domain
{
    public class UserResourceMapping
    {
        private string ResourceId { get; set; }

        public ResourceType ResourceType { get; set; }

        public string UserId { get; set; }

        public EUserType UserType { get; set; }

        /**
         * The user type.
         */
        public enum EUserType 
        {
            Owner,

            Member
        }
    }
}