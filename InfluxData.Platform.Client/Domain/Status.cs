namespace InfluxData.Platform.Client.Domain
{
    /**
     * Status defines if a resource is active or inactive.
     */
    public enum Status
    {
        /**
         * Active status means that the resource can be used.
         */
        Active,

        /**
         * Inactive status means that the resource cannot be used.
         */
        Inactive
    }
}