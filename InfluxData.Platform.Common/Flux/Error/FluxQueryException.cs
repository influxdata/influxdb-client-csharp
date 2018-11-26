namespace Platform.Common.Flux.Error
{
    public class FluxQueryException : InfluxException
    {
        public int Reference { get; }

        public FluxQueryException(string message, int reference) : base(new QueryErrorResponse(0, message))
        {
            Reference = reference;
        }
    }
}