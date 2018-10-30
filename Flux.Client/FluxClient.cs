using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Flux.Client.Client;
using Flux.Flux.Options;
using Newtonsoft.Json;
using Platform.Common.Flux.Csv;
using Platform.Common.Flux.Domain;
using Platform.Common.Flux.Error;
using Platform.Common.Platform.Rest;

namespace Flux.Client
{
    public class FluxClient : IFluxClient
    {
        private readonly DefaultClientIO _client;
        private readonly FluxConnectionOptions _options;
        
        private FluxCsvParser _csvParser = new FluxCsvParser(); 

        public FluxClient(FluxConnectionOptions options)
        {
            _options = options ?? throw new ArgumentException("options");
            _client = new DefaultClientIO(options);
        }

        public async Task<List<FluxTable>> Query(string query)
        {
            var responseHttp = await _client.DoRequest(FluxService.Query(
                                            FluxService.CreateBody(FluxService.GetDefaultDialect(), query)))
                            .ConfigureAwait(false);

            RaiseForInfluxError(responseHttp);

            var consumer = new FluxCsvParser.FluxResponseConsumerTable();

            _csvParser.ParseFluxResponse(responseHttp.ResponseContent, null, consumer);

            return consumer.Tables;
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
                
                RaiseForInfluxError(responseHttp);
                
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return false;
            }
        }

        public string Version()
        {
            throw new NotImplementedException();
        }

        internal struct ErrorsWrapper
        {
            public IReadOnlyList<string> Errors;
        }

        internal static void RaiseForInfluxError(RequestResult resultRequest)
        {
            var statusCode = resultRequest.StatusCode;

            if (statusCode >= 200 && statusCode < 300)
            {
                return;
            }

            var wrapper = resultRequest.ResponseContent.Length > 1
                            ? JsonConvert.DeserializeObject<ErrorsWrapper>(resultRequest.ResponseContent.ToString())
                            : new ErrorsWrapper();

            var response = new QueryErrorResponse(statusCode, wrapper.Errors);

            var message = InfluxException.GetErrorMessage(resultRequest);

            if (message != null)
            {
                throw new InfluxException(response);
            }

            throw new HttpException(response);
        }
    }
}