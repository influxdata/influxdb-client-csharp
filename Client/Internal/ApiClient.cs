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

        private IList<KeyValuePair<string, string>> _sessionTokens; //key is name of cookie, value is the value
        private bool _signout;

        public ApiClient(InfluxDBClientOptions options, LoggingHandler loggingHandler, GzipHandler gzipHandler)
        {
            _options = options;
            _loggingHandler = loggingHandler;
            _gzipHandler = gzipHandler;

            var timeoutTotalMilliseconds = (int) options.Timeout.TotalMilliseconds;
            var totalMilliseconds = (int) options.ReadWriteTimeout.TotalMilliseconds;

            RestClient = new RestClient(options.Url);
            RestClient.FollowRedirects = options.AllowHttpRedirects;
            RestClient.AutomaticDecompression = false;
            Configuration = new Configuration
            {
                ApiClient = this,
                BasePath = options.Url,
                Timeout = timeoutTotalMilliseconds,
                ReadWriteTimeout = totalMilliseconds,
            };
            RestClient.Proxy = options.WebProxy;
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

                AddRequestTokens(request, _sessionTokens);
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

            if (_sessionTokens == null)
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
                    _sessionTokens = authResponse.Cookies
                        .Select(rrc => new KeyValuePair<string, string>(rrc.Name, rrc.Value))
                        .ToArray();
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

            var signOutSessionToken = _sessionTokens;
            _sessionTokens = null;

            var request = new RestRequest("/api/v2/signout", Method.POST);
            AddRequestTokens(request, signOutSessionToken);
            RestClient.Execute(request);
        }

        private static void AddRequestTokens(IRestRequest request, IList<KeyValuePair<string, string>> tokens)
        {
            if (tokens == null)
                return;
            foreach (var kvp in tokens)
                request.AddCookie(kvp.Key, kvp.Value);
        }
    }
}