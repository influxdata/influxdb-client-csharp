using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using RestSharp;

namespace InfluxDB.Client.Api.Client
{
    public partial class ApiClient
    {
        private readonly List<string> _noAuthRoute = new List<string>
            {"/api/v2/signin", "/api/v2/signout", "/api/v2/setup"};

        private readonly InfluxDBClientOptions _options;

        private char[] _sessionToken;
        private bool _signout;

        public ApiClient(InfluxDBClientOptions options)
        {
            _options = options;

            Configuration = new Configuration
            {
                BasePath = options.Url,
                Timeout = 10_000,
                ApiClient = this
            };

            RestClient = new RestClient(options.Url);
        }

        partial void InterceptRequest(IRestRequest request)
        {
            Intercept(request);
        }
        
        internal void Intercept(IRestRequest request)
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

                    var request = new RestRequest(_options.Url + "/api/v2/signin", Method.POST)
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

            var request = new RestRequest(_options.Url + "/api/v2/signout", Method.POST);
            RestClient.Execute(request);
        }
    }
}