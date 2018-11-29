using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using InfluxData.Platform.Client.Option;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;
using Task = System.Threading.Tasks.Task;

namespace InfluxData.Platform.Client.Client
{
    public class PlatformClient : AbstractClient
    {
        private readonly AuthenticateDelegatingHandler _authenticateDelegatingHandler;

        protected internal PlatformClient(PlatformOptions options)
        {
            Arguments.CheckNotNull(options, "PlatformOptions");

            _authenticateDelegatingHandler = new AuthenticateDelegatingHandler(options);

            Client.HttpClient = new HttpClient(_authenticateDelegatingHandler);
            Client.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Client.HttpClient.BaseAddress = new Uri(options.Url);
            Client.HttpClient.Timeout = options.Timeout;
        }

        /// <summary>
        /// Get the <see cref="Domain.Organization"/> client.
        /// </summary>
        /// <returns>the new client instance for Organization API</returns>
        public OrganizationClient CreateOrganizationClient()
        {
            return new OrganizationClient(Client);
        }

        /// <summary>
        /// Get the <see cref="Domain.User"/> client.
        /// </summary>
        /// <returns>the new client instance for User API</returns>
        public UserClient CreateUserClient()
        {
            return new UserClient(Client);
        }

        /// <summary>
        /// Get the <see cref="Domain.Bucket"/> client.
        /// </summary>
        /// <returns>the new client instance for Bucket API</returns>
        public BucketClient CreateBucketClient()
        {
            return new BucketClient(Client);
        }
        
        /// <summary>
        /// Get the <see cref="Domain.Source"/> client.
        /// </summary>
        /// <returns>the new client instance for Source API</returns>
        public SourceClient CreateSourceClient()
        {
            return new SourceClient(Client);
        }
        
        /// <summary>
        /// Get the <see cref="Domain.Authorization"/> client.
        /// </summary>
        /// <returns>the new client instance for Authorization API</returns>
        public AuthorizationClient CreateAuthorizationClient()
        {
            return new AuthorizationClient(Client);
        }

        /// <summary>
        /// Get the health of an instance.
        /// </summary>
        /// <returns>health of an instance</returns>
        public async Task<Health> Health()
        {
            try
            {
                var request = await Get("/health");

                return Call<Health>(request);
            }
            catch (Exception e)
            {
                return new Health {Status = "error", Message = e.Message};
            }
        }

        public async Task Close()
        {
            //
            // signout
            //
            try
            {
                await _authenticateDelegatingHandler.Signout();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("The signout exception");
                Console.WriteLine(e);
            }
        }
    }
}