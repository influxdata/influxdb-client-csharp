using System.Collections.Generic;
using InfluxDB.Client.Core;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;

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
        public ScraperTargetResponse CreateScraperTarget(ScraperTargetRequest scraperTargetRequest)
        {
            Arguments.CheckNotNull(scraperTargetRequest, nameof(scraperTargetRequest));

            return _service.ScrapersPost(scraperTargetRequest);
        }

        /// <summary>
        /// Creates a new ScraperTarget and sets <see cref="ScraperTargetResponse.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">the name of the new ScraperTarget</param>
        /// <param name="url">the url of the new ScraperTarget</param>
        /// <param name="bucketId">the id of the bucket that its use to writes</param>
        /// <param name="orgId">the id of the organization that owns new ScraperTarget</param>
        /// <returns>created ScraperTarget</returns>
        public ScraperTargetResponse CreateScraperTarget(string name, string url,
            string bucketId, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(url, nameof(url));
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var scrapperTarget =
                new ScraperTargetRequest(name, ScraperTargetRequest.TypeEnum.Prometheus, url, orgId, bucketId);

            return CreateScraperTarget(scrapperTarget);
        }

        /// <summary>
        /// Update a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetResponse">ScraperTarget update to apply</param>
        /// <returns>updated ScraperTarget</returns>
        public ScraperTargetResponse UpdateScraperTarget(ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            return UpdateScraperTarget(scraperTargetResponse.Id, scraperTargetResponse);
        }
        
        /// <summary>
        /// Update a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">id of the scraper target (required)</param>
        /// <param name="scraperTargetRequest">ScraperTargetRequest update to apply</param>
        /// <returns>updated ScraperTarget</returns>
        public ScraperTargetResponse UpdateScraperTarget(string scraperTargetId, ScraperTargetRequest scraperTargetRequest)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNotNull(scraperTargetRequest, nameof(scraperTargetRequest));

            return _service.ScrapersScraperTargetIDPatch(scraperTargetId, scraperTargetRequest);
        }

        /// <summary>
        /// Delete a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of ScraperTarget to delete</param>
        /// <returns>scraper target deleted</returns>
        public void DeleteScraperTarget(string scraperTargetId)
        {
            Arguments.CheckNotNull(scraperTargetId, nameof(scraperTargetId));

            _service.ScrapersScraperTargetIDDelete(scraperTargetId);
        }

        /// <summary>
        /// Delete a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetResponse">ScraperTarget to delete</param>
        /// <returns>scraper target deleted</returns>
        public void DeleteScraperTarget(ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            DeleteScraperTarget(scraperTargetResponse.Id);
        }

        /// <summary>
        /// Clone a ScraperTarget.
        /// </summary>
        /// <param name="clonedName">name of cloned ScraperTarget</param>
        /// <param name="scraperTargetId">ID of ScraperTarget to clone</param>
        /// <returns>cloned ScraperTarget</returns>
        public ScraperTargetResponse CloneScraperTarget(string clonedName, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            var scraperTarget = FindScraperTargetById(scraperTargetId);

            return CloneScraperTarget(clonedName, scraperTarget);
        }

        /// <summary>
        /// Clone a ScraperTarget.
        /// </summary>
        /// <param name="clonedName">name of cloned ScraperTarget</param>
        /// <param name="scraperTargetResponse">ScraperTarget to clone</param>
        /// <returns>cloned ScraperTarget</returns>
        public ScraperTargetResponse CloneScraperTarget(string clonedName, ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            var cloned = new ScraperTargetRequest(clonedName, scraperTargetResponse.Type, scraperTargetResponse.Url,
                scraperTargetResponse.OrgID, scraperTargetResponse.BucketID);

            var created = CreateScraperTarget(cloned);

            foreach (var label in GetLabels(scraperTargetResponse)) AddLabel(label, created);

            return created;
        }

        /// <summary>
        /// Retrieve a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of ScraperTarget to get</param>
        /// <returns>ScraperTarget details</returns>
        public ScraperTargetResponse FindScraperTargetById(string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            return _service.ScrapersScraperTargetIDGet(scraperTargetId);
        }

        /// <summary>
        /// Get all ScraperTargets.
        /// </summary>
        /// <returns>A list of ScraperTargets</returns>
        public List<ScraperTargetResponse> FindScraperTargets()
        {
            return _service.ScrapersGet().Configurations;
        }
        
        /// <summary>
        /// Get all ScraperTargets.
        /// </summary>
        /// <param name="organization">specifies the organization of the resource</param>
        /// <returns>A list of ScraperTargets</returns>
        public List<ScraperTargetResponse> FindScraperTargetsByOrg(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            
            return FindScraperTargetsByOrgId(organization.Id);
        }
        
        /// <summary>
        /// Get all ScraperTargets.
        /// </summary>
        /// <param name="orgId">specifies the organization ID of the resource</param>
        /// <returns>A list of ScraperTargets</returns>
        public List<ScraperTargetResponse> FindScraperTargetsByOrgId(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            
            return _service.ScrapersGet(null, orgId).Configurations;
        }

        /// <summary>
        /// List all members of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetResponse">ScraperTarget of the members</param>
        /// <returns>the List all members of a ScraperTarget</returns>
        public List<ResourceMember> GetMembers(ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            return GetMembers(scraperTargetResponse.Id);
        }

        /// <summary>
        /// List all members of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of ScraperTarget to get members</param>
        /// <returns>the List all members of a ScraperTarget</returns>
        public List<ResourceMember> GetMembers(string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            return _service.ScrapersScraperTargetIDMembersGet(scraperTargetId).Users;
        }

        /// <summary>
        /// Add a ScraperTarget member.
        /// </summary>
        /// <param name="member">the member of a scraperTarget</param>
        /// <param name="scraperTargetResponse">the ScraperTarget of a member</param>
        /// <returns>created mapping</returns>
        public ResourceMember AddMember(User member, ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(member, nameof(member));

            return AddMember(member.Id, scraperTargetResponse.Id);
        }

        /// <summary>
        /// Add a ScraperTarget member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="scraperTargetId">the ID of a scraperTarget</param>
        /// <returns>created mapping</returns>
        public ResourceMember AddMember(string memberId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return _service.ScrapersScraperTargetIDMembersPost(scraperTargetId, new AddResourceMemberRequestBody(memberId));
        }

        /// <summary>
        /// Removes a member from a ScraperTarget.
        /// </summary>
        /// <param name="member">the member of a ScraperTarget</param>
        /// <param name="scraperTargetResponse">the ScraperTarget of a member</param>
        /// <returns>async task</returns>
        public void DeleteMember(User member, ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(member, nameof(member));

            DeleteMember(member.Id, scraperTargetResponse.Id);
        }

        /// <summary>
        /// Removes a member from a ScraperTarget.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <returns>async task</returns>
        public void DeleteMember(string memberId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            _service.ScrapersScraperTargetIDMembersUserIDDelete(memberId, scraperTargetId);
        }

        /// <summary>
        /// List all owners of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetResponse">ScraperTarget of the owners</param>
        /// <returns>the List all owners of a ScraperTarget</returns>
        public List<ResourceOwner> GetOwners(ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            return GetOwners(scraperTargetResponse.Id);
        }

        /// <summary>
        /// List all owners of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of a ScraperTarget to get owners</param>
        /// <returns>the List all owners of a scraperTarget</returns>
        public List<ResourceOwner> GetOwners(string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            return _service.ScrapersScraperTargetIDOwnersGet(scraperTargetId).Users;
        }

        /// <summary>
        /// Add a ScraperTarget owner.
        /// </summary>
        /// <param name="owner">the owner of a ScraperTarget</param>
        /// <param name="scraperTargetResponse">the ScraperTarget of a owner</param>
        /// <returns>created mapping</returns>
        public ResourceOwner AddOwner(User owner, ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(owner, nameof(owner));

            return AddOwner(owner.Id, scraperTargetResponse.Id);
        }

        /// <summary>
        /// Add a ScraperTarget owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <returns>created mapping</returns>
        public ResourceOwner AddOwner(string ownerId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var memberRequest = new AddResourceMemberRequestBody(ownerId);
            
            return _service.ScrapersScraperTargetIDOwnersPost(scraperTargetId, memberRequest);
        }

        /// <summary>
        /// Removes a owner from a ScraperTarget.
        /// </summary>
        /// <param name="owner">the owner of a scraperTarget</param>
        /// <param name="scraperTargetResponse">the ScraperTarget of a owner</param>
        /// <returns>async task</returns>
        public void DeleteOwner(User owner, ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(owner, nameof(owner));

            DeleteOwner(owner.Id, scraperTargetResponse.Id);
        }

        /// <summary>
        /// Removes a owner from a ScraperTarget.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <returns>async task</returns>
        public void DeleteOwner(string ownerId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            _service.ScrapersScraperTargetIDOwnersUserIDDelete(ownerId, scraperTargetId);
        }

        /// <summary>
        /// List all labels of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetResponse">a ScraperTarget of the labels</param>
        /// <returns>the List all labels of a ScraperTarget</returns>
        public List<Label> GetLabels(ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));

            return GetLabels(scraperTargetResponse.Id);
        }

        /// <summary>
        /// List all labels of a ScraperTarget.
        /// </summary>
        /// <param name="scraperTargetId">ID of a ScraperTarget to get labels</param>
        /// <returns>the List all labels of a ScraperTarget</returns>
        public List<Label> GetLabels(string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));

            return _service.ScrapersScraperTargetIDLabelsGet(scraperTargetId).Labels;
        }

        /// <summary>
        /// Add a ScraperTarget label.
        /// </summary>
        /// <param name="label">the label of a ScraperTarget</param>
        /// <param name="scraperTargetResponse">a ScraperTarget of a label</param>
        /// <returns>added label</returns>
        public Label AddLabel(Label label, ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(label, nameof(label));

            return AddLabel(label.Id, scraperTargetResponse.Id);
        }

        /// <summary>
        /// Add a ScraperTarget label.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <returns>added label</returns>
        public Label AddLabel(string labelId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var mapping = new LabelMapping(labelId);
            
            return _service.ScrapersScraperTargetIDLabelsPost(scraperTargetId, mapping).Label;
        }

        /// <summary>
        /// Removes a label from a ScraperTarget.
        /// </summary>
        /// <param name="label">the label of a ScraperTarget</param>
        /// <param name="scraperTargetResponse">a ScraperTarget of a owner</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteLabel(Label label, ScraperTargetResponse scraperTargetResponse)
        {
            Arguments.CheckNotNull(scraperTargetResponse, nameof(scraperTargetResponse));
            Arguments.CheckNotNull(label, nameof(label));

            DeleteLabel(label.Id, scraperTargetResponse.Id);
        }

        /// <summary>
        /// Removes a label from a ScraperTarget.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="scraperTargetId">the ID of a ScraperTarget</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteLabel(string labelId, string scraperTargetId)
        {
            Arguments.CheckNonEmptyString(scraperTargetId, nameof(scraperTargetId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            _service.ScrapersScraperTargetIDLabelsLabelIDDelete(scraperTargetId, labelId);
        }
    }
}