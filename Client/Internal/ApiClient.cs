using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using InfluxDB.Client.Core.Internal;
using RestSharp;

namespace InfluxDB.Client.Api.Client
{
    public partial class ApiClient
    {
        private readonly List<string> _noAuthRoute = new List<string>
            {"/api/v2/signin", "/api/v2/signout", "/api/v2/setup"};

        private readonly InfluxDBClientOptions _options;
        private readonly LoggingHandler _loggingHandler;
        private readonly GzipHandler _gzipHandler;

        private char[] _sessionToken;
        private bool _signout;

        public ApiClient(InfluxDBClientOptions options, LoggingHandler loggingHandler, GzipHandler gzipHandler)
        {
            _options = options;
            _loggingHandler = loggingHandler;
            _gzipHandler = gzipHandler;

            var timeoutTotalMilliseconds = (int) options.Timeout.TotalMilliseconds;
            var totalMilliseconds = (int) options.ReadWriteTimeout.TotalMilliseconds;

            RestClient = new RestClient(options.Url);
            RestClient.AutomaticDecompression = false;
            Configuration = new Configuration
            {
                ApiClient = this,
                BasePath = options.Url,
                Timeout = timeoutTotalMilliseconds,
                ReadWriteTimeout = totalMilliseconds,
            };
        }

        partial void InterceptRequest(IRestRequest request)
        {
            BeforeIntercept(request);
        }

        partial void InterceptResponse(IRestRequest request, IRestResponse response)
        {
            AfterIntercept((int) response.StatusCode, () => LoggingHandler.ToHeaders(response.Headers), response.Content);
        }
        
        internal void BeforeIntercept(IRestRequest request)
        {
            if (_signout || _noAuthRoute.Any(requestPath => requestPath.EndsWith(request.Resource))) return;

            if (InfluxDBClientOptions.AuthenticationScheme.Token.Equals(_options.AuthScheme))
            {
                request.AddHeader("Authorization", "Token " + new string(_options.Token));
            }
            else if (InfluxDBClientOptions.AuthenticationScheme.Session.Equals(_options.AuthScheme))
            {
                InitToken();

                if (_sessionToken != null) request.AddHeader("Cookie", "session=" + new string(_sessionToken));
            }
            
            _loggingHandler.BeforeIntercept(request);
            _gzipHandler.BeforeIntercept(request);
        }

        internal T AfterIntercept<T>(int statusCode, Func<IList<HttpHeader>> headers, T body)
        {
            var uncompressed = _gzipHandler.AfterIntercept(statusCode, headers, body);
            return (T) _loggingHandler.AfterIntercept(statusCode, headers, uncompressed);
        }

        private void InitToken()
        {
            if (!InfluxDBClientOptions.AuthenticationScheme.Session.Equals(_options.AuthScheme) || _signout) return;

            if (_sessionToken == null)
            {
                IRestResponse authResponse;
                try
                {
                    var header = "Basic " + Convert.ToBase64String(
                                     Encoding.Default.GetBytes(
                                         _options.Username + ":" + new string(_options.Password)));

                    var request = new RestRequest("/api/v2/signin", Method.POST)
                        .AddHeader("Authorization", header);

                    authResponse = RestClient.Execute(request);
                }
                catch (IOException e)
                {
                    Trace.WriteLine("Cannot retrieve the Session token!");
                    Trace.WriteLine(e);
                    return;
                }

                if (authResponse.Cookies != null)
                {
                    var cookies = authResponse.Cookies.ToList();

                    if (cookies.Count == 1)
                        _sessionToken = cookies
                            .First(cookie => cookie.Name.ToString().Equals("session"))
                            .Value
                            .ToCharArray();
                }
            }
        }

        protected internal void Signout()
        {
            if (!InfluxDBClientOptions.AuthenticationScheme.Session.Equals(_options.AuthScheme) || _signout)
            {
                _signout = true;

                return;
            }

            _signout = true;
            _sessionToken = null;

            var request = new RestRequest("/api/v2/signout", Method.POST);
            RestClient.Execute(request);
        }
    }
}