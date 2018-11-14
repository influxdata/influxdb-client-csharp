namespace Platform.Client.Domain
{
/**
 * The status of the {@link Run}.
 */
    public enum RunStatus 
    {
        Scheduled,

        Executing,

        Failed,

        Success
    }
}