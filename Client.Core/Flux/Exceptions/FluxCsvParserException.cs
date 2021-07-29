using System;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Flux.Domain;

namespace InfluxDB.Client.Core.Flux.Exceptions
{
    /// <summary>
    /// The error that occurs during mapping response to <see cref="FluxTable"/>, <see cref="FluxRecord"/> or <see cref="FluxColumn"/>. 
    /// </summary>
    public class FluxCsvParserException : InfluxException
    {
        public FluxCsvParserException(string message) : base(message)
        {
        }

        public FluxCsvParserException(string message, Exception exception = null) : base(message, exception)
        {
        }
    }
}