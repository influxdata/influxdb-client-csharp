using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Platform.Common.Platform.Rest
{
    public class LoggingHandler: DelegatingHandler
    {
        public LogLevel Level { get; set; }

        public LoggingHandler(LogLevel logLevel)
        {
            Level = logLevel;
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (Level == LogLevel.None)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var isBody = Level == LogLevel.Body;
            var isHeader = isBody || Level == LogLevel.Headers;

            Trace.WriteLine($"--> {request.Method} {request.RequestUri}");

            if (isHeader)
            {
                foreach (var emp in request.Headers)
                {
                    Trace.WriteLine($"--> Header: {emp.Key} Value: {emp.Value.First()}");
                }
            }

            if (isBody)
            {
                Trace.WriteLine($"--> Body: {request.Content?.ReadAsStringAsync().Result}");
            }

            Trace.WriteLine("--> END");
            Trace.WriteLine("-->");

            var response = await base.SendAsync(request, cancellationToken);

            Trace.WriteLine($"<-- {response.StatusCode}");

            if (isHeader)
            {
                foreach (var emp in response.Headers)
                {
                    Trace.WriteLine($"<-- Header: {emp.Key}: {emp.Value.First()}");
                }
            }

            if (isBody)
            {
                Trace.WriteLine($"<-- Body: {response.Content?.ReadAsStringAsync().Result}");
            }
                
            Trace.WriteLine("<-- END");

            return response;
        }
    }
}