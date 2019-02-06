using System;
using System.Collections.Generic;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Internal;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Internal;

namespace InfluxDB.Client
{
    public class TasksApi : AbstractInfluxDBClient
    {
        protected internal TasksApi(DefaultClientIo client) : base(client)
        {
        }

        /// <summary>
        /// Creates a new task. The <see cref="Task.Flux"/> has to have defined a cron or a every repetition
        /// by the <a href="http://bit.ly/option-statement">option statement</a>.
        ///
        /// <example>
        /// This sample shows how to specify every repetition
        /// <code>
        /// option task = {
        ///     name: "mean",
        ///     every: 1h,
        /// }
        ///
        /// from(bucket:"metrics/autogen")
        ///     |&gt; range(start:-task.every)
        ///     |&gt; group(columns:["level"])
        ///     |&gt; mean()
        ///     |&gt; yield(name:"mean")
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async System.Threading.Tasks.Task<Task> CreateTask(Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            var request = await Post(task, "/api/v2/tasks");

            return Call<Task>(request);
        }

        /// <summary>
        /// Creates a new task with task repetition by cron. The <see cref="Task.Flux"/> is without a cron or a every repetition.
        /// The repetition is automatically append to the <a href="http://bit.ly/option-statement">option statement</a>.
        /// </summary>
        /// <param name="name">description of the task</param>
        /// <param name="flux">the Flux script to run for this task</param>
        /// <param name="cron">a task repetition schedule in the form '* * * * * *'</param>
        /// <param name="organization">the organization that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async System.Threading.Tasks.Task<Task> CreateTaskCron(string name, string flux, string cron,
            Organization organization)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(cron, nameof(cron));
            Arguments.CheckNotNull(organization, nameof(organization));

            var task = CreateTask(name, flux, null, cron, organization.Id);

