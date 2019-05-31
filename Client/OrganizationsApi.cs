using System.Collections.Generic;
using InfluxDB.Client.Core;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using ResourceMember = InfluxDB.Client.Api.Domain.ResourceMember;

namespace InfluxDB.Client
{
    public class OrganizationsApi
    {
        private readonly OrganizationsService _service;

        protected internal OrganizationsApi(OrganizationsService service)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
        }

        /// <summary>
        /// Creates a new organization and sets <see cref="InfluxDB.Client.Api.Domain.Organization.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Created organization</returns>
        public Organization CreateOrganization(string name)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));

            var organization = new Organization(null, name);

            return CreateOrganization(organization);
        }

        /// <summary>
        /// Creates a new organization and sets <see cref="Organization.Id" /> with the new identifier.
        /// </summary>
        /// <param name="organization">the organization to create</param>
        /// <returns>created organization</returns>
        public Organization CreateOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return _service.PostOrgs(organization);
        }

        /// <summary>
        /// Update an organization.
        /// </summary>
        /// <param name="organization">organization update to apply</param>
        /// <returns>updated organization</returns>
        public Organization UpdateOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return _service.PatchOrgsID(organization.Id, organization);
        }

        /// <summary>
        /// Delete an organization.
        /// </summary>
        /// <param name="orgId">ID of organization to delete</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteOrganization(string orgId)
        {
            Arguments.CheckNotNull(orgId, nameof(orgId));

            _service.DeleteOrgsID(orgId);
        }

        /// <summary>
        /// Delete an organization.
        /// </summary>
        /// <param name="organization">organization to delete</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            DeleteOrganization(organization.Id);
        }

        /// <summary>
        /// Clone an organization.
        /// </summary>
        /// <param name="clonedName">name of cloned organization</param>
        /// <param name="bucketId">ID of organization to clone</param>
        /// <returns>cloned organization</returns>
        public Organization CloneOrganization(string clonedName, string bucketId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            var organization = FindOrganizationById(bucketId);

            return CloneOrganization(clonedName, organization);
        }

        /// <summary>
        /// Clone an organization.
        /// </summary>
        /// <param name="clonedName">name of cloned organization</param>
        /// <param name="organization">organization to clone</param>
        /// <returns>cloned organization</returns>
        public Organization CloneOrganization(string clonedName, Organization organization)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(organization, nameof(organization));

            var cloned = new Organization(null, clonedName);

            var created = CreateOrganization(cloned);

            foreach (var label in GetLabels(organization)) AddLabel(label, created);

            return created;
        }

        /// <summary>
        /// Retrieve an organization.
        /// </summary>
        /// <param name="orgId">ID of organization to get</param>
        /// <returns>organization details</returns>
        public Organization FindOrganizationById(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return _service.GetOrgsID(orgId);
        }

        /// <summary>
        /// List all organizations.
        /// </summary>
        /// <returns>List all organizations</returns>
        public List<Organization> FindOrganizations()
        {
            return _service.GetOrgs().Orgs;
        }

        /// <summary>
        /// List of secret keys the are stored for Organization. For example:
        /// <code>
        /// github_api_key,
        /// some_other_key,
        /// a_secret_key
        /// </code>
        /// </summary>
        /// <param name="organization">the organization for get secrets</param>
        /// <returns>the secret keys</returns>
        public List<string> GetSecrets(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return GetSecrets(organization.Id);
        }

        /// <summary>
        /// List of secret keys the are stored for Organization. For example:
        /// <code>
        /// github_api_key,
        /// some_other_key,
        /// a_secret_key
        /// </code>
        /// </summary>
        /// <param name="orgId">the organization for get secrets</param>
        /// <returns>the secret keys</returns>
        public List<string> GetSecrets(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return _service.GetOrgsIDSecrets(orgId).Secrets;
        }

        /// <summary>
        /// Patches all provided secrets and updates any previous values.
        /// </summary>
        /// <param name="secrets">secrets to update/add</param>
        /// <param name="organization">the organization for put secrets</param>
        /// <returns></returns>
        public void PutSecrets(Dictionary<string, string> secrets, Organization organization)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNotNull(organization, nameof(organization));

            PutSecrets(secrets, organization.Id);
        }

        /// <summary>
        /// Patches all provided secrets and updates any previous values.
        /// </summary>
        /// <param name="secrets">secrets to update/add</param>
        /// <param name="orgId">the organization for put secrets</param>
        /// <returns></returns>
        public void PutSecrets(Dictionary<string, string> secrets, string orgId)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            _service.PatchOrgsIDSecrets(orgId, secrets);
        }

        /// <summary>
        /// Delete provided secrets.
        /// </summary>
        /// <param name="secrets">secrets to delete</param>
        /// <param name="organization">the organization for delete secrets</param>
        /// <returns>keys successfully patched</returns>
        public void DeleteSecrets(List<string> secrets, Organization organization)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNotNull(organization, nameof(organization));

            DeleteSecrets(secrets, organization.Id);
        }

        /// <summary>
        /// Delete provided secrets.
        /// </summary>
        /// <param name="secrets">secrets to delete</param>
        /// <param name="orgId">the organization for delete secrets</param>
        /// <returns>keys successfully patched</returns>
        public void DeleteSecrets(List<string> secrets, string orgId)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            DeleteSecrets(new SecretKeys(secrets), orgId);
        }
        
        /// <summary>
        /// Delete provided secrets.
        /// </summary>
        /// <param name="secrets">secrets to delete</param>
        /// <param name="orgId">the organization for delete secrets</param>
        /// <returns>keys successfully patched</returns>
        public void DeleteSecrets(SecretKeys secrets, string orgId)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            _service.PostOrgsIDSecrets(orgId, secrets);
        }

        /// <summary>
        /// List all members of an organization.
        /// </summary>
        /// <param name="organization">organization of the members</param>
        /// <returns>the List all members of an organization</returns>
        public List<ResourceMember> GetMembers(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return GetMembers(organization.Id);
        }

        /// <summary>
        /// List all members of an organization.
        /// </summary>
        /// <param name="orgId">ID of organization to get members</param>
        /// <returns>the List all members of an organization</returns>
        public List<ResourceMember> GetMembers(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return _service.GetOrgsIDMembers(orgId).Users;
        }

        /// <summary>
        /// Add organization member.
        /// </summary>
        /// <param name="member">the member of an organization</param>
        /// <param name="organization">the organization of a member</param>
        /// <returns>created mapping</returns>
        public ResourceMember AddMember(User member, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(member, nameof(member));

            return AddMember(member.Id, organization.Id);
        }

        /// <summary>
        /// Add organization member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns>created mapping</returns>
        public ResourceMember AddMember(string memberId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return _service.PostOrgsIDMembers(orgId, new AddResourceMemberRequestBody(memberId));
        }

        /// <summary>
        /// Removes a member from an organization.
        /// </summary>
        /// <param name="member">the member of an organization</param>
        /// <param name="organization">the organization of a member</param>
        /// <returns></returns>
        public void DeleteMember(User member, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(member, nameof(member));

            DeleteMember(member.Id, organization.Id);
        }

        /// <summary>
        /// Removes a member from an organization.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns></returns>
        public void DeleteMember(string memberId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            _service.DeleteOrgsIDMembersID(memberId, orgId);
        }

        /// <summary>
        /// List all owners of an organization.
        /// </summary>
        /// <param name="organization">organization of the owners</param>
        /// <returns>the List all owners of an organization</returns>
        public List<ResourceOwner> GetOwners(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return GetOwners(organization.Id);
        }

        /// <summary>
        /// List all owners of an organization.
        /// </summary>
        /// <param name="orgId">ID of organization to get owners</param>
        /// <returns>the List all owners of an organization</returns>
        public List<ResourceOwner> GetOwners(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return _service.GetOrgsIDOwners(orgId).Users;
        }

        /// <summary>
        /// Add organization owner.
        /// </summary>
        /// <param name="owner">the owner of an organization</param>
        /// <param name="organization">the organization of a owner</param>
        /// <returns>created mapping</returns>
        public ResourceOwner AddOwner(User owner, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(owner, nameof(owner));

            return AddOwner(owner.Id, organization.Id);
        }

        /// <summary>
        /// Add organization owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns>created mapping</returns>
        public ResourceOwner AddOwner(string ownerId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            return _service.PostOrgsIDOwners(orgId, new AddResourceMemberRequestBody(ownerId));
        }

        /// <summary>
        /// Removes a owner from an organization.
        /// </summary>
        /// <param name="owner">the owner of an organization</param>
        /// <param name="organization">the organization of a owner</param>
        /// <returns></returns>
        public void DeleteOwner(User owner, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(owner, nameof(owner));

            DeleteOwner(owner.Id, organization.Id);
        }

        /// <summary>
        /// Removes a owner from an organization.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns></returns>
        public void DeleteOwner(string ownerId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            _service.DeleteOrgsIDOwnersID(ownerId, orgId);
        }

        /// <summary>
        /// Retrieve an organization's logs
        /// </summary>
        /// <param name="organization">for retrieve logs</param>
        /// <returns>logs</returns>
        public List<OperationLog> FindOrganizationLogs(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return FindOrganizationLogs(organization.Id);
        }

        /// <summary>
        /// Retrieve an organization's logs
        /// </summary>
        /// <param name="organization">for retrieve logs</param>
        /// <param name="findOptions">the find options</param>
        /// <returns>logs</returns>
        public OperationLogs FindOrganizationLogs(Organization organization, FindOptions findOptions)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return FindOrganizationLogs(organization.Id, findOptions);
        }

        /// <summary>
        /// Retrieve an organization's logs
        /// </summary>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns>logs</returns>
        public List<OperationLog> FindOrganizationLogs(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return FindOrganizationLogs(orgId, new FindOptions()).Logs;
        }

        /// <summary>
        /// Retrieve an organization's logs
        /// </summary>
        /// <param name="orgId">the ID of an organization</param>
        /// <param name="findOptions">the find options</param>
        /// <returns>logs</returns>
        public OperationLogs FindOrganizationLogs(string orgId, FindOptions findOptions)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return _service.GetOrgsIDLogs(orgId, null, findOptions.Offset, findOptions.Limit);
        }

        /// <summary>
        /// List all labels of an organization.
        /// </summary>
        /// <param name="organization">organization of the labels</param>
        /// <returns>the List all labels of an organization</returns>
        public List<Label> GetLabels(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return GetLabels(organization.Id);
        }

        /// <summary>
        /// List all labels of an organization.
        /// </summary>
        /// <param name="orgId">ID of an organization to get labels</param>
        /// <returns>the List all labels of an organization</returns>
        public List<Label> GetLabels(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return _service.GetOrgsIDLabels(orgId).Labels;
        }

        /// <summary>
        /// Add an organization label.
        /// </summary>
        /// <param name="label">the label of an organization</param>
        /// <param name="organization">an organization of a label</param>
        /// <returns>added label</returns>
        public Label AddLabel(Label label, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(label, nameof(label));

            return AddLabel(label.Id, organization.Id);
        }

        /// <summary>
        /// Add an organization label.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns>added label</returns>
        public Label AddLabel(string labelId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var mapping = new LabelMapping(labelId);
            
            return _service.PostOrgsIDLabels(orgId, mapping).Label;
        }

        /// <summary>
        /// Removes a label from an organization.
        /// </summary>
        /// <param name="label">the label of an organization</param>
        /// <param name="organization">an organization of a owner</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteLabel(Label label, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(label, nameof(label));

            DeleteLabel(label.Id, organization.Id);
        }

        /// <summary>
        /// Removes a label from an organization.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteLabel(string labelId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            _service.DeleteOrgsIDLabelsID(orgId, labelId);
        }
    }
}