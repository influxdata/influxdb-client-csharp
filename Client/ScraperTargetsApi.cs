using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client
{
    public class ScraperTargetsApi
    {
        private readonly ScraperTargetsService _service;

        protected internal ScraperTargetsApi(ScraperTargetsService service)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
        }

        /// <summary>
        /// Creates a new ScraperTarget and sets <see cref="ScraperTargetResponse.Id" /> with the new identifier.
        /// </summary>
        /// <param name="scraperTargetRequest">the scraper to create</param>
        /// <returns>created ScraperTarget</returns>
        public async Task<ScraperTargetResponse> CreateScraperTargetAsync(ScraperTargetRequest scraperTargetRequest)
        {
            Arguments.CheckNotNull(scraperTargetRequest, nameof(scraperTargetRequest));

            return await _service.PostScrapersAsync(scraperTargetRequest);
        }

        /// <summary>
        /// Creates a new ScraperTarget and sets <see cref="ScraperTargetResponse.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">the name of the new ScraperTarget</param>
        /// <param name="url">the url of the new ScraperTarget</param>
        /// <param name="bucketId">the id of the bucket that its use to writes</param>
        /// <param name="orgId">the id of the organization that owns new ScraperTarget</param>
        /// <returns>created ScraperTarget</returns>
        public async Task<ScraperTargetResponse> CreateScraperTargetAsync(string name, string url,
            string bucketId, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(url, nameof(url));
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var scrapperTarget =
                new ScraperTargetRequest(name, ScraperTargetRequest.TypeEnum.Prometheus, url, orgId, bucketId);

            return await CreateScraperTargetAsync(scrapperTarget);
        }

        /// <summary>
        /// Update a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetResponse">ScraperTarget update to apply</param>
        /// <returns>updated ScraperTarget</returns>
        public async Task<ScraperTargetResponse> UpdateScraperTargetAsync(ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            return await UpdateScraperTargetAsync(scraperTargetResponse.Id, scraperTargetResponse);
        }

        /// <summary>
        /// Update a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">id of the scraper target (required)</param>
        /// <param name="scraperTargetRequest">ScraperTargetRequest update to apply</param>
        /// <returns>updated ScraperTarget</returns>
        public async Task<ScraperTargetResponse> UpdateScraperTargetAsync(string scraperTargetId,
            ScraperTargetRequest scraperTargetRequest)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNotNull(scraperTargetRequest, nameof(scraperTargetRequest));

            return await _service.PatchScrapersIDAsync(scraperTargetId, scraperTargetRequest);
        }

        /// <summary>
        /// Delete a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of ScraperTarget to delete</param>
        /// <returns>scraper target deleted</returns>
        public async Task DeleteScraperTargetAsync(string scraperTargetId)
        {
            Arguments.CheckNotNull(scraperTargetId, nameof(scraperTargetId));

            await _service.DeleteScrapersIDAsync(scraperTargetId);
        }

        /// <summary>
        /// Delete a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetResponse">ScraperTarget to delete</param>
        /// <returns>scraper target deleted</returns>
        public async Task DeleteScraperTargetAsync(ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            await DeleteScraperTargetAsync(scraperTargetResponse.Id);
        }

        /// <summary>
        /// Clone a ScraperTarget.
        /// </summary>
        /// <param name="clonedName">name of cloned ScraperTarget</param>
        /// <param name="scraperTargetId">ID of ScraperTarget to clone</param>
        /// <returns>cloned ScraperTarget</returns>
        public async Task<ScraperTargetResponse> CloneScraperTargetAsync(string clonedName, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            var scraperTarget = FindScraperTargetByIdAsync(scraperTargetId);

            return await scraperTarget.ContinueWith(t => CloneScraperTargetAsync(clonedName, t.Result)).Unwrap();
        }

        /// <summary>
        /// Clone a ScraperTarget.
        /// </summary>
        /// <param name="clonedName">name of cloned ScraperTarget</param>
        /// <param name="scraperTargetResponse">ScraperTarget to clone</param>
        /// <returns>cloned ScraperTarget</returns>
        public async Task<ScraperTargetResponse> CloneScraperTargetAsync(string clonedName,
            ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            var cloned = new ScraperTargetRequest(clonedName, scraperTargetResponse.Type, scraperTargetResponse.Url,
                scraperTargetResponse.OrgID, scraperTargetResponse.BucketID);

            return await CreateScraperTargetAsync(cloned).ContinueWith(created =>
            {
                //
                // Add labels
                //
                return GetLabelsAsync(scraperTargetResponse)
                    .ContinueWith(labels => { return labels.Result.Select(label => AddLabelAsync(label, created.Result)); })
                    .ContinueWith(async tasks =>
                    {
                        await Task.WhenAll(tasks.Result);
                        return created.Result;
                    })
                    .Unwrap();
            }).Unwrap();
        }

        /// <summary>
        /// Retrieve a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of ScraperTarget to get</param>
        /// <returns>ScraperTarget details</returns>
        public async Task<ScraperTargetResponse> FindScraperTargetByIdAsync(string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            return await _service.GetScrapersIDAsync(scraperTargetId);
        }

        /// <summary>
        /// Get all ScraperTargets.
        /// </summary>
        /// <returns>A list of ScraperTargets</returns>
        public async Task<List<ScraperTargetResponse>> FindScraperTargetsAsync()
        {
            return (await _service.GetScrapersAsync()).Configurations;
        }

        /// <summary>
        /// Get all ScraperTargets.
        /// </summary>
        /// <param name="organization">specifies the organization of the resource</param>
        /// <returns>A list of ScraperTargets</returns>
        public async Task<List<ScraperTargetResponse>> FindScraperTargetsByOrgAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await FindScraperTargetsByOrgIdAsync(organization.Id);
        }

        /// <summary>
        /// Get all ScraperTargets.
        /// </summary>
        /// <param name="orgId">specifies the organization ID of the resource</param>
        /// <returns>A list of ScraperTargets</returns>
        public async Task<List<ScraperTargetResponse>> FindScraperTargetsByOrgIdAsync(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return (await _service.GetScrapersAsync(null, orgId)).Configurations;
        }

        /// <summary>
        /// List all members of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetResponse">ScraperTarget of the members</param>
        /// <returns>the List all members of a ScraperTarget</returns>
        public async Task<List<ResourceMember>> GetMembersAsync(ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            return await GetMembersAsync(scraperTargetResponse.Id);
        }

        /// <summary>
        /// List all members of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of ScraperTarget to get members</param>
        /// <returns>the List all members of a ScraperTarget</returns>
        public async Task<List<ResourceMember>> GetMembersAsync(string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            return (await _service.GetScrapersIDMembersAsync(scraperTargetId)).Users;
        }

        /// <summary>
        /// Add a ScraperTarget member.
        /// </summary>
        /// <param name="member">the member of a scraperTarget</param>
        /// <param name="scraperTargetResponse">the ScraperTarget of a member</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddMemberAsync(User member, ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(member, nameof(member));

            return await AddMemberAsync(member.Id, scraperTargetResponse.Id);
        }

        /// <summary>
        /// Add a ScraperTarget member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="scraperTargetId">the ID of a scraperTarget</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddMemberAsync(string memberId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return await _service.PostScrapersIDMembersAsync(scraperTargetId,
                new AddResourceMemberRequestBody(memberId));
        }

        /// <summary>
        /// Removes a member from a ScraperTarget.
        /// </summary>
        /// <param name="member">the member of a ScraperTarget</param>
        /// <param name="scraperTargetResponse">the ScraperTarget of a member</param>
        /// <returns>async task</returns>
        public async Task DeleteMemberAsync(User member, ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(member, nameof(member));

            await DeleteMemberAsync(member.Id, scraperTargetResponse.Id);
        }

        /// <summary>
        /// Removes a member from a ScraperTarget.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <returns>async task</returns>
        public async Task DeleteMemberAsync(string memberId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            await _service.DeleteScrapersIDMembersIDAsync(memberId, scraperTargetId);
        }

        /// <summary>
        /// List all owners of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetResponse">ScraperTarget of the owners</param>
        /// <returns>the List all owners of a ScraperTarget</returns>
        public async Task<List<ResourceOwner>> GetOwnersAsync(ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            return await GetOwnersAsync(scraperTargetResponse.Id);
        }

        /// <summary>
        /// List all owners of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of a ScraperTarget to get owners</param>
        /// <returns>the List all owners of a scraperTarget</returns>
        public async Task<List<ResourceOwner>> GetOwnersAsync(string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            return (await _service.GetScrapersIDOwnersAsync(scraperTargetId)).Users;
        }

        /// <summary>
        /// Add a ScraperTarget owner.
        /// </summary>
        /// <param name="owner">the owner of a ScraperTarget</param>
        /// <param name="scraperTargetResponse">the ScraperTarget of a owner</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceOwner> AddOwnerAsync(User owner, ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(owner, nameof(owner));

            return await AddOwnerAsync(owner.Id, scraperTargetResponse.Id);
        }

        /// <summary>
        /// Add a ScraperTarget owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceOwner> AddOwnerAsync(string ownerId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var memberRequest = new AddResourceMemberRequestBody(ownerId);

            return await _service.PostScrapersIDOwnersAsync(scraperTargetId, memberRequest);
        }

        /// <summary>
        /// Removes a owner from a ScraperTarget.
        /// </summary>
        /// <param name="owner">the owner of a scraperTarget</param>
        /// <param name="scraperTargetResponse">the ScraperTarget of a owner</param>
        /// <returns>async task</returns>
        public async Task DeleteOwnerAsync(User owner, ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(owner, nameof(owner));

            await DeleteOwnerAsync(owner.Id, scraperTargetResponse.Id);
        }

        /// <summary>
        /// Removes a owner from a ScraperTarget.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <returns>async task</returns>
        public async Task DeleteOwnerAsync(string ownerId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            await _service.DeleteScrapersIDOwnersIDAsync(ownerId, scraperTargetId);
        }

        /// <summary>
        /// List all labels of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetResponse">a ScraperTarget of the labels</param>
        /// <returns>the List all labels of a ScraperTarget</returns>
        public async Task<List<Label>> GetLabelsAsync(ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            return await GetLabelsAsync(scraperTargetResponse.Id);
        }

        /// <summary>
        /// List all labels of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of a ScraperTarget to get labels</param>
        /// <returns>the List all labels of a ScraperTarget</returns>
        public async Task<List<Label>> GetLabelsAsync(string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            return (await _service.GetScrapersIDLabelsAsync(scraperTargetId)).Labels;
        }

        /// <summary>
        /// Add a ScraperTarget label.
        /// </summary>
        /// <param name="label">the label of a ScraperTarget</param>
        /// <param name="scraperTargetResponse">a ScraperTarget of a label</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabelAsync(Label label, ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(label, nameof(label));

            return await AddLabelAsync(label.Id, scraperTargetResponse.Id);
        }

        /// <summary>
        /// Add a ScraperTarget label.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabelAsync(string labelId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var mapping = new LabelMapping(labelId);

            return (await _service.PostScrapersIDLabelsAsync(scraperTargetId, mapping)).Label;
        }

        /// <summary>
        /// Removes a label from a ScraperTarget.
        /// </summary>
        /// <param name="label">the label of a ScraperTarget</param>
        /// <param name="scraperTargetResponse">a ScraperTarget of a owner</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteLabelAsync(Label label, ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(label, nameof(label));

            await DeleteLabelAsync(label.Id, scraperTargetResponse.Id);
        }

        /// <summary>
        /// Removes a label from a ScraperTarget.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteLabelAsync(string labelId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            await _service.DeleteScrapersIDLabelsIDAsync(scraperTargetId, labelId);
        }
    }
}