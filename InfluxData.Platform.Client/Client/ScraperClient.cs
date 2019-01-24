using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;
using Task = System.Threading.Tasks.Task;

namespace InfluxData.Platform.Client.Client
{
    public class ScraperClient : AbstractPlatformClient
    {
        protected internal ScraperClient(DefaultClientIo client) : base(client)
        {
        }

        /// <summary>
        /// Creates a new ScraperTarget and sets <see cref="ScraperTarget.Id"/> with the new identifier.
        /// </summary>
        /// <param name="scraperTarget">the scraper to create</param>
        /// <returns>created ScraperTarget</returns>
        public async Task<ScraperTargetResponse> CreateScraperTarget(ScraperTarget scraperTarget)
        {
            Arguments.CheckNotNull(scraperTarget, nameof(scraperTarget));

            var response = await Post(scraperTarget, "/api/v2/scrapers");

            return Call<ScraperTargetResponse>(response);
        }

        /// <summary>
        /// Creates a new ScraperTarget and sets <see cref="ScraperTarget.Id"/> with the new identifier.
        /// </summary>
        /// <param name="name">the name of the new ScraperTarget</param>
        /// <param name="url">the url of the new ScraperTarget</param>
        /// <param name="bucketId">the id of the bucket that its use to writes</param>
        /// <param name="orgId">the id of the organization that owns new ScraperTarget</param>
        /// <returns>created ScraperTarget</returns>
        public async Task<ScraperTargetResponse> CreateScraperTarget(string name, string url,
            string bucketId, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(url, nameof(url));
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var scrapperTarget = new ScraperTarget
                {Name = name, Url = url, BucketId = bucketId, OrgId = orgId, Type = ScraperType.Prometheus};
            
            return await CreateScraperTarget(scrapperTarget);
        }
        
        /// <summary>
        /// Update a ScraperTarget.
        /// </summary>
        /// <param name="scraperTarget">ScraperTarget update to apply</param>
        /// <returns>updated ScraperTarget</returns>
        public async Task<ScraperTargetResponse> UpdateScraperTarget(ScraperTarget scraperTarget)
        {
            Arguments.CheckNotNull(scraperTarget, nameof(scraperTarget));

            var result = await Patch(scraperTarget, $"/api/v2/scrapers/{scraperTarget.Id}");

            return Call<ScraperTargetResponse>(result);
        }
        
        /// <summary>
        /// Delete a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of ScraperTarget to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteScraperTarget(string scraperTargetId)
        {
            Arguments.CheckNotNull(scraperTargetId, nameof(scraperTargetId));

            var request = await Delete($"/api/v2/scrapers/{scraperTargetId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        /// Delete a ScraperTarget.
        /// </summary>
        /// <param name="scraperTarget">ScraperTarget to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteScraperTarget(ScraperTarget scraperTarget)
        {
            Arguments.CheckNotNull(scraperTarget, nameof(scraperTarget));

            await DeleteScraperTarget(scraperTarget.Id);
        }
         
        /// <summary>
        /// Retrieve a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of ScraperTarget to get</param>
        /// <returns>ScraperTarget details</returns>
        public async Task<ScraperTargetResponse> FindScraperTargetById(string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            var request = await Get($"/api/v2/scrapers/{scraperTargetId}");

            return Call<ScraperTargetResponse>(request, 404);
        }
        
        /// <summary>
        /// Get all ScraperTargets.
        /// </summary>
        /// <returns>A list of ScraperTargets</returns>
        public async Task<List<ScraperTargetResponse>> FindScraperTargets()
        {
            var request = await Get("/api/v2/scrapers");

            var responses = Call<ScraperTargetResponses>(request);

            return responses.TargetResponses;
        }
    }
}