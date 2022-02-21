using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;

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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created ScraperTarget</returns>
        public Task<ScraperTargetResponse> CreateScraperTargetAsync(ScraperTargetRequest scraperTargetRequest,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(scraperTargetRequest, nameof(scraperTargetRequest));

            return _service.PostScrapersAsync(scraperTargetRequest, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Creates a new ScraperTarget and sets <see cref="ScraperTargetResponse.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">the name of the new ScraperTarget</param>
        /// <param name="url">the url of the new ScraperTarget</param>
        /// <param name="bucketId">the id of the bucket that its use to writes</param>
        /// <param name="orgId">the id of the organization that owns new ScraperTarget</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created ScraperTarget</returns>
        public Task<ScraperTargetResponse> CreateScraperTargetAsync(string name, string url,
            string bucketId, string orgId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(url, nameof(url));
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var scrapperTarget =
                new ScraperTargetRequest(name, ScraperTargetRequest.TypeEnum.Prometheus, url, orgId, bucketId);

            return CreateScraperTargetAsync(scrapperTarget, cancellationToken);
        }

        /// <summary>
        /// Update a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetResponse">ScraperTarget update to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>updated ScraperTarget</returns>
        public Task<ScraperTargetResponse> UpdateScraperTargetAsync(ScraperTargetResponse scraperTargetResponse,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            return UpdateScraperTargetAsync(scraperTargetResponse.Id, scraperTargetResponse, cancellationToken);
        }

        /// <summary>
        /// Update a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">id of the scraper target (required)</param>
        /// <param name="scraperTargetRequest">ScraperTargetRequest update to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>updated ScraperTarget</returns>
        public Task<ScraperTargetResponse> UpdateScraperTargetAsync(string scraperTargetId,
            ScraperTargetRequest scraperTargetRequest, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNotNull(scraperTargetRequest, nameof(scraperTargetRequest));

            return _service.PatchScrapersIDAsync(scraperTargetId, scraperTargetRequest,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Delete a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of ScraperTarget to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>scraper target deleted</returns>
        public Task DeleteScraperTargetAsync(string scraperTargetId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(scraperTargetId, nameof(scraperTargetId));

            return _service.DeleteScrapersIDAsync(scraperTargetId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Delete a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetResponse">ScraperTarget to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>scraper target deleted</returns>
        public Task DeleteScraperTargetAsync(ScraperTargetResponse scraperTargetResponse,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            return DeleteScraperTargetAsync(scraperTargetResponse.Id, cancellationToken);
        }

        /// <summary>
        /// Clone a ScraperTarget.
        /// </summary>
        /// <param name="clonedName">name of cloned ScraperTarget</param>
        /// <param name="scraperTargetId">ID of ScraperTarget to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned ScraperTarget</returns>
        public async Task<ScraperTargetResponse> CloneScraperTargetAsync(string clonedName, string scraperTargetId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            var scraperTarget =
                await FindScraperTargetByIdAsync(scraperTargetId, cancellationToken).ConfigureAwait(false);
            return await CloneScraperTargetAsync(clonedName, scraperTarget, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a ScraperTarget.
        /// </summary>
        /// <param name="clonedName">name of cloned ScraperTarget</param>
        /// <param name="scraperTargetResponse">ScraperTarget to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned ScraperTarget</returns>
        public async Task<ScraperTargetResponse> CloneScraperTargetAsync(string clonedName,
            ScraperTargetResponse scraperTargetResponse, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            var cloned = new ScraperTargetRequest(clonedName, scraperTargetResponse.Type, scraperTargetResponse.Url,
                scraperTargetResponse.OrgID, scraperTargetResponse.BucketID);

            var created = await CreateScraperTargetAsync(cloned, cancellationToken).ConfigureAwait(false);
            var labels = await GetLabelsAsync(scraperTargetResponse, cancellationToken).ConfigureAwait(false);
            foreach (var label in labels) await AddLabelAsync(label, created, cancellationToken).ConfigureAwait(false);

            return created;
        }

        /// <summary>
        /// Retrieve a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of ScraperTarget to get</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ScraperTarget details</returns>
        public Task<ScraperTargetResponse> FindScraperTargetByIdAsync(string scraperTargetId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            return _service.GetScrapersIDAsync(scraperTargetId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Get all ScraperTargets.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of ScraperTargets</returns>
        public async Task<List<ScraperTargetResponse>> FindScraperTargetsAsync(
            CancellationToken cancellationToken = default)
        {
            var response = await _service.GetScrapersAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.Configurations;
        }

        /// <summary>
        /// Get all ScraperTargets.
        /// </summary>
        /// <param name="organization">specifies the organization of the resource</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of ScraperTargets</returns>
        public Task<List<ScraperTargetResponse>> FindScraperTargetsByOrgAsync(Organization organization,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return FindScraperTargetsByOrgIdAsync(organization.Id, cancellationToken);
        }

        /// <summary>
        /// Get all ScraperTargets.
        /// </summary>
        /// <param name="orgId">specifies the organization ID of the resource</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of ScraperTargets</returns>
        public async Task<List<ScraperTargetResponse>> FindScraperTargetsByOrgIdAsync(string orgId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var response = await _service.GetScrapersAsync(null, orgId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Configurations;
        }

        /// <summary>
        /// List all members of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetResponse">ScraperTarget of the members</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all members of a ScraperTarget</returns>
        public Task<List<ResourceMember>> GetMembersAsync(ScraperTargetResponse scraperTargetResponse,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            return GetMembersAsync(scraperTargetResponse.Id, cancellationToken);
        }

        /// <summary>
        /// List all members of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of ScraperTarget to get members</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all members of a ScraperTarget</returns>
        public async Task<List<ResourceMember>> GetMembersAsync(string scraperTargetId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            var response = await _service
                .GetScrapersIDMembersAsync(scraperTargetId, cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.Users;
        }

        /// <summary>
        /// Add a ScraperTarget member.
        /// </summary>
        /// <param name="member">the member of a scraperTarget</param>
        /// <param name="scraperTargetResponse">the ScraperTarget of a member</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created mapping</returns>
        public Task<ResourceMember> AddMemberAsync(User member, ScraperTargetResponse scraperTargetResponse,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(member, nameof(member));

            return AddMemberAsync(member.Id, scraperTargetResponse.Id, cancellationToken);
        }

        /// <summary>
        /// Add a ScraperTarget member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="scraperTargetId">the ID of a scraperTarget</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created mapping</returns>
        public Task<ResourceMember> AddMemberAsync(string memberId, string scraperTargetId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return _service.PostScrapersIDMembersAsync(scraperTargetId,
                new AddResourceMemberRequestBody(memberId), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Removes a member from a ScraperTarget.
        /// </summary>
        /// <param name="member">the member of a ScraperTarget</param>
        /// <param name="scraperTargetResponse">the ScraperTarget of a member</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>async task</returns>
        public Task DeleteMemberAsync(User member, ScraperTargetResponse scraperTargetResponse,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(member, nameof(member));

            return DeleteMemberAsync(member.Id, scraperTargetResponse.Id, cancellationToken);
        }

        /// <summary>
        /// Removes a member from a ScraperTarget.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>async task</returns>
        public Task DeleteMemberAsync(string memberId, string scraperTargetId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return _service.DeleteScrapersIDMembersIDAsync(memberId, scraperTargetId,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// List all owners of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetResponse">ScraperTarget of the owners</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all owners of a ScraperTarget</returns>
        public Task<List<ResourceOwner>> GetOwnersAsync(ScraperTargetResponse scraperTargetResponse,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            return GetOwnersAsync(scraperTargetResponse.Id, cancellationToken);
        }

        /// <summary>
        /// List all owners of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of a ScraperTarget to get owners</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all owners of a scraperTarget</returns>
        public async Task<List<ResourceOwner>> GetOwnersAsync(string scraperTargetId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            var response = await _service
                .GetScrapersIDOwnersAsync(scraperTargetId, cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.Users;
        }

        /// <summary>
        /// Add a ScraperTarget owner.
        /// </summary>
        /// <param name="owner">the owner of a ScraperTarget</param>
        /// <param name="scraperTargetResponse">the ScraperTarget of a owner</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created mapping</returns>
        public Task<ResourceOwner> AddOwnerAsync(User owner, ScraperTargetResponse scraperTargetResponse,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(owner, nameof(owner));

            return AddOwnerAsync(owner.Id, scraperTargetResponse.Id, cancellationToken);
        }

        /// <summary>
        /// Add a ScraperTarget owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created mapping</returns>
        public Task<ResourceOwner> AddOwnerAsync(string ownerId, string scraperTargetId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var memberRequest = new AddResourceMemberRequestBody(ownerId);

            return _service.PostScrapersIDOwnersAsync(scraperTargetId, memberRequest,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Removes a owner from a ScraperTarget.
        /// </summary>
        /// <param name="owner">the owner of a scraperTarget</param>
        /// <param name="scraperTargetResponse">the ScraperTarget of a owner</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>async task</returns>
        public Task DeleteOwnerAsync(User owner, ScraperTargetResponse scraperTargetResponse,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(owner, nameof(owner));

            return DeleteOwnerAsync(owner.Id, scraperTargetResponse.Id, cancellationToken);
        }

        /// <summary>
        /// Removes a owner from a ScraperTarget.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>async task</returns>
        public Task DeleteOwnerAsync(string ownerId, string scraperTargetId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            return _service.DeleteScrapersIDOwnersIDAsync(ownerId, scraperTargetId,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// List all labels of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetResponse">a ScraperTarget of the labels</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all labels of a ScraperTarget</returns>
        public Task<List<Label>> GetLabelsAsync(ScraperTargetResponse scraperTargetResponse,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            return GetLabelsAsync(scraperTargetResponse.Id, cancellationToken);
        }

        /// <summary>
        /// List all labels of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of a ScraperTarget to get labels</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all labels of a ScraperTarget</returns>
        public async Task<List<Label>> GetLabelsAsync(string scraperTargetId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            var response = await _service
                .GetScrapersIDLabelsAsync(scraperTargetId, cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.Labels;
        }

        /// <summary>
        /// Add a ScraperTarget label.
        /// </summary>
        /// <param name="label">the label of a ScraperTarget</param>
        /// <param name="scraperTargetResponse">a ScraperTarget of a label</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>added label</returns>
        public Task<Label> AddLabelAsync(Label label, ScraperTargetResponse scraperTargetResponse,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(label, nameof(label));

            return AddLabelAsync(label.Id, scraperTargetResponse.Id, cancellationToken);
        }

        /// <summary>
        /// Add a ScraperTarget label.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabelAsync(string labelId, string scraperTargetId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var mapping = new LabelMapping(labelId);

            var response = await _service
                .PostScrapersIDLabelsAsync(scraperTargetId, mapping, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Label;
        }

        /// <summary>
        /// Removes a label from a ScraperTarget.
        /// </summary>
        /// <param name="label">the label of a ScraperTarget</param>
        /// <param name="scraperTargetResponse">a ScraperTarget of a owner</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteLabelAsync(Label label, ScraperTargetResponse scraperTargetResponse,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(label, nameof(label));

            return DeleteLabelAsync(label.Id, scraperTargetResponse.Id, cancellationToken);
        }

        /// <summary>
        /// Removes a label from a ScraperTarget.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteLabelAsync(string labelId, string scraperTargetId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return _service.DeleteScrapersIDLabelsIDAsync(scraperTargetId, labelId,
                cancellationToken: cancellationToken);
        }
    }
}