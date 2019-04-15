using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Internal;
using Microsoft.Net.Http.Headers;

namespace InfluxDB.Client.Internal
{
    public class AuthenticateDelegatingHandler : DelegatingHandler
    {
        private readonly InfluxDBClientOptions _options;

        private char[] _sessionToken;
        private bool _signout;

        public AuthenticateDelegatingHandler(InfluxDBClientOptions options)
        {
            Arguments.CheckNotNull(options, "options");

            InnerHandler = new HttpClientHandler();

            _options = options;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_signout) return await base.SendAsync(request, cancellationToken);

            if (InfluxDBClientOptions.AuthenticationScheme.Token.Equals(_options.AuthScheme))
            {
                request.Headers.Add("Authorization", "Token " + String(_options.Token));
            }
            else if (InfluxDBClientOptions.AuthenticationScheme.Session.Equals(_options.AuthScheme))
            {
                await InitToken(cancellationToken);

                if (_sessionToken != null) request.Headers.Add("Cookie", "session=" + String(_sessionToken));
            }

            // Call the inner handler.
            return await base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        ///     Init the Session token if is <see cref="InfluxDBClientOptions.AuthenticationScheme.Session" /> used.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>async task</returns>
        private async Task InitToken(CancellationToken cancellationToken)
        {
            if (!InfluxDBClientOptions.AuthenticationScheme.Session.Equals(_options.AuthScheme) || _signout) return;

            //TODO or expired
            if (_sessionToken == null)
            {
                var authRequest = new HttpRequestMessage(HttpMethod.Post, _options.Url + "/api/v2/signin");
                var header = AuthorizationHeader(_options.Username, String(_options.Password));
                authRequest.Headers.Add("Authorization", header);

                HttpResponseMessage authResponse;

                try
                {
                    authResponse = await base.SendAsync(authRequest, cancellationToken);
                }
                catch (IOException e)
                {
                    Trace.WriteLine("Cannot retrieve the Session token!");
                    Trace.WriteLine(e);
                    return;
                }

                if (authResponse.Headers.TryGetValues("Set-Cookie", out var values))
                    _sessionToken = SetCookieHeaderValue.ParseList(values.ToList())
                        .ToList().First(cookie => cookie.Name.ToString().Equals("session")).Value.ToString()
                        .ToCharArray();
            }
        }

        protected internal static string AuthorizationHeader(string username, string password)
        {
            return "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(username + ":" + password));
        }

        /// <summary>
        ///     Expire the current session.
        /// </summary>
        /// <returns>async task</returns>
        protected internal async Task Signout()
        {
            if (!InfluxDBClientOptions.AuthenticationScheme.Session.Equals(_options.AuthScheme) || _signout)
            {
                _signout = true;

                return;
            }

            _signout = true;
            _sessionToken = null;

            var authRequest = new HttpRequestMessage(new HttpMethod(HttpMethodKind.Post.Name()),
                _options.Url + "/api/v2/signout");

            await base.SendAsync(authRequest, new CancellationToken());
        }

        private string String(char[] password)
        {
            return new string(password);
        }
    }
}