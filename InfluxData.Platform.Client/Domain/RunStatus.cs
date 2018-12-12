namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// The status of the <see cref="Run"/>.
    /// </summary>
    public enum RunStatus
    {
        Scheduled,

        Executing,

        Failed,

        Success
    }
}