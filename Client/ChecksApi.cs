using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using InfluxDB.Client.Domain;

namespace InfluxDB.Client
{
    /// <summary>
    /// The client of the InfluxDB 2.0 that implement Check Api.
    /// </summary>
    public class ChecksApi
    {
        private readonly ChecksService _service;

        protected internal ChecksApi(ChecksService service)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
        }

        /// <summary>
        /// Add new Threshold check.
        /// </summary>
        /// <param name="name">the check name</param>
        /// <param name="query">The text of the flux query</param>
        /// <param name="every">Check repetition interval</param>
        /// <param name="messageTemplate">template that is used to generate and write a status message</param>
        /// <param name="threshold">condition for that specific status</param>
        /// <param name="orgId">the organization that owns this check</param>
        /// <returns>ThresholdCheck created</returns>
        public Task<ThresholdCheck> CreateThresholdCheckAsync(string name, string query,
            string every, string messageTemplate, Threshold threshold, string orgId)
        {
            Arguments.CheckNotNull(threshold, nameof(threshold));

            var thresholds = new List<Threshold> {threshold};

            return CreateThresholdCheckAsync(name, query, every, messageTemplate, thresholds, orgId);
        }

        /// <summary>
        /// Add new Threshold check.
        /// </summary>
        /// <param name="name">the check name</param>
        /// <param name="query">The text of the flux query</param>
        /// <param name="every">Check repetition interval</param>
        /// <param name="messageTemplate">template that is used to generate and write a status message</param>
        /// <param name="thresholds">conditions for that specific status</param>
        /// <param name="orgId">the organization that owns this check</param>
        /// <returns>ThresholdCheck created</returns>
        public async Task<ThresholdCheck> CreateThresholdCheckAsync(string name, string query,
            string every,
            string messageTemplate, List<Threshold> thresholds, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(every, nameof(every));
            Arguments.CheckNonEmptyString(messageTemplate, nameof(messageTemplate));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(thresholds, nameof(thresholds));

            var check = new ThresholdCheck(name: name, type: ThresholdCheck.TypeEnum.Threshold, thresholds: thresholds,
                orgID: orgId, every: every, statusMessageTemplate: messageTemplate, status: TaskStatusType.Active,
                query: CreateDashboardQuery(query));

            return (ThresholdCheck) await CreateCheckAsync(check);
        }

        /// <summary>
        /// Add new Deadman check.
        /// </summary>
        /// <param name="name">the check name</param>
        /// <param name="query">The text of the flux query</param>
        /// <param name="every">Check repetition interval</param>
        /// <param name="timeSince">string duration before deadman triggers</param>
        /// <param name="staleTime">string duration for time that a series is considered stale and should not trigger deadman</param>
        /// <param name="messageTemplate">template that is used to generate and write a status message</param>
        /// <param name="level">the state to record if check matches a criteria</param>
        /// <param name="orgId">the organization that owns this check</param>
        /// <returns>DeadmanCheck created</returns>
        public async Task<DeadmanCheck> CreateDeadmanCheckAsync(string name, string query, string every,
            string timeSince, string staleTime, string messageTemplate, CheckStatusLevel level, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(every, nameof(every));
            Arguments.CheckNonEmptyString(timeSince, nameof(timeSince));
            Arguments.CheckNonEmptyString(staleTime, nameof(staleTime));
            Arguments.CheckNonEmptyString(messageTemplate, nameof(messageTemplate));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(level, nameof(level));

            var check = new DeadmanCheck(level: level, staleTime: staleTime, timeSince: timeSince, name: name,
                every: every, type: DeadmanCheck.TypeEnum.Deadman,
                orgID: orgId, query: CreateDashboardQuery(query), statusMessageTemplate: messageTemplate,
                status: TaskStatusType.Active);

            return (DeadmanCheck) await CreateCheckAsync(check);
        }

        /// <summary>
        /// Add new check.
        /// </summary>
        /// <param name="check">check to create</param>
        /// <returns>Check created</returns>
        public Task<Check> CreateCheckAsync(Check check)
        {
            return _service.CreateCheckAsync(check);
        }

        /// <summary>
        /// Update a check.
        /// </summary>
        /// <param name="check">check update to apply</param>
        /// <returns>An updated check</returns>
        public Task<Check> UpdateCheckAsync(Check check)
        {
            Arguments.CheckNotNull(check, nameof(check));

            Enum.TryParse(check.Status.ToString(), true,
                out CheckPatch.StatusEnum status);

            return UpdateCheckAsync(check.Id,
                new CheckPatch(check.Name, check.Description, status));
        }

        /// <summary>
        /// Update a check.
        /// </summary>
        /// <param name="checkId">ID of check</param>
        /// <param name="patch">update to apply</param>
        /// <returns>An updated check</returns>
        public Task<Check> UpdateCheckAsync(string checkId, CheckPatch patch)
        {
            Arguments.CheckNonEmptyString(checkId, nameof(checkId));
            Arguments.CheckNotNull(patch, nameof(patch));

            return _service.PatchChecksIDAsync(checkId, patch);
        }

