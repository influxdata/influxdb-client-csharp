using Platform.Common.Flux.Domain;

namespace Platform.Common.Flux.Error
{
    /// <summary>
    /// The error that occurs during mapping response to <see cref="FluxTable"/>, <see cref="FluxRecord"/> or <see cref="FluxColumn"/>. 
    /// </summary>
    public class FluxCsvParserException : InfluxException
    {
        public FluxCsvParserException(string message) : base(message)
        {
        }
    }
}