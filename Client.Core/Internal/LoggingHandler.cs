using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace InfluxDB.Client.Core.Internal
{
    public class LoggingHandler
    {
        public LogLevel Level { get; set; }

        public LoggingHandler(LogLevel logLevel)
        {
            Level = logLevel;
        }

        public void BeforeIntercept(HttpRequestMessage req)
        {
            if (Level == LogLevel.None)
            {
                return;
            }

            var isBody = Level == LogLevel.Body;
            var isHeader = isBody || Level == LogLevel.Headers;

            var uri = req.RequestUri;
            var uriToLog = string.IsNullOrEmpty(uri.Query) ? uri.AbsoluteUri : uri.AbsoluteUri.Replace(uri.Query, "");
            Trace.WriteLine($"--> {req.Method} {uriToLog}");

            if (isHeader)
            {
                var queryString = HttpUtility.ParseQueryString(uri.Query);
                var query = queryString
                    .AllKeys
                    .Select(key => (key, Enumerable.Repeat(queryString.Get(key), 1)));
                LogHeaders(query, "-->", "Query");

                var headers = req.Headers.Select(ToKeyValue());
                LogHeaders(headers, "-->");
            }

            if (isBody)
            {
                var body = req.Content;
                if (body != null)
                {
                    if (body is StringContent stringContent)
                    {
                        var result = stringContent.ReadAsStringAsync().Result;
                        Trace.WriteLine($"--> Body: {result}");
                    }
                    else
                    {
                        Trace.WriteLine($"--> Body: {body}");
                    }
                }
            }

            Trace.WriteLine("--> END");
            Trace.WriteLine("-->");
        }

        public void AfterIntercept(int statusCode, Func<HttpResponseHeaders> headers, object body)
        {
            if (Level == LogLevel.None)
            {
                return;
            }

            var isBody = Level == LogLevel.Body;
            var isHeader = isBody || Level == LogLevel.Headers;

            Trace.WriteLine($"<-- {statusCode}");

            if (isHeader)
            {
                LogHeaders(headers.Invoke().Select(ToKeyValue()), "<--");
            }

            if (isBody && body != null)
            {
                string stringBody;

                if (body is Stream stream)
                {
                    var sr = new StreamReader(stream);
                    stringBody = sr.ReadToEnd();
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
        }

        public static IDictionary<string, IList<string>> ToHeaders(HttpResponseHeaders parameters)
        {
            return parameters.ToDictionary(it => it.Key, a => a.Value.ToList() as IList<string>);
        }

        private void LogHeaders(IEnumerable<(string key, IEnumerable<string>)> headers, string direction,
            string type = "Header")
        {
            foreach (var keyValue in headers)
            {
                Trace.WriteLine($"{direction} {type}: {keyValue.Item1}={string.Join(", ", keyValue.Item2)}");
            }
        }

        private static Func<KeyValuePair<string, IEnumerable<string>>, (string Key, IEnumerable<string>)> ToKeyValue()
        {
            return it => (it.Key, it.Value);
        }
    }
}