        /// <summary>
        /// Delete a check.
        /// </summary>
        /// <param name="check">the check to delete</param>
        /// <returns></returns>
        public Task DeleteCheckAsync(Check check)
        {
            Arguments.CheckNotNull(check, nameof(check));

            return DeleteCheckAsync(check.Id);
        }

        /// <summary>
        /// Delete a check.
        /// </summary>
        /// <param name="checkId">checkID the ID of check to delete</param>
        /// <returns></returns>
        public Task DeleteCheckAsync(string checkId)
        {
            Arguments.CheckNonEmptyString(checkId, nameof(checkId));

            return _service.DeleteChecksIDAsync(checkId);
        }

        /// <summary>
        /// Get a check.
        /// </summary>
        /// <param name="checkId">ID of check</param>
        /// <returns>the check requested</returns>
        public Task<Check> FindCheckByIdAsync(string checkId)
        {
            Arguments.CheckNonEmptyString(checkId, nameof(checkId));

            return _service.GetChecksIDAsync(checkId);
        }

        /// <summary>
        /// Get checks.
        /// </summary>
        /// <param name="orgId">only show checks belonging to specified organization</param>
        /// <returns>A list of checks</returns>
        public async Task<List<Check>> FindChecksAsync(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var checks = await FindChecksAsync(orgId, new FindOptions());
            return checks._Checks;
        }

        /// <summary>
        /// Get all checks.
        /// </summary>
        /// <param name="orgId">only show checks belonging to specified organization</param>
        /// <param name="findOptions">find options</param>
        /// <returns>A list of checks</returns>
        public Task<Checks> FindChecksAsync(string orgId, FindOptions findOptions)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return _service.GetChecksAsync(orgId, offset: findOptions.Offset,
                limit: findOptions.Limit);
        }

        /// <summary>
        /// List all labels for a check.
        /// </summary>
        /// <param name="check"> the check</param>
        /// <returns>a list of all labels for a check</returns>
        public Task<List<Label>> GetLabelsAsync(Check check)
        {
            Arguments.CheckNotNull(check, nameof(check));

            return GetLabelsAsync(check.Id);
        }

        /// <summary>
        /// List all labels for a check.
        /// </summary>
        /// <param name="checkId">ID of the check</param>
        /// <returns>a list of all labels for a check</returns>
        public async Task<List<Label>> GetLabelsAsync(string checkId)
        {
            Arguments.CheckNonEmptyString(checkId, nameof(checkId));

            var labels = await _service.GetChecksIDLabelsAsync(checkId);
            return labels.Labels;
        }

        /// <summary>
        /// Add a label to a check.
        /// </summary>
        /// <param name="label">label to add</param>
        /// <param name="check">the check</param>
        /// <returns>the label was added to the check</returns>
        public Task<Label> AddLabelAsync(Label label, Check check)
        {
            Arguments.CheckNotNull(check, nameof(check));
            Arguments.CheckNotNull(label, nameof(label));

            return AddLabelAsync(label.Id, check.Id);
        }

        /// <summary>
        /// Add a label to a check.
        /// </summary>
        /// <param name="labelId">ID of label to add</param>
        /// <param name="checkId">ID of the check</param>
        /// <returns>the label was added to the check</returns>
        public async Task<Label> AddLabelAsync(string labelId, string checkId)
        {
            Arguments.CheckNonEmptyString(checkId, nameof(checkId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var mapping = new LabelMapping(labelId);

            var labels = await _service.PostChecksIDLabelsAsync(checkId, mapping);
            return labels.Label;
        }

        /// <summary>
        /// Delete label from a check.
        /// </summary>
        /// <param name="label">the label to delete</param>
        /// <param name="check">the check</param>
        /// <returns></returns>
        public Task DeleteLabelAsync(Label label, Check check)
        {
            Arguments.CheckNotNull(check, nameof(check));
            Arguments.CheckNotNull(label, nameof(label));

            return DeleteLabelAsync(label.Id, check.Id);
        }

        /// <summary>
        /// Delete label from a check.
        /// </summary>
        /// <param name="labelId">labelID the label id to delete</param>
        /// <param name="checkId">checkID ID of the check</param>
        /// <returns></returns>
        public Task DeleteLabelAsync(string labelId, string checkId)
        {
            Arguments.CheckNonEmptyString(checkId, nameof(checkId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return _service.DeleteChecksIDLabelsIDAsync(checkId, labelId);
        }

        private DashboardQuery CreateDashboardQuery(string query)
        {
            return new DashboardQuery(editMode: QueryEditMode.Advanced, text: query);
        }
    }
}