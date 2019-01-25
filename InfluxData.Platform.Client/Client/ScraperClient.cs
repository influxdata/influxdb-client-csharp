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
        
         /// <summary>
        /// List all members of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTarget">ScraperTarget of the members</param>
        /// <returns>the List all members of a ScraperTarget</returns>
        public async Task<List<ResourceMember>> GetMembers(ScraperTarget scraperTarget)
        {
            Arguments.CheckNotNull(scraperTarget, nameof(scraperTarget));

            return await GetMembers(scraperTarget.Id);
        }

        /// <summary>
        /// List all members of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of ScraperTarget to get members</param>
        /// <returns>the List all members of a ScraperTarget</returns>
        public async Task<List<ResourceMember>> GetMembers(string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            var request = await Get($"/api/v2/scrapers/{scraperTargetId}/members");

            var response = Call<ResourceMembers>(request);

            return response?.Users;
        }

        /// <summary>
        /// Add a ScraperTarget member.
        /// </summary>
        /// <param name="member">the member of a scraperTarget</param>
        /// <param name="scraperTarget">the ScraperTarget of a member</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddMember(User member, ScraperTarget scraperTarget)
        {
            Arguments.CheckNotNull(scraperTarget, nameof(scraperTarget));
            Arguments.CheckNotNull(member, nameof(member));

            return await AddMember(member.Id, scraperTarget.Id);
        }

        /// <summary>
        /// Add a ScraperTarget member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="scraperTargetId">the ID of a scraperTarget</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddMember(string memberId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            var user = new User {Id = memberId};

            var request = await Post(user, $"/api/v2/scrapers/{scraperTargetId}/members");

            return Call<ResourceMember>(request);
        }

        /// <summary>
        /// Removes a member from a ScraperTarget.
        /// </summary>
        /// <param name="member">the member of a ScraperTarget</param>
        /// <param name="scraperTarget">the ScraperTarget of a member</param>
        /// <returns>async task</returns>
        public async Task DeleteMember(User member, ScraperTarget scraperTarget)
        {
            Arguments.CheckNotNull(scraperTarget, nameof(scraperTarget));
            Arguments.CheckNotNull(member, nameof(member));

            await DeleteMember(member.Id, scraperTarget.Id);
        }

        /// <summary>
        /// Removes a member from a ScraperTarget.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <returns>async task</returns>
        public async Task DeleteMember(string memberId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));
            
            var request = await Delete($"/api/v2/scrapers/{scraperTargetId}/members/{memberId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        /// List all owners of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTarget">ScraperTarget of the owners</param>
        /// <returns>the List all owners of a ScraperTarget</returns>
        public async Task<List<ResourceMember>> GetOwners(ScraperTarget scraperTarget)
        {
            Arguments.CheckNotNull(scraperTarget, nameof(scraperTarget));

            return await GetOwners(scraperTarget.Id);
        }

        /// <summary>
        /// List all owners of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of a ScraperTarget to get owners</param>
        /// <returns>the List all owners of a scraperTarget</returns>
        public async Task<List<ResourceMember>> GetOwners(string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            var request = await Get($"/api/v2/scrapers/{scraperTargetId}/owners");

            var response = Call<ResourceMembers>(request);

            return response?.Users;
        }

        /// <summary>
        /// Add a ScraperTarget owner.
        /// </summary>
        /// <param name="owner">the owner of a ScraperTarget</param>
        /// <param name="scraperTarget">the ScraperTarget of a owner</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddOwner(User owner, ScraperTarget scraperTarget)
        {
            Arguments.CheckNotNull(scraperTarget, nameof(scraperTarget));
            Arguments.CheckNotNull(owner, nameof(owner));

            return await AddOwner(owner.Id, scraperTarget.Id);
        }

        /// <summary>
        /// Add a ScraperTarget owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddOwner(string ownerId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var user = new User {Id = ownerId};

            var request = await Post(user, $"/api/v2/scrapers/{scraperTargetId}/owners");

            return Call<ResourceMember>(request);
        }

        /// <summary>
        /// Removes a owner from a ScraperTarget.
        /// </summary>
        /// <param name="owner">the owner of a scraperTarget</param>
        /// <param name="scraperTarget">the ScraperTarget of a owner</param>
        /// <returns>async task</returns>
        public async Task DeleteOwner(User owner, ScraperTarget scraperTarget)
        {
            Arguments.CheckNotNull(scraperTarget, nameof(scraperTarget));
            Arguments.CheckNotNull(owner, nameof(owner));

            await DeleteOwner(owner.Id, scraperTarget.Id);
        }

        /// <summary>
        /// Removes a owner from a ScraperTarget.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <returns>async task</returns>
        public async Task DeleteOwner(string ownerId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));
            
            var request = await Delete($"/api/v2/scrapers/{scraperTargetId}/owners/{ownerId}");

            RaiseForInfluxError(request);
        }
    }
}