using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client
{
    public class LabelsApi
    {
        private readonly LabelsService _service;

        protected internal LabelsApi(LabelsService service)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
        }

        /// <summary>
        ///     Create a label
        /// </summary>
        /// <param name="request">label to create</param>
        /// <returns>Added label</returns>
        public async Task<Label> CreateLabelAsync(LabelCreateRequest request)
        {
            Arguments.CheckNotNull(request, nameof(request));

            return await _service.PostLabelsAsync(request).ContinueWith(t=> t.Result.Label);
        }

        /// <summary>
        ///     Create a label
        /// </summary>
        /// <param name="name">name of a label</param>
        /// <param name="properties">properties of a label</param>
        /// <param name="orgId">owner of a label</param>
        /// <returns>Added label</returns>
        public async Task<Label> CreateLabelAsync(string name, Dictionary<string, string> properties,
            string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(properties, nameof(properties));

            return await CreateLabelAsync(new LabelCreateRequest(orgId, name, properties));
        }

        /// <summary>
        ///     Update a single label
        /// </summary>
        /// <param name="label">label to update</param>
        /// <returns>Updated label</returns>
        public async Task<Label> UpdateLabelAsync(Label label)
        {
            Arguments.CheckNotNull(label, nameof(label));

            var labelUpdate = new LabelUpdate {Properties = label.Properties};

            return await UpdateLabelAsync(label.Id, labelUpdate);
        }

        /// <summary>
        ///     Update a single label
        /// </summary>
        /// <param name="labelId">ID of label to update</param>
        /// <param name="labelUpdate">label update</param>
        /// <returns>Updated label</returns>
        public async Task<Label> UpdateLabelAsync(string labelId, LabelUpdate labelUpdate)
        {
            return await _service.PatchLabelsIDAsync(labelId, labelUpdate).ContinueWith(t => t.Result.Label);
        }

        /// <summary>
        ///     Delete a label.
        /// </summary>
        /// <param name="label">label to delete</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteLabelAsync(Label label)
        {
            Arguments.CheckNotNull(label, nameof(label));

            await DeleteLabelAsync(label.Id);
        }

        /// <summary>
        ///     Delete a label.
        /// </summary>
        /// <param name="labelId">ID of label to delete</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteLabelAsync(string labelId)
        {
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            await _service.DeleteLabelsIDAsync(labelId);
        }

        /// <summary>
        ///     Clone a label.
        /// </summary>
        /// <param name="clonedName">name of cloned label</param>
        /// <param name="labelId">ID of label to clone</param>
        /// <returns>cloned label</returns>
        public async Task<Label> CloneLabelAsync(string clonedName, string labelId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return await FindLabelByIdAsync(labelId).ContinueWith(t => CloneLabelAsync(clonedName, t.Result)).Unwrap();
        }

        /// <summary>
        ///     Clone a label.
        /// </summary>
        /// <param name="clonedName">name of cloned label</param>
        /// <param name="label">label to clone</param>
        /// <returns>cloned label</returns>
        public async Task<Label> CloneLabelAsync(string clonedName, Label label)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(label, nameof(label));

            var cloned =
                new LabelCreateRequest(label.OrgID, clonedName, new Dictionary<string, string>(label.Properties));

            return await CreateLabelAsync(cloned);
        }

        /// <summary>
        ///     Retrieve a label.
        /// </summary>
        /// <param name="labelId">ID of a label to get</param>
        /// <returns>Label detail</returns>
        public async Task<Label> FindLabelByIdAsync(string labelId)
        {
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return await _service.GetLabelsIDAsync(labelId).ContinueWith(t => t.Result.Label);
        }

        /// <summary>
        ///     List all labels.
        /// </summary>
        /// <returns>List all labels.</returns>
        public async Task<List<Label>> FindLabelsAsync()
        {
            return await _service.GetLabelsAsync().ContinueWith(t => t.Result.Labels);
        }

        /// <summary>
        ///     Get all labels.
        /// </summary>
        /// <param name="organization">specifies the organization of the resource</param>
        /// <returns>all labels</returns>
        public async Task<List<Label>> FindLabelsByOrgAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await FindLabelsByOrgIdAsync(organization.Id);
        }

        /// <summary>
        ///     Get all labels.
        /// </summary>
        /// <param name="orgId">specifies the organization ID of the resource</param>
        /// <returns>all labels</returns>
        public async Task<List<Label>> FindLabelsByOrgIdAsync(string orgId)
        {
            return await _service.GetLabelsAsync(null, orgId).ContinueWith(t => t.Result.Labels);
        }
    }
}