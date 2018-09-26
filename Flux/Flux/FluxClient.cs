using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Flux.flux.dto;
using Flux.Flux.Client;
using Flux.Flux.Options;
using Newtonsoft.Json;

namespace Flux.Flux
{
    public class FluxClient : IFluxClient
    {
        private readonly DefaultClientIO _client;
        private readonly FluxConnectionOptions _options;

        public FluxClient(FluxConnectionOptions options)
        {
            _options = options ?? throw new ArgumentException("options");
            _client = new DefaultClientIO(options);
        }

        public List<FluxTable> Flux(string query)
        {
            throw new NotImplementedException();
        }

        public HttpResponseMessage FluxRaw(string query)
        {
            throw new NotImplementedException();
        }

        public HttpResponseMessage FluxRaw(string query, FluxOptions options)
        {
            throw new NotImplementedException();
        }

        public void FluxRaw(string query, Action<HttpResponseMessage> onResponse)
        {
            throw new NotImplementedException();
        }

        public void FluxRaw(string query, Action<HttpResponseMessage> onResponse, Action onFailure)
        {
            throw new NotImplementedException();
        }

        public void FluxRaw(string query, FluxOptions options, Action<HttpResponseMessage> onResponse)
        {
            throw new NotImplementedException();
        }

        public void FluxRaw(string query, FluxOptions options, Action<HttpResponseMessage> onResponse, Action onFailure)
        {
            throw new NotImplementedException();
        }

        /** <summary>
         * Check service health.
         * </summary>
         */
        public async Task<bool> Ping()
        {
            try
            {
                var responseHttp = await _client.DoRequest(FluxService.Ping()).ConfigureAwait(false);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return false;
            }
        }
        
        /*static object FromJson(string json)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    DateParseHandling = DateParseHandling.None
                };
                
                return JsonConvert.DeserializeObject<object>(json, settings);
            }
            catch (JsonReaderException ex)
            {
                throw new Exception($"Bad JSON: {ex}");
            }
        }*/
    }
}