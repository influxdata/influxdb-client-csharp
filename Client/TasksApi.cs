using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;

namespace InfluxDB.Client
{
    public class TasksApi
    {
        private readonly TasksService _service;

        protected internal TasksApi(TasksService service)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
        }

        /// <summary>
        /// Creates a new task. The <see cref="InfluxDB.Client.Api.Domain.TaskType"/> has to have defined a cron or a every repetition
        /// by the <a href="http://bit.ly/option-statement">option statement</a>.
        /// <example>
        ///     This sample shows how to specify every repetition
        ///     <code>
        /// option task = {
        /// name: "mean",
        /// every: 1h,
        /// }
        /// 
        /// from(bucket:"metrics/autogen")
        /// |&gt; range(start:-task.every)
        /// |&gt; group(columns:["level"])
        /// |&gt; mean()
        /// |&gt; yield(name:"mean")
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<TaskType> CreateTaskAsync(TaskType task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            var status = (TaskStatusType) Enum.Parse(typeof(TaskStatusType), task.Status.ToString());
            var taskCreateRequest = new TaskCreateRequest(orgID: task.OrgID, org: task.Org, status: status,
                flux: task.Flux, description: task.Description);

            return await CreateTaskAsync(taskCreateRequest);
        }

        /// <summary>
        /// Create a new task.
        /// </summary>
        /// <param name="taskCreateRequest">task to create (required)</param>
        /// <returns>Task created</returns>
        public async Task<TaskType> CreateTaskAsync(TaskCreateRequest taskCreateRequest)
        {
            Arguments.CheckNotNull(taskCreateRequest, nameof(taskCreateRequest));

            return await _service.PostTasksAsync(taskCreateRequest);
        }

