using InfluxDB.Client.Core.Exceptions;

namespace InfluxDB.Client.Core.Flux.Exceptions
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