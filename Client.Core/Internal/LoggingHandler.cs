using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using RestSharp;

namespace InfluxDB.Client.Core.Internal
{
    public class LoggingHandler
    {
        public LogLevel Level { get; set; }

        public LoggingHandler(LogLevel logLevel)
        {
            Level = logLevel;
        }

        public void BeforeIntercept(IRestRequest request)
        {
            if (Level == LogLevel.None)
            {
                return;
            }

            var isBody = Level == LogLevel.Body;
            var isHeader = isBody || Level == LogLevel.Headers;

            Trace.WriteLine($"--> {request.Method} {request.Resource}");

            if (isHeader)
            {
                var query = ToHeaders(request.Parameters, ParameterType.QueryString);
                LogHeaders(query, "-->", "Query");
                
                var headers = ToHeaders(request.Parameters);
                LogHeaders(headers, "-->");
            }

            if (isBody)
            {
                var body = request.Parameters.FirstOrDefault(parameter =>
                    parameter.Type.Equals(ParameterType.RequestBody));

                if (body != null)
                {
                    string stringBody;
                    
                    if (body.Value is byte[] bytes)
                    {
                        stringBody = Encoding.UTF8.GetString(bytes);
                    }
                    else
                    {
                        stringBody = body.Value.ToString();
                    }
                    
                    Trace.WriteLine($"--> Body: {stringBody}");
                }
            }

            Trace.WriteLine("--> END");
            Trace.WriteLine("-->");
        }

        public object AfterIntercept(int statusCode, Func<IList<HttpHeader>> headers, object body)
        {
            var freshBody = body;
            if (Level == LogLevel.None)
            {
                return freshBody;
            }

            var isBody = Level == LogLevel.Body;
            var isHeader = isBody || Level == LogLevel.Headers;

            Trace.WriteLine($"<-- {statusCode}");

            if (isHeader)
            {
                LogHeaders(headers.Invoke(), "<--");
            }

            if (isBody && body != null)
            {
                string stringBody;

                if (body is Stream)
                {
                    var stream = body as Stream;
                    var sr = new StreamReader(stream);
                    stringBody = sr.ReadToEnd();

                    freshBody = new MemoryStream(Encoding.UTF8.GetBytes(stringBody));
                }
                else
                {
                    stringBody = body.ToString();
                }

                if (!string.IsNullOrEmpty(stringBody))
                {
                    Trace.WriteLine($"<-- Body: {stringBody}");
                }
            }

            Trace.WriteLine("<-- END");

            return freshBody;
        }

        public static List<HttpHeader> ToHeaders(IList<Parameter> parameters, ParameterType type = ParameterType.HttpHeader)
        {
            return parameters
                .Where(parameter => parameter.Type.Equals(type))
                .Select(h => new HttpHeader {Name = h.Name, Value = h.Value.ToString()})
                .ToList();
        }

        private void LogHeaders(IList<HttpHeader> headers, string direction, string type = "Header")
        {
            foreach (var emp in headers)
            {
                Trace.WriteLine($"{direction} {type}: {emp.Name}={emp.Value}");
            }
        }
    }
}