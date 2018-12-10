namespace Platform.Common.Flux.Error
{
    public class FluxQueryException : InfluxException
    {
        public int Reference { get; }

        public FluxQueryException(string message, int reference) : base(message, reference)
        {
            Reference = reference;
        }
    }
}