using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Option;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;

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

        public async Task Close()
        {
            //
            // signout
            //
            try
            {
                await _authenticateDelegatingHandler.Signout();
            }
            catch (IOException e)
            {
                Console.WriteLine("The signout exception");
                Console.WriteLine(e);
            }
        }
    }
}