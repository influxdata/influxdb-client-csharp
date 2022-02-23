using System;
using System.Collections.Generic;
using System.Threading;
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
        /// <param name="task">task to create (required)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created Task</returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<TaskType> CreateTaskAsync(TaskType task, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, nameof(task));

            var status = (TaskStatusType)Enum.Parse(typeof(TaskStatusType), task.Status.ToString());
            var taskCreateRequest = new TaskCreateRequest(task.OrgID, task.Org, status,
                task.Flux, task.Description);

            return CreateTaskAsync(taskCreateRequest, cancellationToken);
        }

        /// <summary>
        /// Create a new task.
        /// </summary>
        /// <param name="taskCreateRequest">task to create (required)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created Task</returns>
        public Task<TaskType> CreateTaskAsync(TaskCreateRequest taskCreateRequest,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(taskCreateRequest, nameof(taskCreateRequest));

            return _service.PostTasksAsync(taskCreateRequest, cancellationToken: cancellationToken);
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<TaskType> CreateTaskCronAsync(string name, string flux, string cron,
            Organization organization, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(cron, nameof(cron));
            Arguments.CheckNotNull(organization, nameof(organization));

            return CreateTaskCronAsync(name, flux, cron, organization.Id, cancellationToken);
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<TaskType> CreateTaskCronAsync(string name, string flux, string cron,
            string orgId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(cron, nameof(cron));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var task = CreateTaskAsync(name, flux, null, cron, orgId);

            return CreateTaskAsync(task, cancellationToken);
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created Task</returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<TaskType> CreateTaskEveryAsync(string name, string flux, string every,
            Organization organization, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(every, nameof(every));
            Arguments.CheckNotNull(organization, nameof(organization));

            return CreateTaskEveryAsync(name, flux, every, organization.Id, cancellationToken);
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created Task</returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<TaskType> CreateTaskEveryAsync(string name, string flux, string every,
            string orgId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(every, nameof(every));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var task = CreateTaskAsync(name, flux, every, null, orgId);

            return CreateTaskAsync(task, cancellationToken);
        }

        /// <summary>
        /// Update a task. This will cancel all queued runs.
        /// </summary>
        /// <param name="task">task update to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>task updated</returns>
        public Task<TaskType> UpdateTaskAsync(TaskType task, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, nameof(task));

            var status = (TaskStatusType)Enum.Parse(typeof(TaskStatusType), task.Status.ToString());

            var request = new TaskUpdateRequest(status, task.Flux, task.Name, task.Every, task.Cron);

            return UpdateTaskAsync(task.Id, request, cancellationToken);
        }


        /// <summary>
        /// Update a task. This will cancel all queued runs.
        /// </summary>
        /// <param name="taskId">ID of task to get</param>
        /// <param name="request">task update to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>task updated</returns>
        public Task<TaskType> UpdateTaskAsync(string taskId, TaskUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNotNull(request, nameof(request));

            return _service.PatchTasksIDAsync(taskId, request, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Delete a task.
        /// </summary>
        /// <param name="taskId">ID of task to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>task deleted</returns>
        public Task DeleteTaskAsync(string taskId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(taskId, nameof(taskId));

            return _service.DeleteTasksIDAsync(taskId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Delete a task.
        /// </summary>
        /// <param name="task">task to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>task deleted</returns>
        public Task DeleteTaskAsync(TaskType task, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return DeleteTaskAsync(task.Id, cancellationToken);
        }

        /// <summary>
        /// Clone a task.
        /// </summary>
        /// <param name="taskId">ID of task to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned task</returns>
        public async Task<TaskType> CloneTaskAsync(string taskId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            var task = await FindTaskByIdAsync(taskId, cancellationToken).ConfigureAwait(false);

            return await CloneTaskAsync(task, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a task.
        /// </summary>
        /// <param name="task">task to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned task</returns>
        public async Task<TaskType> CloneTaskAsync(TaskType task, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, nameof(task));

            var status = (TaskStatusType)Enum.Parse(typeof(TaskStatusType), task.Status.ToString());
            var cloned = new TaskCreateRequest(task.OrgID, task.Org, status,
                task.Flux, task.Description);

            var created = await CreateTaskAsync(cloned, cancellationToken).ConfigureAwait(false);
            var labels = await GetLabelsAsync(task, cancellationToken).ConfigureAwait(false);
            foreach (var label in labels) await AddLabelAsync(label, created, cancellationToken).ConfigureAwait(false);

            return created;
        }

        /// <summary>
        /// Retrieve a task.
        /// </summary>
        /// <param name="taskId">ID of task to get</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>task details</returns>
        public Task<TaskType> FindTaskByIdAsync(string taskId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            return _service.GetTasksIDAsync(taskId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="user">filter tasks to a specific user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of tasks</returns>
        public Task<List<TaskType>> FindTasksByUserAsync(User user, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return FindTasksByUserIdAsync(user.Id, cancellationToken);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="userId">filter tasks to a specific user ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of tasks</returns>
        public Task<List<TaskType>> FindTasksByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return FindTasksAsync(userId: userId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="organization">filter tasks to a specific organization</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of tasks</returns>
        public Task<List<TaskType>> FindTasksByOrganizationAsync(Organization organization,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return FindTasksByOrganizationIdAsync(organization.Id, cancellationToken);
        }


        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="orgId">filter tasks to a specific organization ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of tasks</returns>
        public Task<List<TaskType>> FindTasksByOrganizationIdAsync(string orgId,
            CancellationToken cancellationToken = default)
        {
            return FindTasksAsync(orgId: orgId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="afterId">returns tasks after specified ID</param>
        /// <param name="userId">filter tasks to a specific user ID</param>
        /// <param name="orgId">filter tasks to a specific organization ID</param>
        /// <param name="org">Filter tasks to a specific organization name. (optional)</param>
        /// <param name="name">Returns task with a specific name. (optional)</param>
        /// <param name="limit">The number of tasks to return (optional, default to 100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of tasks</returns>
        public async Task<List<TaskType>> FindTasksAsync(string afterId = null, string userId = null,
            string orgId = null, string org = null, string name = null, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            var response = await _service
                .GetTasksAsync(after: afterId, user: userId, orgID: orgId, name: name, org: org, limit: limit,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response._Tasks;
        }

        /// <summary>
        /// List all members of a task.
        /// </summary>
        /// <param name="task">task of the members</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all members of a task</returns>
        public Task<List<ResourceMember>> GetMembersAsync(TaskType task, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, "task");

            return GetMembersAsync(task.Id, cancellationToken);
        }

        /// <summary>
        /// List all members of a task.
        /// </summary>
        /// <param name="taskId">ID of task to get members</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all members of a task</returns>
        public async Task<List<ResourceMember>> GetMembersAsync(string taskId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            var response = await _service.GetTasksIDMembersAsync(taskId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Users;
        }

        /// <summary>
        /// Add a task member.
        /// </summary>
        /// <param name="member">the member of a task</param>
        /// <param name="task">the task of a member</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created mapping</returns>
        public Task<ResourceMember> AddMemberAsync(User member, TaskType task,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(member, "member");

            return AddMemberAsync(member.Id, task.Id, cancellationToken);
        }

        /// <summary>
        /// Add a task member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="taskId">the ID of a task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created mapping</returns>
        public Task<ResourceMember> AddMemberAsync(string memberId, string taskId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return _service.PostTasksIDMembersAsync(taskId, new AddResourceMemberRequestBody(memberId),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Removes a member from a task.
        /// </summary>
        /// <param name="member">the member of a task</param>
        /// <param name="task">the task of a member</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>member removed</returns>
        public Task DeleteMemberAsync(User member, TaskType task, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(member, "member");

            return DeleteMemberAsync(member.Id, task.Id, cancellationToken);
        }

        /// <summary>
        /// Removes a member from a task.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="taskId">the ID of a task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>member removed</returns>
        public Task DeleteMemberAsync(string memberId, string taskId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return _service.DeleteTasksIDMembersIDAsync(memberId, taskId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// List all owners of a task.
        /// </summary>
        /// <param name="task">task of the owners</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all owners of a task</returns>
        public Task<List<ResourceOwner>> GetOwnersAsync(TaskType task, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, "Task is required");

            return GetOwnersAsync(task.Id, cancellationToken);
        }

        /// <summary>
        /// List all owners of a task.
        /// </summary>
        /// <param name="taskId">ID of a task to get owners</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all owners of a task</returns>
        public async Task<List<ResourceOwner>> GetOwnersAsync(string taskId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            var response = await _service.GetTasksIDOwnersAsync(taskId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Users;
        }

        /// <summary>
        /// Add a task owner.
        /// </summary>
        /// <param name="owner">the owner of a task</param>
        /// <param name="task">the task of a owner</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created mapping</returns>
        public Task<ResourceOwner> AddOwnerAsync(User owner, TaskType task,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(owner, "owner");

            return AddOwnerAsync(owner.Id, task.Id, cancellationToken);
        }

        /// <summary>
        /// Add a task owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="taskId">the ID of a task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created mapping</returns>
        public Task<ResourceOwner> AddOwnerAsync(string ownerId, string taskId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            return _service.PostTasksIDOwnersAsync(taskId, new AddResourceMemberRequestBody(ownerId),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Removes a owner from a task.
        /// </summary>
        /// <param name="owner">the owner of a task</param>
        /// <param name="task">the task of a owner</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>owner removed</returns>
        public Task DeleteOwnerAsync(User owner, TaskType task, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(owner, "owner");

            return DeleteOwnerAsync(owner.Id, task.Id, cancellationToken);
        }

        /// <summary>
        /// Removes a owner from a task.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="taskId">the ID of a task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>owner removed</returns>
        public Task DeleteOwnerAsync(string ownerId, string taskId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            return _service.DeleteTasksIDOwnersIDAsync(ownerId, taskId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieve all logs for a task.
        /// </summary>
        /// <param name="task">task to get logs for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the list of all logs for a task</returns>
        public Task<List<LogEvent>> GetLogsAsync(TaskType task, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return GetLogsAsync(task.Id, cancellationToken);
        }

        /// <summary>
        /// Retrieve all logs for a task.
        /// </summary>
        /// <param name="taskId">ID of task to get logs for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the list of all logs for a task</returns>
        public async Task<List<LogEvent>> GetLogsAsync(string taskId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            var response = await _service.GetTasksIDLogsAsync(taskId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Events;
        }

        /// <summary>
        /// Retrieve list of run records for a task.
        /// </summary>
        /// <param name="task"> task to get runs for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the list of run records for a task</returns>
        public Task<List<Run>> GetRunsAsync(TaskType task, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return GetRunsAsync(task, null, null, null, cancellationToken);
        }


        /// <summary>
        /// Retrieve list of run records for a task.
        /// </summary>
        /// <param name="task"> task to get runs for</param>
        /// <param name="afterTime">filter runs to those scheduled after this time</param>
        /// <param name="beforeTime">filter runs to those scheduled before this time</param>
        /// <param name="limit">the number of runs to return. Default value: 20.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the list of run records for a task</returns>
        public Task<List<Run>> GetRunsAsync(TaskType task, DateTime? afterTime,
            DateTime? beforeTime, int? limit, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return GetRunsAsync(task.Id, afterTime, beforeTime, limit, cancellationToken);
        }

        /// <summary>
        /// Retrieve list of run records for a task.
        /// </summary>
        /// <param name="taskId">ID of task to get runs for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the list of run records for a task</returns>
        public Task<List<Run>> GetRunsAsync(string taskId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            return GetRunsAsync(taskId, null, null, null, cancellationToken);
        }

        /// <summary>
        /// Retrieve list of run records for a task.
        /// </summary>
        /// <param name="taskId">ID of task to get runs for</param>
        /// <param name="afterTime">filter runs to those scheduled after this time</param>
        /// <param name="beforeTime">filter runs to those scheduled before this time</param>
        /// <param name="limit">the number of runs to return. Default value: 20.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the list of run records for a task</returns>
        public async Task<List<Run>> GetRunsAsync(string taskId,
            DateTime? afterTime, DateTime? beforeTime, int? limit, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            var response = await _service
                .GetTasksIDRunsAsync(taskId, null, null, limit, afterTime, beforeTime, cancellationToken)
                .ConfigureAwait(false);
            return response._Runs;
        }

        /// <summary>
        /// Retrieve a single run record for a task.
        /// </summary>
        /// <param name="taskId">ID of task to get runs for</param>
        /// <param name="runId">ID of run</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>a single run record for a task</returns>
        public Task<Run> GetRunAsync(string taskId, string runId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));

            return _service.GetTasksIDRunsIDAsync(taskId, runId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retry a task run.
        /// </summary>
        /// <param name="run">the run to retry</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the executed run</returns>
        public Task<Run> RetryRunAsync(Run run, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(run, nameof(run));

            return RetryRunAsync(run.TaskID, run.Id, cancellationToken);
        }

        /// <summary>
        /// Retry a task run.
        /// </summary>
        /// <param name="taskId">ID of task with the run to retry</param>
        /// <param name="runId">ID of run to retry</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the executed run</returns>
        public Task<Run> RetryRunAsync(string taskId, string runId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));

            return _service.PostTasksIDRunsIDRetryAsync(taskId, runId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Cancels a currently running run.
        /// </summary>
        /// <param name="run">the run to cancel</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public Task CancelRunAsync(Run run, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(run, nameof(run));

            return CancelRunAsync(run.TaskID, run.Id, cancellationToken);
        }

        /// <summary>
        /// Cancels a currently running run.
        /// </summary>
        /// <param name="taskId">ID of task with the run to cancel</param>
        /// <param name="runId">ID of run to cancel</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public Task CancelRunAsync(string taskId, string runId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));

            return _service.DeleteTasksIDRunsIDAsync(taskId, runId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieve all logs for a run.
        /// </summary>
        /// <param name="run">the run to gets logs for it</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the list of all logs for a run</returns>
        public Task<List<LogEvent>> GetRunLogsAsync(Run run, CancellationToken cancellationToken = default)
        {
            return GetRunLogsAsync(run.TaskID, run.Id, cancellationToken);
        }

        /// <summary>
        /// Retrieve all logs for a run.
        /// </summary>
        /// <param name="taskId">ID of task to get run logs for it</param>
        /// <param name="runId">ID of run to get logs for it</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the list of all logs for a run</returns>
        public async Task<List<LogEvent>> GetRunLogsAsync(string taskId, string runId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));

            var response = await _service.GetTasksIDRunsIDLogsAsync(taskId, runId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Events;
        }

        /// <summary>
        /// List all labels of a Task.
        /// </summary>
        /// <param name="task">a Task of the labels</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all labels of a Task</returns>
        public Task<List<Label>> GetLabelsAsync(TaskType task, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return GetLabelsAsync(task.Id, cancellationToken);
        }

        /// <summary>
        /// List all labels of a Task.
        /// </summary>
        /// <param name="taskId">ID of a Task to get labels</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all labels of a Task</returns>
        public async Task<List<Label>> GetLabelsAsync(string taskId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            var response = await _service.GetTasksIDLabelsAsync(taskId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Labels;
        }

        /// <summary>
        /// Add a Task label.
        /// </summary>
        /// <param name="label">the label of a Task</param>
        /// <param name="task">a Task of a label</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>added label</returns>
        public Task<Label> AddLabelAsync(Label label, TaskType task, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, nameof(task));
            Arguments.CheckNotNull(label, nameof(label));

            return AddLabelAsync(label.Id, task.Id, cancellationToken);
        }

        /// <summary>
        /// Add a Task label.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="taskId">the ID of a Task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabelAsync(string labelId, string taskId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var mapping = new LabelMapping(labelId);

            var response = await _service.PostTasksIDLabelsAsync(taskId, mapping, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Label;
        }

        /// <summary>
        /// Removes a label from a Task.
        /// </summary>
        /// <param name="label">the label of a Task</param>
        /// <param name="task">a Task of a owner</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteLabelAsync(Label label, TaskType task, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(task, nameof(task));
            Arguments.CheckNotNull(label, nameof(label));

            return DeleteLabelAsync(label.Id, task.Id, cancellationToken);
        }

        /// <summary>
        /// Removes a label from a Task.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="taskId">the ID of a Task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteLabelAsync(string labelId, string taskId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return _service.DeleteTasksIDLabelsIDAsync(taskId, labelId, cancellationToken: cancellationToken);
        }

        private TaskType CreateTaskAsync(string name, string flux, string every, string cron, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            if (every != null)
            {
                Arguments.CheckDuration(every, nameof(every));
            }

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