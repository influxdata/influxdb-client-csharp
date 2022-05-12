using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.Client.Core.Internal;
using RestSharp;
using RestSharp.Authenticators;

namespace InfluxDB.Client.Api.Client
{
    public partial class ApiClient
    {
        private readonly List<string> _noAuthRoute = new List<string>
            { "/api/v2/signin", "/api/v2/signout", "/api/v2/setup" };

        private readonly InfluxDBClientOptions _options;
        private readonly LoggingHandler _loggingHandler;
        private readonly GzipHandler _gzipHandler;
        internal readonly RestClientOptions RestClientOptions;

        private bool _initializedSessionTokens = false;
        private bool _signout;

        public ApiClient(InfluxDBClientOptions options, LoggingHandler loggingHandler, GzipHandler gzipHandler)
        {
            _options = options;
            _loggingHandler = loggingHandler;
            _gzipHandler = gzipHandler;

            var version = AssemblyHelper.GetVersion(typeof(InfluxDBClient));
            RestClientOptions = new RestClientOptions(options.Url)
            {
                Timeout = (int)options.Timeout.TotalMilliseconds,
                UserAgent = $"influxdb-client-csharp/{version}",
                Proxy = options.WebProxy,
                FollowRedirects = options.AllowHttpRedirects,
                AutomaticDecompression = DecompressionMethods.None
            };
            if (!options.VerifySsl)
            {
                RestClientOptions.RemoteCertificateValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;
            }

            if (options.VerifySslCallback != null)
            {
                RestClientOptions.RemoteCertificateValidationCallback = options.VerifySslCallback;
            }

            if (options.ClientCertificates != null)
            {
                RestClientOptions.ClientCertificates ??= new X509CertificateCollection();
                RestClientOptions.ClientCertificates.AddRange(options.ClientCertificates);
            }

            RestClient = new RestClient(RestClientOptions);
            Configuration = new Configuration
            {
                ApiClient = this,
                BasePath = options.Url
            };
        }

        partial void InterceptRequest(RestRequest request)
        {
            BeforeIntercept(request);
        }

        partial void InterceptResponse(RestRequest request, RestResponse response)
        {
            AfterIntercept((int)response.StatusCode, () => response.Headers, response.Content);
        }

        internal void BeforeIntercept(RestRequest request)
        {
            if (_signout || _noAuthRoute.Any(requestPath => requestPath.EndsWith(request.Resource)))
            {
                return;
            }

            if (InfluxDBClientOptions.AuthenticationScheme.Token.Equals(_options.AuthScheme))
            {
                request.AddHeader("Authorization", "Token " + new string(_options.Token));
            }
            else if (InfluxDBClientOptions.AuthenticationScheme.Session.Equals(_options.AuthScheme))
            {
                InitToken();
            }

            _loggingHandler.BeforeIntercept(request);
            _gzipHandler.BeforeIntercept(request);
        }

        internal T AfterIntercept<T>(int statusCode, Func<IEnumerable<HeaderParameter>> headers, T body)
        {
            var uncompressed = _gzipHandler.AfterIntercept(statusCode, headers, body);
            return (T)_loggingHandler.AfterIntercept(statusCode, headers, uncompressed);
        }

        private void InitToken()
        {
            if (!InfluxDBClientOptions.AuthenticationScheme.Session.Equals(_options.AuthScheme) || _signout)
            {
                return;
            }

            if (!_initializedSessionTokens)
            {
                RestResponse authResponse;
                try
                {
                    var header = "Basic " + Convert.ToBase64String(
                        Encoding.Default.GetBytes(
                            _options.Username + ":" + new string(_options.Password)));

                    var request = new RestRequest("/api/v2/signin", Method.Post)
                        .AddHeader("Authorization", header);

                    authResponse = RestClient.ExecuteAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (IOException e)
                {
                    Trace.WriteLine("Cannot retrieve the Session token!");
                    Trace.WriteLine(e);
                    return;
                }

                if (authResponse.Cookies != null)
                {
                    _initializedSessionTokens = true;
                    // The cookies doesn't follow redirects => we have to manually set `Cookie` header by Authenticator.
                    if (_options.AllowHttpRedirects && authResponse.Cookies.Count > 0)
                    {
                        var headerParameter = authResponse
                            .Headers?
                            .FirstOrDefault(it =>
                                string.Equals("Set-Cookie", it.Name, StringComparison.OrdinalIgnoreCase));

                        RestClient.Authenticator = new CookieRedirectAuthenticator(headerParameter);
                    }
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

            _initializedSessionTokens = false;

            var request = new RestRequest("/api/v2/signout", Method.Post);
            RestClient.ExecuteAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();
            RestClient.Authenticator = null;
        }
    }

    /// <summary>
    /// Set Cookies to HTTP Request.
    /// </summary>
    internal class CookieRedirectAuthenticator : AuthenticatorBase
    {
        internal CookieRedirectAuthenticator(Parameter setCookie) : base(setCookie.Value?.ToString() ?? "")
        {
        }

        protected override ValueTask<Parameter> GetAuthenticationParameter(string cookie)
        {
            return new ValueTask<Parameter>(new HeaderParameter("Cookie", cookie));
        }
    }
}