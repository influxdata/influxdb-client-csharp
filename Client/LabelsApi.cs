using System;
using System.Collections.Generic;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Internal;
using InfluxDB.Client.Generated.Domain;
using InfluxDB.Client.Generated.Service;

namespace InfluxDB.Client
{
    public class LabelsApi : AbstractClient
    {
        private readonly LabelsService _service;

        protected internal LabelsApi(DefaultClientIo client, LabelsService service) : base(client)
        {
            _service = service;
        }

        /// <summary>
        ///     Create a label
        /// </summary>
        /// <param name="request">label to create</param>
        /// <returns>Added label</returns>
        public Label CreateLabel(LabelCreateRequest request)
        {
            Arguments.CheckNotNull(request, nameof(request));

            return _service.LabelsPost(request).Label;
        }

        /// <summary>
        ///     Create a label
        /// </summary>
        /// <param name="name">name of a label</param>
        /// <param name="properties">properties of a label</param>
        /// <param name="orgId">owner of a label</param>
        /// <returns>Added label</returns>
        public Label CreateLabel(string name, Dictionary<string, string> properties,
            string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(properties, nameof(properties));

            return CreateLabel(new LabelCreateRequest(orgId, name, properties));
        }

        /// <summary>
        ///     Update a single label
        /// </summary>
        /// <param name="label">label to update</param>
        /// <returns>Updated label</returns>
        public Label UpdateLabel(Label label)
        {
            Arguments.CheckNotNull(label, nameof(label));

            var labelUpdate = new LabelUpdate {Properties = label.Properties};

            return _service.LabelsLabelIDPatch(label.Id, labelUpdate).Label;
        }

        /// <summary>
        ///     Update a single label
        /// </summary>
        /// <param name="labelId">ID of label to update</param>
        /// <param name="labelUpdate">label update</param>
        /// <returns>Updated label</returns>
        public Label UpdateLabel(string labelId, LabelUpdate labelUpdate)
        {
            return _service.LabelsLabelIDPatch(labelId, labelUpdate).Label;
        }

        /// <summary>
        ///     Delete a label.
        /// </summary>
        /// <param name="label">label to delete</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteLabel(Label label)
        {
            Arguments.CheckNotNull(label, nameof(label));

            DeleteLabel(label.Id);
        }

        /// <summary>
        ///     Delete a label.
        /// </summary>
        /// <param name="labelId">ID of label to delete</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteLabel(string labelId)
        {
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            _service.LabelsLabelIDDelete(labelId);
        }

        /// <summary>
        ///     Clone a label.
        /// </summary>
        /// <param name="clonedName">name of cloned label</param>
        /// <param name="labelId">ID of label to clone</param>
        /// <returns>cloned label</returns>
        public Label CloneLabel(string clonedName, string labelId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var label = FindLabelById(labelId);
            if (label == null) throw new InvalidOperationException($"NotFound Label with ID: {labelId}");

            return CloneLabel(clonedName, label);
        }

        /// <summary>
        ///     Clone a label.
        /// </summary>
        /// <param name="clonedName">name of cloned label</param>
        /// <param name="label">label to clone</param>
        /// <returns>cloned label</returns>
        public Label CloneLabel(string clonedName, Label label)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(label, nameof(label));

            var cloned =
                new LabelCreateRequest(label.OrgID, clonedName, new Dictionary<string, string>(label.Properties));

            return CreateLabel(cloned);
        }

        /// <summary>
        ///     Retrieve a label.
        /// </summary>
        /// <param name="labelId">ID of a label to get</param>
        /// <returns>Label detail</returns>
        public Label FindLabelById(string labelId)
        {
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return _service.LabelsLabelIDGet(labelId).Label;
        }

        /// <summary>
        ///     List all labels.
        /// </summary>
        /// <returns>List all labels.</returns>
        public Labels FindLabels()
        {
            return _service.LabelsGet().Labels;
        }

        /// <summary>
        ///     Get all labels.
        /// </summary>
        /// <param name="organization">specifies the organization of the resource</param>
        /// <returns>all labels</returns>
        public Labels FindLabelsByOrg(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return FindLabelsByOrgId(organization.Id);
        }

        /// <summary>
        ///     Get all labels.
        /// </summary>
        /// <param name="orgId">specifies the organization ID of the resource</param>
        /// <returns>all labels</returns>
        public Labels FindLabelsByOrgId(string orgId)
        {
            return _service.LabelsGet(null, orgId).Labels;
        }
    }
}