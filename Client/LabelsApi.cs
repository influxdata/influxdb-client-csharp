using System.Collections.Generic;
using System.Threading;
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
        /// Create a label
        /// </summary>
        /// <param name="request">label to create</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Added label</returns>
        public async Task<Label> CreateLabelAsync(LabelCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(request, nameof(request));

            var response = await _service.PostLabelsAsync(request, cancellationToken).ConfigureAwait(false);
            return response.Label;
        }

        /// <summary>
        /// Create a label
        /// </summary>
        /// <param name="name">name of a label</param>
        /// <param name="properties">properties of a label</param>
        /// <param name="orgId">owner of a label</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Added label</returns>
        public Task<Label> CreateLabelAsync(string name, Dictionary<string, string> properties,
            string orgId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(properties, nameof(properties));

            return CreateLabelAsync(new LabelCreateRequest(orgId, name, properties), cancellationToken);
        }

        /// <summary>
        /// Update a single label
        /// </summary>
        /// <param name="label">label to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated label</returns>
        public Task<Label> UpdateLabelAsync(Label label, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(label, nameof(label));

            var labelUpdate = new LabelUpdate { Properties = label.Properties };

            return UpdateLabelAsync(label.Id, labelUpdate, cancellationToken);
        }

        /// <summary>
        /// Update a single label
        /// </summary>
        /// <param name="labelId">ID of label to update</param>
        /// <param name="labelUpdate">label update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated label</returns>
        public async Task<Label> UpdateLabelAsync(string labelId, LabelUpdate labelUpdate,
            CancellationToken cancellationToken = default)
        {
            var response = await _service.PatchLabelsIDAsync(labelId, labelUpdate, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Label;
        }

        /// <summary>
        /// Delete a label.
        /// </summary>
        /// <param name="label">label to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteLabelAsync(Label label, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(label, nameof(label));

            return DeleteLabelAsync(label.Id, cancellationToken);
        }

        /// <summary>
        /// Delete a label.
        /// </summary>
        /// <param name="labelId">ID of label to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteLabelAsync(string labelId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return _service.DeleteLabelsIDAsync(labelId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Clone a label.
        /// </summary>
        /// <param name="clonedName">name of cloned label</param>
        /// <param name="labelId">ID of label to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned label</returns>
        public async Task<Label> CloneLabelAsync(string clonedName, string labelId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var label = await FindLabelByIdAsync(labelId).ConfigureAwait(false);

            return await CloneLabelAsync(clonedName, label, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a label.
        /// </summary>
        /// <param name="clonedName">name of cloned label</param>
        /// <param name="label">label to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned label</returns>
        public Task<Label> CloneLabelAsync(string clonedName, Label label,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(label, nameof(label));

            var cloned =
                new LabelCreateRequest(label.OrgID, clonedName, new Dictionary<string, string>(label.Properties));

            return CreateLabelAsync(cloned, cancellationToken);
        }

        /// <summary>
        /// Retrieve a label.
        /// </summary>
        /// <param name="labelId">ID of a label to get</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Label detail</returns>
        public async Task<Label> FindLabelByIdAsync(string labelId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var response = await _service.GetLabelsIDAsync(labelId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Label;
        }

        /// <summary>
        /// List all labels.
        /// <param name="cancellationToken">Cancellation token</param>
        /// </summary>
        /// <returns>List all labels.</returns>
        public async Task<List<Label>> FindLabelsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _service.GetLabelsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.Labels;
        }

        /// <summary>
        /// Get all labels.
        /// </summary>
        /// <param name="organization">specifies the organization of the resource</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>all labels</returns>
        public Task<List<Label>> FindLabelsByOrgAsync(Organization organization,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return FindLabelsByOrgIdAsync(organization.Id, cancellationToken);
        }

        /// <summary>
        /// Get all labels.
        /// </summary>
        /// <param name="orgId">specifies the organization ID of the resource</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>all labels</returns>
        public async Task<List<Label>> FindLabelsByOrgIdAsync(string orgId,
            CancellationToken cancellationToken = default)
        {
            var response = await _service.GetLabelsAsync(null, orgId, cancellationToken).ConfigureAwait(false);
            return response.Labels;
        }
    }
}