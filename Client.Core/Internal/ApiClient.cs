using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using InfluxDB.Client.Core.Internal;

[assembly: InternalsVisibleTo("Client.Test, PublicKey=002400000480000094000000060200000024000052534131" +
                              "0004000001000100efaac865f88dd35c90dc548945405aae34056eedbe42cad60971f89a861a78437e86d" +
                              "95804a1aeeb0de18ac3728782f9dc8dbae2e806167a8bb64c0402278edcefd78c13dbe7f8d13de36eb362" +
                              "21ec215c66ee2dfe7943de97b869c5eea4d92f92d345ced67de5ac8fc3cd2f8dd7e3c0c53bdb0cc433af8" +
                              "59033d069cad397a7")]
namespace InfluxDB.Client.Core.Api
{
    public partial class ApiClient
    {
        public enum AuthenticationType
        {
            Session,
            Token,
        }

        private readonly List<string> _noAuthRoute = new List<string>
            { "/api/v2/signin", "/api/v2/signout", "/api/v2/setup" };

        private readonly LoggingHandler _loggingHandler;
        private readonly GzipHandler _gzipHandler;

        private string _sessionTokens;
        private bool _signout;
        internal readonly Configuration Configuration;
        private readonly char[] _token;
        private readonly string _username;
        private readonly char[] _password;
        private readonly AuthenticationType _authScheme;

        public ApiClient(string url, char[] token, string username, char[] password, AuthenticationType authScheme,
            TimeSpan timeout, TimeSpan readWriteTimeout, bool allowHttpRedirects, bool verifySsl, IWebProxy webProxy,
            LoggingHandler loggingHandler, GzipHandler gzipHandler)
        {
            _token = token;
            _username = username;
            _password = password;
            _authScheme = authScheme;
            _loggingHandler = loggingHandler;
            _gzipHandler = gzipHandler;

            var timeoutTotalMilliseconds = (int)timeout.TotalMilliseconds;
            var totalMilliseconds = (int)readWriteTimeout.TotalMilliseconds;

            RestClient = new RestClient(url);
            RestClient.FollowRedirects = allowHttpRedirects;
            if (!verifySsl)
            {
                RestClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }

            RestClient.AutomaticDecompression = false;
            Configuration = new Configuration
            {
                BasePath = url,
                Timeout = timeoutTotalMilliseconds,
                ReadWriteTimeout = totalMilliseconds,
            };
            Configuration.Proxy = webProxy;
        }

        partial void InterceptRequest(HttpRequestMessage req)
        {
            BeforeIntercept(req);
        }

        partial void InterceptResponse(HttpRequestMessage req, HttpResponseMessage response)
        {
            AfterIntercept((int)response.StatusCode, () => response.Headers, response.Content);
        }

        internal void BeforeIntercept(HttpRequestMessage req)
        {
            if (_signout || _noAuthRoute.Any(requestPath => requestPath.EndsWith(req.RequestUri.AbsolutePath))) return;

            if (AuthenticationType.Token.Equals(_authScheme))
            {
                req.Headers.Add("Authorization", "Token " + new string(_token));
            }
            else if (AuthenticationType.Session.Equals(_authScheme))
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
            return (T)_loggingHandler.AfterIntercept(statusCode, headers, uncompressed);
        }

        private void InitToken()
        {
            if (!AuthenticationType.Session.Equals(_authScheme) || _signout) return;

            if (_sessionTokens == null)
            {
                ApiResponse<object> authResponse;
                try
                {
                    var header = "Basic " + Convert.ToBase64String(
                        Encoding.Default.GetBytes(
                            _username + ":" + new string(_password)));

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
            if (!AuthenticationType.Session.Equals(_authScheme) || _signout)
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