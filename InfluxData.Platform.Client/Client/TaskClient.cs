using System;
using System.Collections.Generic;
using InfluxData.Platform.Client.Domain;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;

namespace InfluxData.Platform.Client.Client
{
    public class TaskClient : AbstractClient
    {
        protected internal TaskClient(DefaultClientIo client) : base(client)
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
        /// <param name="user">the user that owns this Task</param>
        /// <param name="organization">the organization that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async System.Threading.Tasks.Task<Task> CreateTaskCron(string name, string flux, string cron, User user,
            Organization organization)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(cron, nameof(cron));
            Arguments.CheckNotNull(user, nameof(user));
            Arguments.CheckNotNull(organization, nameof(organization));

            var task = CreateTask(name, flux, null, cron, user, organization.Id);

            return await CreateTask(task);
        }

        /// <summary>
        /// Creates a new task with task repetition by cron. The <see cref="Task.Flux"/> is without a cron or a every repetition.
        /// The repetition is automatically append to the <a href="http://bit.ly/option-statement">option statement</a>.
        /// </summary>
        /// <param name="name">description of the task</param>
        /// <param name="flux">the Flux script to run for this task</param>
        /// <param name="cron">a task repetition schedule in the form '* * * * * *'</param>
        /// <param name="userId">the user ID that owns this Task</param>
        /// <param name="organizationId">the organization ID that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async System.Threading.Tasks.Task<Task> CreateTaskCron(string name, string flux, string cron,
            string userId, string organizationId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(cron, nameof(cron));
            Arguments.CheckNonEmptyString(userId, nameof(userId));
            Arguments.CheckNonEmptyString(organizationId, nameof(organizationId));

            var user = new User {Id = userId};
            var organization = new Organization {Id = organizationId};

            return await CreateTaskCron(name, flux, cron, user, organization);
        }

        /// <summary>
        /// Creates a new task with task repetition by duration expression ("1h", "30s"). The <see cref="Task.Flux"/> is without a cron or a every repetition.
        /// The repetition is automatically append to the <a href="http://bit.ly/option-statement">option statement</a>.
        /// </summary>
        /// <param name="name">description of the task</param>
        /// <param name="flux">the Flux script to run for this task</param>
        /// <param name="every">a task repetition by duration expression</param>
        /// <param name="user">the user that owns this Task</param>
        /// <param name="organization">the organization that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async System.Threading.Tasks.Task<Task> CreateTaskEvery(string name, string flux, string every,
            User user, Organization organization)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(every, nameof(every));
            Arguments.CheckNotNull(user, nameof(user));
            Arguments.CheckNotNull(organization, nameof(organization));

            var task = CreateTask(name, flux, every, null, user, organization.Id);

            return await CreateTask(task);
        }

        /// <summary>
        ///  Creates a new task with task repetition by duration expression ("1h", "30s"). The <see cref="Task.Flux"/> is without a cron or a every repetition.
        /// The repetition is automatically append to the <a href="http://bit.ly/option-statement">option statement</a>.
        /// </summary>
        /// <param name="name">description of the task</param>
        /// <param name="flux">the Flux script to run for this task</param>
        /// <param name="every">a task repetition by duration expression</param>
        /// <param name="userId">the user ID that owns this Task</param>
        /// <param name="organizationId">the organization ID that owns this Task</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async System.Threading.Tasks.Task<Task> CreateTaskEvery(string name, string flux, string every,
            string userId, string organizationId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNonEmptyString(every, nameof(every));
            Arguments.CheckNonEmptyString(userId, nameof(userId));
            Arguments.CheckNonEmptyString(organizationId, nameof(organizationId));

            var user = new User {Id = userId};
            var organization = new Organization {Id = organizationId};

            return await CreateTaskEvery(name, flux, every, user, organization);
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

            return Call<Task>(request, "task not found");
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
        /// <param name="organizationId">filter tasks to a specific organization ID</param>
        /// <returns>A list of tasks</returns>
        public async System.Threading.Tasks.Task<List<Task>> FindTasksByOrganizationId(string organizationId)
        {
            return await FindTasks(null, null, organizationId);
        }
        
        /// <summary>
        /// Lists tasks, limit 100.
        /// </summary>
        /// <param name="afterId">returns tasks after specified ID</param>
        /// <param name="userId">filter tasks to a specific user ID</param>
        /// <param name="organizationId">filter tasks to a specific organization ID</param>
        /// <returns>A list of tasks</returns>
        public async System.Threading.Tasks.Task<List<Task>> FindTasks(string afterId, string userId, string organizationId)
        {
            var request = await Get($"/api/v2/tasks?after={afterId}&user={userId}&organization={organizationId}");
            
            var tasks = Call<Tasks>(request);

            return tasks.TaskList;
        }

        private Task CreateTask(string name, string flux, string every, string cron, User user, String organizationId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(flux, nameof(flux));
            Arguments.CheckNotNull(user, nameof(user));
            Arguments.CheckNonEmptyString(organizationId, nameof(organizationId));

            if (every != null)
            {
                Arguments.CheckDuration(every, nameof(every));
            }

            Task task = new Task
            {
                Name = name,
                OrganizationId = organizationId,
                Owner = user,
                Status = Status.Active,
                Every = every,
                Cron = cron,
                Flux = flux
            };

            String repetition = "";
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