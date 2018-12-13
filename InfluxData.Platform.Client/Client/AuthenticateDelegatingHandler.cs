using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Option;
using Microsoft.Net.Http.Headers;
using Platform.Common;
using Platform.Common.Platform;

namespace InfluxData.Platform.Client.Client
{
    class AuthenticateDelegatingHandler : DelegatingHandler
    {
        private PlatformOptions _platformOptions;

        private char[] _sessionToken;
        private bool _signout;
        
        public AuthenticateDelegatingHandler(PlatformOptions platformOptions)
        {
            Arguments.CheckNotNull(platformOptions, "PlatformOptions");

            InnerHandler = new HttpClientHandler();

            _platformOptions = platformOptions;
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(
                        HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (PlatformOptions.AuthenticationScheme.Token.Equals(_platformOptions.AuthScheme))
            {
                request.Headers.Add("Authorization", "Token " + String(_platformOptions.Token));
            } 
            else if (PlatformOptions.AuthenticationScheme.Session.Equals(_platformOptions.AuthScheme)) 
            {
                await InitToken(cancellationToken);

                if (_sessionToken != null) 
                {
                    request.Headers.Add("Cookie", "session=" + String(_sessionToken));
                }
            }
            
            // Call the inner handler.
            return await base.SendAsync(request, cancellationToken);
        }
        
        /// <summary>
        /// Init the Session token if is <see cref="InfluxData.Platform.Client.Option.PlatformOptions.AuthenticationScheme.Session"/> used.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>async task</returns>
        private async Task InitToken(CancellationToken cancellationToken) 
        {
            if (!PlatformOptions.AuthenticationScheme.Session.Equals(_platformOptions.AuthScheme) || _signout) 
            {
                return;
            }

            //TODO or expired
            if (_sessionToken == null)
            {
                var authRequest = new HttpRequestMessage(HttpMethod.Post, _platformOptions.Url + "/api/v2/signin");
                var header = AuthorizationHeader(_platformOptions.Username, String(_platformOptions.Password));
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
                {
                    _sessionToken = SetCookieHeaderValue.ParseList(values.ToList())
                                    .ToList().First(cookie => cookie.Name.ToString().Equals("session")).Value.ToString().ToCharArray();
                }
            }
        }

        protected internal static string AuthorizationHeader(string username, string password)
        {
            return "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(username + ":" + password));
        }

        /// <summary>
        /// Expire the current session.
        /// </summary>
        /// <returns>async task</returns>
        protected internal async Task Signout()
        {
            if (!PlatformOptions.AuthenticationScheme.Session.Equals(_platformOptions.AuthScheme) || _signout)
            {
                return;
            }

            _signout = true;
            _sessionToken = null;

            var authRequest = new HttpRequestMessage(new HttpMethod(HttpMethodKind.Post.Name()), _platformOptions.Url + "/api/v2/signout");

            await base.SendAsync(authRequest, new CancellationToken());
        }
        
        private string String(char[] password) 
        {
            return new string(password);
        }
    }
}