            return await CreateTask(task);
        }

        /// <summary>
        /// Creates a new task with task repetition by cron. The <see cref="Task.Flux"/> is without a cron or a every repetition.
        /// The repetition is automatically append to the <a href="http://bit.ly/option-statement">option statement</a>.
        /// </summary>
        /// <param name="name">description of the task</param>
        /// <param name="flux">the Flux script to run for this task</param>
        /// <param name="cron">a task repetition schedule in the form '* * * * * *'</param>
        /// <param name="orgId">the organization ID that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async System.Threading.Tasks.Task<Task> CreateTaskCron(string name, string flux, string cron,
            string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(cron, nameof(cron));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var organization = new Organization {Id = orgId};

            return await CreateTaskCron(name, flux, cron, organization);
        }

        /// <summary>
        /// Creates a new task with task repetition by duration expression ("1h", "30s"). The <see cref="Task.Flux"/> is without a cron or a every repetition.
        /// The repetition is automatically append to the <a href="http://bit.ly/option-statement">option statement</a>.
        /// </summary>
        /// <param name="name">description of the task</param>
        /// <param name="flux">the Flux script to run for this task</param>
        /// <param name="every">a task repetition by duration expression</param>
        /// <param name="organization">the organization that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async System.Threading.Tasks.Task<Task> CreateTaskEvery(string name, string flux, string every,
            Organization organization)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(every, nameof(every));
            Arguments.CheckNotNull(organization, nameof(organization));

            var task = CreateTask(name, flux, every, null, organization.Id);

            return await CreateTask(task);
        }

        /// <summary>
        ///  Creates a new task with task repetition by duration expression ("1h", "30s"). The <see cref="Task.Flux"/> is without a cron or a every repetition.
        /// The repetition is automatically append to the <a href="http://bit.ly/option-statement">option statement</a>.
        /// </summary>
        /// <param name="name">description of the task</param>
        /// <param name="flux">the Flux script to run for this task</param>
        /// <param name="every">a task repetition by duration expression</param>
        /// <param name="orgId">the organization ID that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async System.Threading.Tasks.Task<Task> CreateTaskEvery(string name, string flux, string every,
            string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(every, nameof(every));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var organization = new Organization {Id = orgId};

            return await CreateTaskEvery(name, flux, every, organization);
        }

        /// <summary>
        /// Update a task. This will cancel all queued runs.
        /// </summary>
        /// <param name="task">task update to apply</param>
        /// <returns>task updated</returns>
        public async System.Threading.Tasks.Task<Task> UpdateTask(Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            var result = await Patch(task, $"/api/v2/tasks/{task.Id}");

            return Call<Task>(result);
        }

        /// <summary>
        /// Delete a task.
        /// </summary>
        /// <param name="taskId">ID of task to delete</param>
        /// <returns>async task</returns>
        public async System.Threading.Tasks.Task DeleteTask(string taskId)
        {
            Arguments.CheckNotNull(taskId, nameof(taskId));

            var request = await Delete($"/api/v2/tasks/{taskId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        /// Delete a task.
        /// </summary>
        /// <param name="task">task to delete</param>
        /// <returns>async task</returns>
        public async System.Threading.Tasks.Task DeleteTask(Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            await DeleteTask(task.Id);
        }

        /// <summary>
        /// Retrieve an task.
        /// </summary>
        /// <param name="taskId">ID of task to get</param>
        /// <returns>task details</returns>
        public async System.Threading.Tasks.Task<Task> FindTaskById(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            var request = await Get($"/api/v2/tasks/{taskId}");

            return Call<Task>(request, 404);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <returns>A list of tasks</returns>
        public async System.Threading.Tasks.Task<List<Task>> FindTasks()
        {
            return await FindTasks(null, null, null);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="user">filter tasks to a specific user</param>
        /// <returns>A list of tasks</returns>
        public async System.Threading.Tasks.Task<List<Task>> FindTasksByUser(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return await FindTasksByUserId(user.Id);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="userId">filter tasks to a specific user ID</param>
        /// <returns>A list of tasks</returns>
        public async System.Threading.Tasks.Task<List<Task>> FindTasksByUserId(string userId)
        {
            return await FindTasks(null, userId, null);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="organization">filter tasks to a specific organization</param>
        /// <returns>A list of tasks</returns>
        public async System.Threading.Tasks.Task<List<Task>> FindTasksByOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await FindTasksByOrganizationId(organization.Id);
        }


        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="orgId">filter tasks to a specific organization ID</param>
        /// <returns>A list of tasks</returns>
        public async System.Threading.Tasks.Task<List<Task>> FindTasksByOrganizationId(string orgId)
        {
            return await FindTasks(null, null, orgId);
        }

        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="afterId">returns tasks after specified ID</param>
        /// <param name="userId">filter tasks to a specific user ID</param>
        /// <param name="orgId">filter tasks to a specific organization ID</param>
        /// <returns>A list of tasks</returns>
        public async System.Threading.Tasks.Task<List<Task>> FindTasks(string afterId, string userId,
            string orgId)
        {
            var request = await Get($"/api/v2/tasks?after={afterId}&user={userId}&organization={orgId}");

            var tasks = Call<Tasks>(request);

            return tasks.TaskList;
        }

        /// <summary>
        /// List all members of a task.
        /// </summary>
        /// <param name="task">task of the members</param>
        /// <returns>the List all members of a task</returns>
        public async System.Threading.Tasks.Task<List<ResourceMember>> GetMembers(Task task)
        {
            Arguments.CheckNotNull(task, "task");

            return await GetMembers(task.Id);
        }

        /// <summary>
        /// List all members of a task.
        /// </summary>
        /// <param name="taskId">ID of task to get members</param>
        /// <returns>the List all members of a task</returns>
        public async System.Threading.Tasks.Task<List<ResourceMember>> GetMembers(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            var request = await Get($"/api/v2/tasks/{taskId}/members");

            var response = Call<ResourceMembers>(request);

            return response?.Users;
        }

        /// <summary>
        /// Add a task member.
        /// </summary>
        /// <param name="member">the member of a task</param>
        /// <param name="task">the task of a member</param>
        /// <returns>created mapping</returns>
        public async System.Threading.Tasks.Task<ResourceMember> AddMember(User member, Task task)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(member, "member");

            return await AddMember(member.Id, task.Id);
        }

        /// <summary>
        /// Add a task member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="taskId">the ID of a task</param>
        /// <returns>created mapping</returns>
        public async System.Threading.Tasks.Task<ResourceMember> AddMember(string memberId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            var user = new User {Id = memberId};

            var request = await Post(user, $"/api/v2/tasks/{taskId}/members");

            return Call<ResourceMember>(request);
        }

        /// <summary>
        /// Removes a member from a task.
        /// </summary>
        /// <param name="member">the member of a task</param>
        /// <param name="task">the task of a member</param>
        /// <returns>async task</returns>
        public async System.Threading.Tasks.Task DeleteMember(User member, Task task)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(member, "member");

            await DeleteMember(member.Id, task.Id);
        }

        /// <summary>
        /// Removes a member from a task.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="taskId">the ID of a task</param>
        /// <returns>async task</returns>
        public async System.Threading.Tasks.Task DeleteMember(string memberId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            var request = await Delete($"/api/v2/tasks/{taskId}/members/{memberId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        /// List all owners of a task.
        /// </summary>
        /// <param name="task">task of the owners</param>
        /// <returns>the List all owners of a task</returns>
        public async System.Threading.Tasks.Task<List<ResourceMember>> GetOwners(Task task)
        {
            Arguments.CheckNotNull(task, "Task is required");

            return await GetOwners(task.Id);
        }

        /// <summary>
        /// List all owners of a task.
        /// </summary>
        /// <param name="taskId">ID of a task to get owners</param>
        /// <returns>the List all owners of a task</returns>
        public async System.Threading.Tasks.Task<List<ResourceMember>> GetOwners(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            var request = await Get($"/api/v2/tasks/{taskId}/owners");

            var response = Call<ResourceMembers>(request);

            return response?.Users;
        }

        /// <summary>
        /// Add a task owner.
        /// </summary>
        /// <param name="owner">the owner of a task</param>
        /// <param name="task">the task of a owner</param>
        /// <returns>created mapping</returns>
        public async System.Threading.Tasks.Task<ResourceMember> AddOwner(User owner, Task task)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(owner, "owner");

            return await AddOwner(owner.Id, task.Id);
        }

        /// <summary>
        /// Add a task owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="taskId">the ID of a task</param>
        /// <returns>created mapping</returns>
        public async System.Threading.Tasks.Task<ResourceMember> AddOwner(string ownerId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var user = new User {Id = ownerId};

            var request = await Post(user, $"/api/v2/tasks/{taskId}/owners");

            return Call<ResourceMember>(request);
        }

        /// <summary>
        /// Removes a owner from a task.
        /// </summary>
        /// <param name="owner">the owner of a task</param>
        /// <param name="task">the task of a owner</param>
        /// <returns>async task</returns>
        public async System.Threading.Tasks.Task DeleteOwner(User owner, Task task)
        {
            Arguments.CheckNotNull(task, "task");
            Arguments.CheckNotNull(owner, "owner");

            await DeleteOwner(owner.Id, task.Id);
        }

        /// <summary>
        /// Removes a owner from a task.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="taskId">the ID of a task</param>
        /// <returns>async task</returns>
        public async System.Threading.Tasks.Task DeleteOwner(string ownerId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var request = await Delete($"/api/v2/tasks/{taskId}/owners/{ownerId}");

            RaiseForInfluxError(request);
        }
        
        /// <summary>
        /// Retrieve all logs for a task.
        /// </summary>
        /// <param name="task">task to get logs for</param>
        /// <returns>the list of all logs for a task</returns>
        public async System.Threading.Tasks.Task<List<string>> GetLogs(Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return await GetLogs(task.Id, task.OrgId);
        }

        /// <summary>
        /// Retrieve all logs for a task.
        /// </summary>
        /// <param name="taskId">ID of task to get logs for</param>
        /// <param name="orgId">ID of organization to get logs for</param>
        /// <returns>the list of all logs for a task</returns>
        public async System.Threading.Tasks.Task<List<string>> GetLogs(string taskId, string orgId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            
            var request = await Get($"/api/v2/tasks/{taskId}/logs?orgID={orgId}");

            return Call<List<string>>(request, "task not found");
        }

        /// <summary>
        /// Retrieve list of run records for a task.
        /// </summary>
        /// <param name="task"> task to get runs for</param>
        /// <returns>the list of run records for a task</returns>
        public async System.Threading.Tasks.Task<List<Run>> GetRuns(Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return await GetRuns(task, null, null, null);
        }


        /// <summary>
        /// Retrieve list of run records for a task.
        /// </summary>
        /// <param name="task"> task to get runs for</param>
        /// <param name="afterTime">filter runs to those scheduled after this time</param>
        /// <param name="beforeTime">filter runs to those scheduled before this time</param>
        /// <param name="limit">the number of runs to return. Default value: 20.</param>
        /// <returns>the list of run records for a task</returns>
        public async System.Threading.Tasks.Task<List<Run>> GetRuns(Task task, DateTime? afterTime,
            DateTime? beforeTime, int? limit)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return await GetRuns(task.Id, task.OrgId, afterTime, beforeTime, limit);
        }

        /// <summary>
        /// Retrieve list of run records for a task.
        /// </summary>
        /// <param name="taskId">ID of task to get runs for</param>
        /// <param name="orgId">ID of organization</param>
        /// <returns>the list of run records for a task</returns>
        public async System.Threading.Tasks.Task<List<Run>> GetRuns(string taskId, string orgId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return await GetRuns(taskId, orgId, null, null, null);
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
        public async System.Threading.Tasks.Task<List<Run>> GetRuns(string taskId, string orgId,
            DateTime? afterTime, DateTime? beforeTime, int? limit)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            const string format = "yyyy-MM-dd'T'HH:mm:ss.fffZ";
            
            var after = afterTime?.ToString(format);
            var before = beforeTime?.ToString(format);
            
            var path = $"/api/v2/tasks/{taskId}/runs?afterTime={after}&beforeTime={before}&orgID={orgId}&limit={limit}";
            var request = await Get(path);
            
            var response = Call<Runs>(request);

            return response?.RunList;
        }

        /// <summary>
        /// Retrieve a single run record for a task.
        /// </summary>
        /// <param name="taskId">ID of task to get runs for</param>
        /// <param name="runId">ID of run</param>
        /// <returns>a single run record for a task</returns>
        public async System.Threading.Tasks.Task<Run> GetRun(string taskId, string runId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));

            var request = await Get($"/api/v2/tasks/{taskId}/runs/{runId}");

            return Call<Run>(request, 404);
        }

        /// <summary>
        /// Retry a task run.
        /// </summary>
        /// <param name="run">the run to retry</param>
        /// <returns>the executed run</returns>
        public async System.Threading.Tasks.Task<Run> RetryRun(Run run)
        {
            Arguments.CheckNotNull(run, nameof(run));

            return await RetryRun(run.TaskId, run.Id);
        }
        
        /// <summary>
        /// Retry a task run.
        /// </summary>
        /// <param name="taskId">ID of task with the run to retry</param>
        /// <param name="runId">ID of run to retry</param>
        /// <returns>the executed run</returns>
        public async System.Threading.Tasks.Task<Run> RetryRun(string taskId, string runId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));
            
            var request = await Post($"/api/v2/tasks/{taskId}/runs/{runId}/retry");

            return Call<Run>(request, 404);
        }
        
        /// <summary>
        /// Cancels a currently running run.
        /// </summary>
        /// <param name="run">the run to cancel</param>
        /// <returns>async task</returns>
        public async System.Threading.Tasks.Task CancelRun(Run run)
        {
            Arguments.CheckNotNull(run, nameof(run));

            await CancelRun(run.TaskId, run.Id);
        }
        
        /// <summary>
        /// Cancels a currently running run.
        /// </summary>
        /// <param name="taskId">ID of task with the run to cancel</param>
        /// <param name="runId">ID of run to cancel</param>
        /// <returns>async task</returns>
        public async System.Threading.Tasks.Task CancelRun(string taskId, string runId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));
            
            var request = await Delete($"/api/v2/tasks/{taskId}/runs/{runId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        /// Retrieve all logs for a run.
        /// </summary>
        /// <param name="run">the run to gets logs for it</param>
        /// <param name="orgId">ID of organization to get logs for it</param>
        /// <returns>the list of all logs for a run</returns>
        public async System.Threading.Tasks.Task<List<string>> GetRunLogs(Run run, string orgId)
        {
            return await GetRunLogs(run.TaskId, run.Id, orgId);
        }
        
        /// <summary>
        /// Retrieve all logs for a run.
        /// </summary>
        /// <param name="taskId">ID of task to get run logs for it</param>
        /// <param name="runId">ID of run to get logs for it</param>
        /// <param name="orgId">ID of organization to get logs for it</param>
        /// <returns>the list of all logs for a run</returns>
        public async System.Threading.Tasks.Task<List<string>> GetRunLogs(string taskId, string runId, string orgId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(runId, nameof(runId));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var request = await Get($"/api/v2/tasks/{taskId}/runs/{runId}/logs?orgID={orgId}");

            return Call<List<string>>(request);
        }

        /// <summary>
        /// List all labels of a Task.
        /// </summary>
        /// <param name="task">a Task of the labels</param>
        /// <returns>the List all labels of a Task</returns>
        public async System.Threading.Tasks.Task<List<Label>> GetLabels(Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return await GetLabels(task.Id);
        }

        /// <summary>
        /// List all labels of a Task.
        /// </summary>
        /// <param name="taskId">ID of a Task to get labels</param>
        /// <returns>the List all labels of a Task</returns>
        public async System.Threading.Tasks.Task<List<Label>> GetLabels(string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));

            return await GetLabels(taskId, "tasks");
        }

        /// <summary>
        /// Add a Task label.
        /// </summary>
        /// <param name="label">the label of a Task</param>
        /// <param name="task">a Task of a label</param>
        /// <returns>added label</returns>
        public async System.Threading.Tasks.Task<Label> AddLabel(Label label, Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));
            Arguments.CheckNotNull(label, nameof(label));

            return await AddLabel(label.Id, task.Id);
        }

        /// <summary>
        /// Add a Task label.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="taskId">the ID of a Task</param>
        /// <returns>added label</returns>
        public async System.Threading.Tasks.Task<Label> AddLabel(string labelId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return await AddLabel(labelId, taskId, "tasks", ResourceType.Tasks);
        }

        /// <summary>
        /// Removes a label from a Task.
        /// </summary>
        /// <param name="label">the label of a Task</param>
        /// <param name="task">a Task of a owner</param>
        /// <returns>async task</returns>
        public async System.Threading.Tasks.Task DeleteLabel(Label label, Task task)
        {
            Arguments.CheckNotNull(task, nameof(task));
            Arguments.CheckNotNull(label, nameof(label));

            await DeleteLabel(label.Id, task.Id);
        }

        /// <summary>
        /// Removes a label from a Task.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="taskId">the ID of a Task</param>
        /// <returns>async task</returns>
        public async System.Threading.Tasks.Task DeleteLabel(string labelId, string taskId)
        {
            Arguments.CheckNonEmptyString(taskId, nameof(taskId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            await DeleteLabel(labelId, taskId, "tasks");
        }
        
        private Task CreateTask(string name, string flux, string every, string cron, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            if (every != null)
            {
                Arguments.CheckDuration(every, nameof(every));
            }

            var task = new Task
            {
                Name = name,
                OrgId = orgId,
                Status = Status.Active,
                Every = every,
                Cron = cron,
                Flux = flux
            };

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