        /// <summary>
        /// Creates a new task with task repetition by cron. The <see cref="TaskType.Flux" /> is without a cron or a every
        /// repetition.
        /// The repetition is automatically append to the <a href="http://bit.ly/option-statement">option statement</a>.
        /// </summary>
        /// <param name="name">description of the task</param>
        /// <param name="flux">the Flux script to run for this task</param>
        /// <param name="cron">a task repetition schedule in the form '* * * * * *'</param>
        /// <param name="organization">the organization that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<TaskType> CreateTaskCronAsync(string name, string flux, string cron,
            Organization organization)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(cron, nameof(cron));
            Arguments.CheckNotNull(organization, nameof(organization));

            return await CreateTaskCronAsync(name, flux, cron, organization.Id);
        }

        /// <summary>
        /// Creates a new task with task repetition by cron. The <see cref="TaskType.Flux" /> is without a cron or a every
        /// repetition.
        /// The repetition is automatically append to the <a href="http://bit.ly/option-statement">option statement</a>.
        /// </summary>
        /// <param name="name">description of the task</param>
        /// <param name="flux">the Flux script to run for this task</param>
        /// <param name="cron">a task repetition schedule in the form '* * * * * *'</param>
        /// <param name="orgId">the organization ID that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<TaskType> CreateTaskCronAsync(string name, string flux, string cron,
            string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(cron, nameof(cron));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var task = CreateTaskAsync(name, flux, null, cron, orgId);

            return await CreateTaskAsync(task);
        }

        /// <summary>
        /// Creates a new task with task repetition by duration expression ("1h", "30s"). The <see cref="TaskType.Flux" /> is
        /// without a cron or a every repetition.
        /// The repetition is automatically append to the <a href="http://bit.ly/option-statement">option statement</a>.
        /// </summary>
        /// <param name="name">description of the task</param>
        /// <param name="flux">the Flux script to run for this task</param>
        /// <param name="every">a task repetition by duration expression</param>
        /// <param name="organization">the organization that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<TaskType> CreateTaskEveryAsync(string name, string flux, string every,
            Organization organization)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(every, nameof(every));
            Arguments.CheckNotNull(organization, nameof(organization));

            return await CreateTaskEveryAsync(name, flux, every, organization.Id);
        }

        /// <summary>
        /// Creates a new task with task repetition by duration expression ("1h", "30s"). The <see cref="TaskType.Flux" /> is
        /// without a cron or a every repetition.
        /// The repetition is automatically append to the <a href="http://bit.ly/option-statement">option statement</a>.
        /// </summary>
        /// <param name="name">description of the task</param>
        /// <param name="flux">the Flux script to run for this task</param>
        /// <param name="every">a task repetition by duration expression</param>
        /// <param name="orgId">the organization ID that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<TaskType> CreateTaskEveryAsync(string name, string flux, string every,
            string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(every, nameof(every));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var task = CreateTaskAsync(name, flux, every, null, orgId);

            return await CreateTaskAsync(task);
        }

        /// <summary>
        /// Update a task. This will cancel all queued runs.
        /// </summary>
        /// <param name="task">task update to apply</param>
        /// <returns>task updated</returns>
        public async Task<TaskType> UpdateTaskAsync(TaskType task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            var status = (TaskStatusType) Enum.Parse(typeof(TaskStatusType), task.Status.ToString());

            var request = new TaskUpdateRequest(status, task.Flux, task.Name, task.Every, task.Cron);

            return await UpdateTaskAsync(task.Id, request);
        }


        /// <summary>
        /// Update a task. This will cancel all queued runs.
        /// </summary>
        /// <param name="taskId">ID of task to get</param>
        /// <param name="request">task update to apply</param>
        /// <returns>task updated</returns>
        public async Task<TaskType> UpdateTaskAsync(string taskId, TaskUpdateRequest request)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNotNull(request, nameof(request));

            return await _service.PatchTasksIDAsync(taskId, request);
        }

        /// <summary>
        /// Delete a task.
        /// </summary>
        /// <param name="taskId">ID of task to delete</param>
        /// <returns>task deleted</returns>
        public async Task DeleteTaskAsync(string taskId)
        {
            Arguments.CheckNotNull(taskId, nameof(taskId));

            await _service.DeleteTasksIDAsync(taskId);
        }

        /// <summary>
        /// Delete a task.
        /// </summary>
        /// <param name="task">task to delete</param>
        /// <returns>task deleted</returns>
        public async Task DeleteTaskAsync(TaskType task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            await DeleteTaskAsync(task.Id);
        }

        /// <summary>
        /// Clone a task.
        /// </summary>
        /// <param name="taskId">ID of task to clone</param>
        /// <returns>cloned task</returns>
        public async Task<TaskType> CloneTaskAsync(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            return await FindTaskByIdAsync(taskId).ContinueWith(t => CloneTaskAsync(t.Result)).Unwrap();
        }

        /// <summary>
        /// Clone a task.
        /// </summary>
        /// <param name="task">task to clone</param>
        /// <returns>cloned task</returns>
        public async Task<TaskType> CloneTaskAsync(TaskType task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            var status = (TaskStatusType) Enum.Parse(typeof(TaskStatusType), task.Status.ToString());
            var cloned = new TaskCreateRequest(orgID: task.OrgID, org: task.Org, status: status,
                flux: task.Flux, description: task.Description);

            return await CreateTaskAsync(cloned).ContinueWith(created =>
            {
                //
                // Add labels
                //
                return GetLabelsAsync(task)
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
        /// Retrieve a task.
        /// </summary>
        /// <param name="taskId">ID of task to get</param>
        /// <returns>task details</returns>
        public async Task<TaskType> FindTaskByIdAsync(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            return await _service.GetTasksIDAsync(taskId);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="user">filter tasks to a specific user</param>
        /// <returns>A list of tasks</returns>
        public async Task<List<TaskType>> FindTasksByUserAsync(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return await FindTasksByUserIdAsync(user.Id);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="userId">filter tasks to a specific user ID</param>
        /// <returns>A list of tasks</returns>
        public async Task<List<TaskType>> FindTasksByUserIdAsync(string userId)
        {
            return await FindTasksAsync(null, userId);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="organization">filter tasks to a specific organization</param>
        /// <returns>A list of tasks</returns>
        public async Task<List<TaskType>> FindTasksByOrganizationAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await FindTasksByOrganizationIdAsync(organization.Id);
        }


        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="orgId">filter tasks to a specific organization ID</param>
        /// <returns>A list of tasks</returns>
        public async Task<List<TaskType>> FindTasksByOrganizationIdAsync(string orgId)
        {
            return await FindTasksAsync(null, null, orgId);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="afterId">returns tasks after specified ID</param>
        /// <param name="userId">filter tasks to a specific user ID</param>
        /// <param name="orgId">filter tasks to a specific organization ID</param>
        /// <returns>A list of tasks</returns>
        public async Task<List<TaskType>> FindTasksAsync(string afterId = null, string userId = null, string orgId = null)
        {
            return await _service.GetTasksAsync(null, null, afterId, userId, orgId).ContinueWith(t => t.Result._Tasks);
        }

        /// <summary>
        /// List all members of a task.
        /// </summary>
        /// <param name="task">task of the members</param>
        /// <returns>the List all members of a task</returns>
        public async Task<List<ResourceMember>> GetMembersAsync(TaskType task)
        {
            Arguments.CheckNotNull(task, "task");

            return await GetMembersAsync(task.Id);
        }

        /// <summary>
        /// List all members of a task.
        /// </summary>
        /// <param name="taskId">ID of task to get members</param>
        /// <returns>the List all members of a task</returns>
        public async Task<List<ResourceMember>> GetMembersAsync(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            return await _service.GetTasksIDMembersAsync(taskId).ContinueWith(t => t.Result.Users);
        }

        /// <summary>
        /// Add a task member.
        /// </summary>
        /// <param name="member">the member of a task</param>
        /// <param name="task">the task of a member</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddMemberAsync(User member, TaskType task)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(member, "member");

            return await AddMemberAsync(member.Id, task.Id);
        }

        /// <summary>
        /// Add a task member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="taskId">the ID of a task</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddMemberAsync(string memberId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return await _service.PostTasksIDMembersAsync(taskId, new AddResourceMemberRequestBody(memberId));
        }

        /// <summary>
        /// Removes a member from a task.
        /// </summary>
        /// <param name="member">the member of a task</param>
        /// <param name="task">the task of a member</param>
        /// <returns>member removed</returns>
        public async Task DeleteMemberAsync(User member, TaskType task)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(member, "member");

            await DeleteMemberAsync(member.Id, task.Id);
        }

        /// <summary>
        /// Removes a member from a task.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="taskId">the ID of a task</param>
        /// <returns>member removed</returns>
        public async Task DeleteMemberAsync(string memberId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            await _service.DeleteTasksIDMembersIDAsync(memberId, taskId);
        }

        /// <summary>
        /// List all owners of a task.
        /// </summary>
        /// <param name="task">task of the owners</param>
        /// <returns>the List all owners of a task</returns>
        public async Task<List<ResourceOwner>> GetOwnersAsync(TaskType task)
        {
            Arguments.CheckNotNull(task, "Task is required");

            return await GetOwnersAsync(task.Id);
        }

        /// <summary>
        /// List all owners of a task.
        /// </summary>
        /// <param name="taskId">ID of a task to get owners</param>
        /// <returns>the List all owners of a task</returns>
        public async Task<List<ResourceOwner>> GetOwnersAsync(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            return await _service.GetTasksIDOwnersAsync(taskId).ContinueWith(t => t.Result.Users);
        }

        /// <summary>
        /// Add a task owner.
        /// </summary>
        /// <param name="owner">the owner of a task</param>
        /// <param name="task">the task of a owner</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceOwner> AddOwnerAsync(User owner, TaskType task)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(owner, "owner");

            return await AddOwnerAsync(owner.Id, task.Id);
        }

        /// <summary>
        /// Add a task owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="taskId">the ID of a task</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceOwner> AddOwnerAsync(string ownerId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            return await _service.PostTasksIDOwnersAsync(taskId, new AddResourceMemberRequestBody(ownerId));
        }

        /// <summary>
        /// Removes a owner from a task.
        /// </summary>
        /// <param name="owner">the owner of a task</param>
        /// <param name="task">the task of a owner</param>
        /// <returns>owner removed</returns>
        public async Task DeleteOwnerAsync(User owner, TaskType task)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(owner, "owner");

            await DeleteOwnerAsync(owner.Id, task.Id);
        }

        /// <summary>
        /// Removes a owner from a task.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="taskId">the ID of a task</param>
        /// <returns>owner removed</returns>
        public async Task DeleteOwnerAsync(string ownerId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            await _service.DeleteTasksIDOwnersIDAsync(ownerId, taskId);
        }

        /// <summary>
        /// Retrieve all logs for a task.
        /// </summary>
        /// <param name="task">task to get logs for</param>
        /// <returns>the list of all logs for a task</returns>
        public async Task<List<LogEvent>> GetLogsAsync(TaskType task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return await GetLogsAsync(task.Id);
        }

        /// <summary>
        /// Retrieve all logs for a task.
        /// </summary>
        /// <param name="taskId">ID of task to get logs for</param>
        /// <returns>the list of all logs for a task</returns>
        public async Task<List<LogEvent>> GetLogsAsync(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            return await _service.GetTasksIDLogsAsync(taskId).ContinueWith(t => t.Result.Events);
        }

        /// <summary>
        /// Retrieve list of run records for a task.
        /// </summary>
        /// <param name="task"> task to get runs for</param>
        /// <returns>the list of run records for a task</returns>
        public async Task<List<Run>> GetRunsAsync(TaskType task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return await GetRunsAsync(task, null, null, null);
        }


        /// <summary>
        /// Retrieve list of run records for a task.
        /// </summary>
        /// <param name="task"> task to get runs for</param>
        /// <param name="afterTime">filter runs to those scheduled after this time</param>
        /// <param name="beforeTime">filter runs to those scheduled before this time</param>
        /// <param name="limit">the number of runs to return. Default value: 20.</param>
        /// <returns>the list of run records for a task</returns>
        public async Task<List<Run>> GetRunsAsync(TaskType task, DateTime? afterTime,
            DateTime? beforeTime, int? limit)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return await GetRunsAsync(task.Id, task.Org, afterTime, beforeTime, limit);
        }

        /// <summary>
        /// Retrieve list of run records for a task.
        /// </summary>
        /// <param name="taskId">ID of task to get runs for</param>
        /// <param name="orgId">ID of organization</param>
        /// <returns>the list of run records for a task</returns>
        public async Task<List<Run>> GetRunsAsync(string taskId, string orgId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return await GetRunsAsync(taskId, orgId, null, null, null);
        }

        /// <summary>
        /// Retrieve list of run records for a task.
        /// </summary>
        /// <param name="taskId">ID of task to get runs for</param>
        /// <param name="orgId">ID of organization</param>
        /// <param name="afterTime">filter runs to those scheduled after this time</param>
        /// <param name="beforeTime">filter runs to those scheduled before this time</param>
        /// <param name="limit">the number of runs to return. Default value: 20.</param>
        /// <returns>the list of run records for a task</returns>
        public async Task<List<Run>> GetRunsAsync(string taskId, string orgId,
            DateTime? afterTime, DateTime? beforeTime, int? limit)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return await _service.GetTasksIDRunsAsync(taskId, null, null, limit, afterTime, beforeTime)
                .ContinueWith(t => t.Result._Runs);
        }

        /// <summary>
        /// Retrieve a single run record for a task.
        /// </summary>
        /// <param name="taskId">ID of task to get runs for</param>
        /// <param name="runId">ID of run</param>
        /// <returns>a single run record for a task</returns>
        public async Task<Run> GetRunAsync(string taskId, string runId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));

            return await _service.GetTasksIDRunsIDAsync(taskId, runId);
        }

        /// <summary>
        /// Retry a task run.
        /// </summary>
        /// <param name="run">the run to retry</param>
        /// <returns>the executed run</returns>
        public async Task<Run> RetryRunAsync(Run run)
        {
            Arguments.CheckNotNull(run, nameof(run));

            return await RetryRunAsync(run.TaskID, run.Id);
        }

        /// <summary>
        /// Retry a task run.
        /// </summary>
        /// <param name="taskId">ID of task with the run to retry</param>
        /// <param name="runId">ID of run to retry</param>
        /// <returns>the executed run</returns>
        public async Task<Run> RetryRunAsync(string taskId, string runId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));

            return await _service.PostTasksIDRunsIDRetryAsync(taskId, runId);
        }

        /// <summary>
        /// Cancels a currently running run.
        /// </summary>
        /// <param name="run">the run to cancel</param>
        /// <returns></returns>
        public async Task CancelRunAsync(Run run)
        {
            Arguments.CheckNotNull(run, nameof(run));

            await CancelRunAsync(run.TaskID, run.Id);
        }

        /// <summary>
        /// Cancels a currently running run.
        /// </summary>
        /// <param name="taskId">ID of task with the run to cancel</param>
        /// <param name="runId">ID of run to cancel</param>
        /// <returns></returns>
        public async Task CancelRunAsync(string taskId, string runId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));

            await _service.DeleteTasksIDRunsIDAsync(taskId, runId);
        }

        /// <summary>
        /// Retrieve all logs for a run.
        /// </summary>
        /// <param name="run">the run to gets logs for it</param>
        /// <returns>the list of all logs for a run</returns>
        public async Task<List<LogEvent>> GetRunLogsAsync(Run run)
        {
            return await GetRunLogsAsync(run.TaskID, run.Id);
        }

        /// <summary>
        /// Retrieve all logs for a run.
        /// </summary>
        /// <param name="taskId">ID of task to get run logs for it</param>
        /// <param name="runId">ID of run to get logs for it</param>
        /// <returns>the list of all logs for a run</returns>
        public Task<List<LogEvent>> GetRunLogsAsync(string taskId, string runId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));

            return _service.GetTasksIDRunsIDLogsAsync(taskId, runId).ContinueWith(t => t.Result.Events);
        }

        /// <summary>
        /// List all labels of a Task.
        /// </summary>
        /// <param name="task">a Task of the labels</param>
        /// <returns>the List all labels of a Task</returns>
        public async Task<List<Label>> GetLabelsAsync(TaskType task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return await GetLabelsAsync(task.Id);
        }

        /// <summary>
        /// List all labels of a Task.
        /// </summary>
        /// <param name="taskId">ID of a Task to get labels</param>
        /// <returns>the List all labels of a Task</returns>
        public async Task<List<Label>> GetLabelsAsync(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            return await _service.GetTasksIDLabelsAsync(taskId).ContinueWith(t => t.Result.Labels);
        }

        /// <summary>
        /// Add a Task label.
        /// </summary>
        /// <param name="label">the label of a Task</param>
        /// <param name="task">a Task of a label</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabelAsync(Label label, TaskType task)
        {
            Arguments.CheckNotNull(task, nameof(task));
            Arguments.CheckNotNull(label, nameof(label));

            return await AddLabelAsync(label.Id, task.Id);
        }

        /// <summary>
        /// Add a Task label.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="taskId">the ID of a Task</param>
        /// <returns>added label</returns>
        public Task<Label> AddLabelAsync(string labelId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var mapping = new LabelMapping(labelId);

            return _service.PostTasksIDLabelsAsync(taskId, mapping).ContinueWith(t => t.Result.Label);
        }

        /// <summary>
        /// Removes a label from a Task.
        /// </summary>
        /// <param name="label">the label of a Task</param>
        /// <param name="task">a Task of a owner</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteLabelAsync(Label label, TaskType task)
        {
            Arguments.CheckNotNull(task, nameof(task));
            Arguments.CheckNotNull(label, nameof(label));

            await DeleteLabelAsync(label.Id, task.Id);
        }

        /// <summary>
        /// Removes a label from a Task.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="taskId">the ID of a Task</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteLabelAsync(string labelId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            await _service.DeleteTasksIDLabelsIDAsync(taskId, labelId);
        }

        private TaskType CreateTaskAsync(string name, string flux, string every, string cron, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            if (every != null) Arguments.CheckDuration(every, nameof(every));

            var task = new TaskType(orgID: orgId, name: name, status: TaskStatusType.Active, flux: flux);

            var repetition = "";
            if (every != null)
            {
                repetition += "every: ";
                repetition += every;
            }

            if (cron != null)
            {
                repetition += "cron: ";
                repetition += "\"" + cron + "\"";
            }

            task.Flux = $"option task = {{name: \"{name}\", {repetition}}} \n {flux}";

            return task;
        }
    }
}