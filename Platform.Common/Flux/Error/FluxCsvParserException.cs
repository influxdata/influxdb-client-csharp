using System.Collections.Generic;

namespace Platform.Common.Flux.Error
{
    /**
     * The error that occurs during mapping response to {@link FluxTable}, {@link FluxRecord} or {@link FluxColumn}.
     */
    public class FluxCsvParserException : InfluxException
    {
        public FluxCsvParserException(string message) : base(new QueryErrorResponse(0,
                        new List<string>() {message}.AsReadOnly()))
        {

        }
    }
}