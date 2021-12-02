using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using InfluxDB.Client.Core.Internal;

namespace InfluxDB.Client.Api.Client
{
    public partial class ApiClient
    {
        private readonly List<string> _noAuthRoute = new List<string>
            {"/api/v2/signin", "/api/v2/signout", "/api/v2/setup"};

        private readonly InfluxDBClientOptions _options;
        private readonly LoggingHandler _loggingHandler;
        private readonly GzipHandler _gzipHandler;

        private string _sessionTokens;
        private bool _signout;
        internal readonly Configuration Configuration;

        public ApiClient(InfluxDBClientOptions options, LoggingHandler loggingHandler, GzipHandler gzipHandler)
        {
            _options = options;
            _loggingHandler = loggingHandler;
            _gzipHandler = gzipHandler;

            var timeoutTotalMilliseconds = (int) options.Timeout.TotalMilliseconds;
            var totalMilliseconds = (int) options.ReadWriteTimeout.TotalMilliseconds;

            RestClient = new RestClient(options.Url);
            RestClient.FollowRedirects = options.AllowHttpRedirects;
            if (!options.VerifySsl)
            {
                RestClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }
            RestClient.AutomaticDecompression = false;
            Configuration = new Configuration
            {
                BasePath = options.Url,
                Timeout = timeoutTotalMilliseconds,
                ReadWriteTimeout = totalMilliseconds,
            };
            Configuration.Proxy = options.WebProxy;
        }

        partial void InterceptRequest(HttpRequestMessage req)
        {
            BeforeIntercept(req);
        }

        partial void InterceptResponse(HttpRequestMessage req, HttpResponseMessage response)
        {
            AfterIntercept((int) response.StatusCode, () => response.Headers, response.Content);
        }
        
        internal void BeforeIntercept(HttpRequestMessage req)
        {
            if (_signout || _noAuthRoute.Any(requestPath => requestPath.EndsWith(req.RequestUri.AbsolutePath))) return;

            if (InfluxDBClientOptions.AuthenticationScheme.Token.Equals(_options.AuthScheme))
            {
                req.Headers.Add("Authorization", "Token " + new string(_options.Token));
            }
            else if (InfluxDBClientOptions.AuthenticationScheme.Session.Equals(_options.AuthScheme))
            {
                InitToken();

                AddRequestTokens(req, _sessionTokens);
            }
            
            _loggingHandler.BeforeIntercept(req);
            _gzipHandler.BeforeIntercept(req);
        }

        internal T AfterIntercept<T>(int statusCode, Func<HttpResponseHeaders> headers, T body)
        {
            var uncompressed = _gzipHandler.AfterIntercept(statusCode, headers, body);
            return (T) _loggingHandler.AfterIntercept(statusCode, headers, uncompressed);
        }

        private void InitToken()
        {
            if (!InfluxDBClientOptions.AuthenticationScheme.Session.Equals(_options.AuthScheme) || _signout) return;

            if (_sessionTokens == null)
            {
                ApiResponse<object> authResponse;
                try
                {
                    var header = "Basic " + Convert.ToBase64String(
                        Encoding.Default.GetBytes(
                            _options.Username + ":" + new string(_options.Password)));

                    var localVarRequestOptions = new RequestOptions();
                    localVarRequestOptions.HeaderParameters.Add("Authorization", header);

                    authResponse = Post<object>("/api/v2/signin", localVarRequestOptions, Configuration);
                }
                catch (IOException e)
                {
                    Trace.WriteLine("Cannot retrieve the Session token!");
                    Trace.WriteLine(e);
                    return;
                }

                if (authResponse.Headers.TryGetValue("Set-Cookie", out var cookie))
                {
                    _sessionTokens = cookie.FirstOrDefault();
                }
                else
                {
                    _sessionTokens = null;
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

            var localVarRequestOptions = new RequestOptions();

            AddRequestTokens(localVarRequestOptions, signOutSessionToken);
            Post<object>("/api/v2/signout", localVarRequestOptions, Configuration);
        }

        private static void AddRequestTokens(RequestOptions request, string tokens)
        {
            if (string.IsNullOrEmpty(tokens))
                return;
            
            request.HeaderParameters.Add("Cookie", tokens);
        }

        private static void AddRequestTokens(HttpRequestMessage request, string tokens)
        {
            if (string.IsNullOrEmpty(tokens))
                return;
            
            request.Headers.Add("Cookie", tokens);
        }
    }
}