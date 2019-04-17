using System;
using System.Collections.Generic;
using InfluxDB.Client.Core;
using InfluxDB.Client.Generated.Domain;
using InfluxDB.Client.Generated.Service;

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
        /// Creates a new task. The <see cref="InfluxDB.Client.Generated.Domain.Task"/> has to have defined a cron or a every repetition
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
        public Task CreateTask(Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            var status = (TaskCreateRequest.StatusEnum) Enum.Parse(typeof(TaskCreateRequest.StatusEnum), task.Status.ToString());
            var taskCreateRequest = new TaskCreateRequest(task.OrgID, task.Org, status, task.Flux);

            return CreateTask(taskCreateRequest);
        }
        
        /// <summary>
        /// Create a new task.
        /// </summary>
        /// <param name="taskCreateRequest">task to create (required)</param>
        /// <returns>Task created</returns>
        public Task CreateTask(TaskCreateRequest taskCreateRequest)
        {
            Arguments.CheckNotNull(taskCreateRequest, nameof(taskCreateRequest));

            return _service.TasksPost(taskCreateRequest);
        }

        /// <summary>
        /// Creates a new task with task repetition by cron. The <see cref="Task.Flux" /> is without a cron or a every
        /// repetition.
        /// The repetition is automatically append to the <a href="http://bit.ly/option-statement">option statement</a>.
        /// </summary>
        /// <param name="name">description of the task</param>
        /// <param name="flux">the Flux script to run for this task</param>
        /// <param name="cron">a task repetition schedule in the form '* * * * * *'</param>
        /// <param name="organization">the organization that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task CreateTaskCron(string name, string flux, string cron,
            Organization organization)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(cron, nameof(cron));
            Arguments.CheckNotNull(organization, nameof(organization));

            return CreateTaskCron(name, flux, cron, organization.Id);
        }

        /// <summary>
        /// Creates a new task with task repetition by cron. The <see cref="Task.Flux" /> is without a cron or a every
        /// repetition.
        /// The repetition is automatically append to the <a href="http://bit.ly/option-statement">option statement</a>.
        /// </summary>
        /// <param name="name">description of the task</param>
        /// <param name="flux">the Flux script to run for this task</param>
        /// <param name="cron">a task repetition schedule in the form '* * * * * *'</param>
        /// <param name="orgId">the organization ID that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task CreateTaskCron(string name, string flux, string cron,
            string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(cron, nameof(cron));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var task = CreateTask(name, flux, null, cron, orgId);

            return CreateTask(task);
        }

        /// <summary>
        /// Creates a new task with task repetition by duration expression ("1h", "30s"). The <see cref="Task.Flux" /> is
        /// without a cron or a every repetition.
        /// The repetition is automatically append to the <a href="http://bit.ly/option-statement">option statement</a>.
        /// </summary>
        /// <param name="name">description of the task</param>
        /// <param name="flux">the Flux script to run for this task</param>
        /// <param name="every">a task repetition by duration expression</param>
        /// <param name="organization">the organization that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task CreateTaskEvery(string name, string flux, string every,
            Organization organization)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(every, nameof(every));
            Arguments.CheckNotNull(organization, nameof(organization));

            return CreateTaskEvery(name, flux, every, organization.Id);
        }

        /// <summary>
        /// Creates a new task with task repetition by duration expression ("1h", "30s"). The <see cref="Task.Flux" /> is
        /// without a cron or a every repetition.
        /// The repetition is automatically append to the <a href="http://bit.ly/option-statement">option statement</a>.
        /// </summary>
        /// <param name="name">description of the task</param>
        /// <param name="flux">the Flux script to run for this task</param>
        /// <param name="every">a task repetition by duration expression</param>
        /// <param name="orgId">the organization ID that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task CreateTaskEvery(string name, string flux, string every,
            string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(every, nameof(every));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var task = CreateTask(name, flux, every, null, orgId);

            return CreateTask(task);
        }

        /// <summary>
        /// Update a task. This will cancel all queued runs.
        /// </summary>
        /// <param name="task">task update to apply</param>
        /// <returns>task updated</returns>
        public Task UpdateTask(Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            var status = (TaskUpdateRequest.StatusEnum) Enum.Parse(typeof(TaskUpdateRequest.StatusEnum), task.Status.ToString());

            var request = new TaskUpdateRequest(status, task.Flux, task.Name, task.Every, task.Cron);

            return UpdateTask(task.Id, request);
        }

        
        /// <summary>
        /// Update a task. This will cancel all queued runs.
        /// </summary>
        /// <param name="taskId">ID of task to get</param>
        /// <param name="request">task update to apply</param>
        /// <returns>task updated</returns>
        public Task UpdateTask(string taskId, TaskUpdateRequest request)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNotNull(request, nameof(request));

            return _service.TasksTaskIDPatch(taskId, request);
        }

        /// <summary>
        /// Delete a task.
        /// </summary>
        /// <param name="taskId">ID of task to delete</param>
        /// <returns>task deleted</returns>
        public void DeleteTask(string taskId)
        {
            Arguments.CheckNotNull(taskId, nameof(taskId));

            _service.TasksTaskIDDelete(taskId);
        }

        /// <summary>
        /// Delete a task.
        /// </summary>
        /// <param name="task">task to delete</param>
        /// <returns>task deleted</returns>
        public void DeleteTask(Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            DeleteTask(task.Id);
        }

        /// <summary>
        /// Clone a task.
        /// </summary>
        /// <param name="taskId">ID of task to clone</param>
        /// <returns>cloned task</returns>
        public Task CloneTask(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            var task = FindTaskById(taskId);
            if (task == null) throw new InvalidOperationException($"NotFound Task with ID: {taskId}");

            return CloneTask(task);
        }

        /// <summary>
        /// Clone a task.
        /// </summary>
        /// <param name="task">task to clone</param>
        /// <returns>cloned task</returns>
        public Task CloneTask(Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            var status = (TaskCreateRequest.StatusEnum) Enum.Parse(typeof(TaskCreateRequest.StatusEnum), task.Status.ToString());
            var cloned = new TaskCreateRequest(task.OrgID, task.Org, status, task.Flux);
            

            var created = CreateTask(cloned);

            foreach (var label in GetLabels(task)) AddLabel(label, created);

            return created;
        }

        /// <summary>
        /// Retrieve a task.
        /// </summary>
        /// <param name="taskId">ID of task to get</param>
        /// <returns>task details</returns>
        public Task FindTaskById(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            return _service.TasksTaskIDGet(taskId);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="user">filter tasks to a specific user</param>
        /// <returns>A list of tasks</returns>
        public List<Task> FindTasksByUser(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return FindTasksByUserId(user.Id);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="userId">filter tasks to a specific user ID</param>
        /// <returns>A list of tasks</returns>
        public List<Task> FindTasksByUserId(string userId)
        {
            return FindTasks(null, userId, null);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="organization">filter tasks to a specific organization</param>
        /// <returns>A list of tasks</returns>
        public List<Task> FindTasksByOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return FindTasksByOrganizationId(organization.Id);
        }


        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="orgId">filter tasks to a specific organization ID</param>
        /// <returns>A list of tasks</returns>
        public List<Task> FindTasksByOrganizationId(string orgId)
        {
            return FindTasks(null, null, orgId);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="afterId">returns tasks after specified ID</param>
        /// <param name="userId">filter tasks to a specific user ID</param>
        /// <param name="orgId">filter tasks to a specific organization ID</param>
        /// <returns>A list of tasks</returns>
        public List<Task> FindTasks(string afterId = null, string userId = null, string orgId = null)
        {
            return _service.TasksGet(null, afterId, userId, null, orgId)._Tasks;
        }

        /// <summary>
        /// List all members of a task.
        /// </summary>
        /// <param name="task">task of the members</param>
        /// <returns>the List all members of a task</returns>
        public List<ResourceMember> GetMembers(Task task)
        {
            Arguments.CheckNotNull(task, "task");

            return GetMembers(task.Id);
        }

        /// <summary>
        /// List all members of a task.
        /// </summary>
        /// <param name="taskId">ID of task to get members</param>
        /// <returns>the List all members of a task</returns>
        public List<ResourceMember> GetMembers(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            return _service.TasksTaskIDMembersGet(taskId).Users;
        }

        /// <summary>
        /// Add a task member.
        /// </summary>
        /// <param name="member">the member of a task</param>
        /// <param name="task">the task of a member</param>
        /// <returns>created mapping</returns>
        public ResourceMember AddMember(User member, Task task)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(member, "member");

            return AddMember(member.Id, task.Id);
        }

        /// <summary>
        /// Add a task member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="taskId">the ID of a task</param>
        /// <returns>created mapping</returns>
        public ResourceMember AddMember(string memberId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            var user = new User(memberId);

            return _service.TasksTaskIDMembersPost(taskId, new AddResourceMemberRequestBody(memberId));
        }

        /// <summary>
        /// Removes a member from a task.
        /// </summary>
        /// <param name="member">the member of a task</param>
        /// <param name="task">the task of a member</param>
        /// <returns>member removed</returns>
        public void DeleteMember(User member, Task task)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(member, "member");

            DeleteMember(member.Id, task.Id);
        }

        /// <summary>
        /// Removes a member from a task.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="taskId">the ID of a task</param>
        /// <returns>member removed</returns>
        public void DeleteMember(string memberId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            _service.TasksTaskIDMembersUserIDDelete(memberId, taskId);
        }

        /// <summary>
        /// List all owners of a task.
        /// </summary>
        /// <param name="task">task of the owners</param>
        /// <returns>the List all owners of a task</returns>
        public List<ResourceOwner> GetOwners(Task task)
        {
            Arguments.CheckNotNull(task, "Task is required");

            return GetOwners(task.Id);
        }

        /// <summary>
        /// List all owners of a task.
        /// </summary>
        /// <param name="taskId">ID of a task to get owners</param>
        /// <returns>the List all owners of a task</returns>
        public List<ResourceOwner> GetOwners(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            return _service.TasksTaskIDOwnersGet(taskId).Users;
        }

        /// <summary>
        /// Add a task owner.
        /// </summary>
        /// <param name="owner">the owner of a task</param>
        /// <param name="task">the task of a owner</param>
        /// <returns>created mapping</returns>
        public ResourceOwner AddOwner(User owner, Task task)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(owner, "owner");

            return AddOwner(owner.Id, task.Id);
        }

        /// <summary>
        /// Add a task owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="taskId">the ID of a task</param>
        /// <returns>created mapping</returns>
        public ResourceOwner AddOwner(string ownerId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            return _service.TasksTaskIDOwnersPost(taskId, new AddResourceMemberRequestBody(ownerId));
        }

        /// <summary>
        /// Removes a owner from a task.
        /// </summary>
        /// <param name="owner">the owner of a task</param>
        /// <param name="task">the task of a owner</param>
        /// <returns>owner removed</returns>
        public void DeleteOwner(User owner, Task task)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(owner, "owner");

            DeleteOwner(owner.Id, task.Id);
        }

        /// <summary>
        /// Removes a owner from a task.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="taskId">the ID of a task</param>
        /// <returns>owner removed</returns>
        public void DeleteOwner(string ownerId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            _service.TasksTaskIDOwnersUserIDDelete(ownerId, taskId);
        }

        /// <summary>
        /// Retrieve all logs for a task.
        /// </summary>
        /// <param name="task">task to get logs for</param>
        /// <returns>the list of all logs for a task</returns>
        public List<LogEvent> GetLogs(Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return GetLogs(task.Id);
        }

        /// <summary>
        /// Retrieve all logs for a task.
        /// </summary>
        /// <param name="taskId">ID of task to get logs for</param>
        /// <returns>the list of all logs for a task</returns>
        public List<LogEvent> GetLogs(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            return _service.TasksTaskIDLogsGet(taskId).Events;
        }

        /// <summary>
        /// Retrieve list of run records for a task.
        /// </summary>
        /// <param name="task"> task to get runs for</param>
        /// <returns>the list of run records for a task</returns>
        public List<Run> GetRuns(Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return GetRuns(task, null, null, null);
        }


        /// <summary>
        /// Retrieve list of run records for a task.
        /// </summary>
        /// <param name="task"> task to get runs for</param>
        /// <param name="afterTime">filter runs to those scheduled after this time</param>
        /// <param name="beforeTime">filter runs to those scheduled before this time</param>
        /// <param name="limit">the number of runs to return. Default value: 20.</param>
        /// <returns>the list of run records for a task</returns>
        public List<Run> GetRuns(Task task, DateTime? afterTime,
            DateTime? beforeTime, int? limit)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return GetRuns(task.Id, task.Org, afterTime, beforeTime, limit);
        }

        /// <summary>
        /// Retrieve list of run records for a task.
        /// </summary>
        /// <param name="taskId">ID of task to get runs for</param>
        /// <param name="orgId">ID of organization</param>
        /// <returns>the list of run records for a task</returns>
        public List<Run> GetRuns(string taskId, string orgId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return GetRuns(taskId, orgId, null, null, null);
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
        public List<Run> GetRuns(string taskId, string orgId,
            DateTime? afterTime, DateTime? beforeTime, int? limit)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return _service.TasksTaskIDRunsGet(taskId, null, null, limit, afterTime, beforeTime)._Runs;
        }

        /// <summary>
        /// Retrieve a single run record for a task.
        /// </summary>
        /// <param name="taskId">ID of task to get runs for</param>
        /// <param name="runId">ID of run</param>
        /// <returns>a single run record for a task</returns>
        public Run GetRun(string taskId, string runId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));

            return _service.TasksTaskIDRunsRunIDGet(taskId, runId);
        }

        /// <summary>
        /// Retry a task run.
        /// </summary>
        /// <param name="run">the run to retry</param>
        /// <returns>the executed run</returns>
        public Run RetryRun(Run run)
        {
            Arguments.CheckNotNull(run, nameof(run));

            return RetryRun(run.TaskID, run.Id);
        }

        /// <summary>
        /// Retry a task run.
        /// </summary>
        /// <param name="taskId">ID of task with the run to retry</param>
        /// <param name="runId">ID of run to retry</param>
        /// <returns>the executed run</returns>
        public Run RetryRun(string taskId, string runId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));

            return _service.TasksTaskIDRunsRunIDRetryPost(taskId, runId);
        }

        /// <summary>
        /// Cancels a currently running run.
        /// </summary>
        /// <param name="run">the run to cancel</param>
        /// <returns></returns>
        public void CancelRun(Run run)
        {
            Arguments.CheckNotNull(run, nameof(run));

            CancelRun(run.TaskID, run.Id);
        }

        /// <summary>
        /// Cancels a currently running run.
        /// </summary>
        /// <param name="taskId">ID of task with the run to cancel</param>
        /// <param name="runId">ID of run to cancel</param>
        /// <returns></returns>
        public void CancelRun(string taskId, string runId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));

            _service.TasksTaskIDRunsRunIDDelete(taskId, runId);
        }

        /// <summary>
        /// Retrieve all logs for a run.
        /// </summary>
        /// <param name="run">the run to gets logs for it</param>
        /// <param name="orgId">ID of organization to get logs for it</param>
        /// <returns>the list of all logs for a run</returns>
        public List<LogEvent> GetRunLogs(Run run, string orgId)
        {
            return GetRunLogs(run.TaskID, run.Id, orgId);
        }

        /// <summary>
        /// Retrieve all logs for a run.
        /// </summary>
        /// <param name="taskId">ID of task to get run logs for it</param>
        /// <param name="runId">ID of run to get logs for it</param>
        /// <param name="orgId">ID of organization to get logs for it</param>
        /// <returns>the list of all logs for a run</returns>
        public List<LogEvent> GetRunLogs(string taskId, string runId, string orgId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return _service.TasksTaskIDRunsRunIDLogsGet(taskId, runId).Events;
        }

        /// <summary>
        /// List all labels of a Task.
        /// </summary>
        /// <param name="task">a Task of the labels</param>
        /// <returns>the List all labels of a Task</returns>
        public List<Label> GetLabels(Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return GetLabels(task.Id);
        }

        /// <summary>
        /// List all labels of a Task.
        /// </summary>
        /// <param name="taskId">ID of a Task to get labels</param>
        /// <returns>the List all labels of a Task</returns>
        public List<Label> GetLabels(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            return _service.TasksTaskIDLabelsGet(taskId).Labels;
        }

        /// <summary>
        /// Add a Task label.
        /// </summary>
        /// <param name="label">the label of a Task</param>
        /// <param name="task">a Task of a label</param>
        /// <returns>added label</returns>
        public Label AddLabel(Label label, Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));
            Arguments.CheckNotNull(label, nameof(label));

            return AddLabel(label.Id, task.Id);
        }

        /// <summary>
        /// Add a Task label.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="taskId">the ID of a Task</param>
        /// <returns>added label</returns>
        public Label AddLabel(string labelId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var mapping = new LabelMapping(labelId);
            
            return _service.TasksTaskIDLabelsPost(taskId, mapping).Label;
        }

        /// <summary>
        /// Removes a label from a Task.
        /// </summary>
        /// <param name="label">the label of a Task</param>
        /// <param name="task">a Task of a owner</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteLabel(Label label, Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));
            Arguments.CheckNotNull(label, nameof(label));

            DeleteLabel(label.Id, task.Id);
        }

        /// <summary>
        /// Removes a label from a Task.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="taskId">the ID of a Task</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteLabel(string labelId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            _service.TasksTaskIDLabelsLabelIDDelete(taskId, labelId);
        }

        private Task CreateTask(string name, string flux, string every, string cron, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            if (every != null) Arguments.CheckDuration(every, nameof(every));

            var task = new Task(orgId, null, name, Task.StatusEnum.Active, null, null, flux);

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