using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    [Ignore("https://github.com/influxdata/influxdb/issues/13576")]
    public class ItTasksApiTest : AbstractItClientTest
    {
        [SetUp]
        public new async Task SetUp()
        {
            _organization = await FindMyOrg();

            var authorization = await AddAuthorization(_organization);

            Client.Dispose();
            Client = InfluxDBClientFactory.Create(InfluxDbUrl, authorization.Token.ToCharArray());

            _tasksApi = Client.GetTasksApi();

            _usersApi = Client.GetUsersApi();

            (await _tasksApi.FindTasks()).ForEach(async task => await _tasksApi.DeleteTask(task));
        }

        private const string TaskFlux = "from(bucket: \"my-bucket\")\n\t|> range(start: 0)\n\t|> last()";
        
        private TasksApi _tasksApi;
        private UsersApi _usersApi;

        private Organization _organization;

        private async Task<Authorization> AddAuthorization(Organization organization)
        {
            var resourceTask = new PermissionResource(PermissionResource.TypeEnum.Tasks, null, null, organization.Id);
            var resourceBucket = new PermissionResource(PermissionResource.TypeEnum.Buckets,
                (await Client.GetBucketsApi().FindBucketByName("my-bucket")).Id, null, organization.Id);
            var resourceOrg = new PermissionResource(PermissionResource.TypeEnum.Orgs);
            var resourceUser = new PermissionResource(PermissionResource.TypeEnum.Users);
            var resourceAuthorization = new PermissionResource(PermissionResource.TypeEnum.Authorizations);


            var authorization = await Client.GetAuthorizationsApi()
                .CreateAuthorization(organization, new List<Permission>
                {
                    new Permission(Permission.ActionEnum.Read, resourceTask),
                    new Permission(Permission.ActionEnum.Write, resourceTask),
                    new Permission(Permission.ActionEnum.Read, resourceOrg),
                    new Permission(Permission.ActionEnum.Write, resourceOrg),
                    new Permission(Permission.ActionEnum.Write, resourceUser),
                    new Permission(Permission.ActionEnum.Write, resourceAuthorization),
                    new Permission(Permission.ActionEnum.Read, resourceBucket),
                    new Permission(Permission.ActionEnum.Write, resourceBucket)
                });

            return authorization;
        }

        [Test]
        public async Task CancelRunNotExist()
        {
            var task = await _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            Thread.Sleep(5_000);

            var runs = await _tasksApi.GetRuns(task, null, null, 1);
            Assert.IsNotEmpty(runs);

            var message = Assert.ThrowsAsync<HttpException>(async () => await _tasksApi.CancelRun(runs[0]))
                .ErrorBody["error"].ToString();

            Assert.AreEqual(message, "run not found");
        }

        [Test]
        public void CancelRunTaskNotExist()
        {
            var message = Assert.ThrowsAsync<HttpException>(async () =>
                await _tasksApi.CancelRun("020f755c3c082000", "020f755c3c082000")).ErrorBody["error"].ToString();

            Assert.AreEqual(message, "task not found");
        }

        [Test]
        public async Task CloneTask()
        {
            var source = await _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = await Client.GetLabelsApi()
                .CreateLabel(GenerateName("Cool Resource"), properties, _organization.Id);
            await _tasksApi.AddLabel(label, source);

            var cloned = await _tasksApi.CloneTask(source);

            Assert.AreEqual(source.Name, cloned.Name);
            Assert.AreEqual(_organization.Id, cloned.OrgID);
            Assert.AreEqual(source.Flux, cloned.Flux);
            Assert.AreEqual("1s", cloned.Every);
            Assert.IsNull(cloned.Cron);
            Assert.IsNull(cloned.Offset);

            var labels = await _tasksApi.GetLabels(cloned);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
        }

        [Test]
        public void CloneTaskNotFound()
        {
            var ioe = Assert.ThrowsAsync<HttpException>(async () => await _tasksApi.CloneTask("020f755c3c082000"));

            Assert.AreEqual("failed to find task", ioe.Message);
        }

        [Test]
        public async Task CreateTask()
        {
            var taskName = GenerateName("it task");

            var flux = $"option task = {{\nname: \"{taskName}\",\nevery: 1h\n}}\n\n{TaskFlux}";

            var task = new Api.Domain.Task(_organization.Id, _organization.Name,
                taskName, "testing task", Api.Domain.Task.StatusEnum.Active, null, null, flux);

            task = await _tasksApi.CreateTask(task);

            Assert.IsNotNull(task);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(taskName, task.Name);
            Assert.AreEqual(_organization.Id, task.OrgID);
            Assert.AreEqual(Api.Domain.Task.StatusEnum.Active, task.Status);
            Assert.AreEqual("1h", task.Every);
            Assert.AreEqual("testing task", task.Description);
            Assert.IsNull(task.Cron);
            Assert.IsTrue(task.Flux.Equals(flux, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async Task CreateTaskCron()
        {
            var taskName = GenerateName("it task");

            var task =
                await _tasksApi.CreateTaskCron(taskName, TaskFlux, "0 2 * * *", _organization);

            Assert.IsNotNull(task);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(taskName, task.Name);
            Assert.AreEqual(_organization.Id, task.OrgID);
            Assert.AreEqual(Api.Domain.Task.StatusEnum.Active, task.Status);
            Assert.AreEqual("0 2 * * *", task.Cron);
            Assert.IsNull(task.Every);
            Assert.IsTrue(task.Flux.EndsWith(TaskFlux, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async Task CreateTaskEvery()
        {
            var taskName = GenerateName("it task");

            var task =
                await _tasksApi.CreateTaskEvery(taskName, TaskFlux, "1h", _organization);

            Assert.IsNotNull(task);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(taskName, task.Name);
            Assert.AreEqual(_organization.Id, task.OrgID);
            Assert.AreEqual(Api.Domain.Task.StatusEnum.Active, task.Status);
            Assert.AreEqual("1h", task.Every);
            Assert.IsNull(task.Cron);
            Assert.IsTrue(task.Flux.EndsWith(TaskFlux, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async Task CreateTaskWithOffset()
        {
            Client.SetLogLevel(LogLevel.Body);

            var taskName = GenerateName("it task");

            var flux = $"option task = {{\nname: \"{taskName}\",\nevery: 1h,\noffset: 30m\n}}\n\n{TaskFlux}";

            var task = new Api.Domain.Task(_organization.Id, _organization.Name, taskName,
                null, Api.Domain.Task.StatusEnum.Active, null, null, flux);

            task = await _tasksApi.CreateTask(task);

            Assert.IsNotNull(task);
            Assert.AreEqual("30m", task.Offset);
        }

        [Test]
        public async Task DeleteTask()
        {
            var task = await _tasksApi.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _organization);

            var foundTask = await _tasksApi.FindTaskById(task.Id);
            Assert.IsNotNull(foundTask);

            await _tasksApi.DeleteTask(task);

            var ioe = Assert.ThrowsAsync<HttpException>(async () => await _tasksApi.FindTaskById(task.Id));

            Assert.AreEqual("failed to find task", ioe.Message);
        }

        [Test]
        public async Task FindTaskById()
        {
            var taskName = GenerateName("it task");

            var task = await _tasksApi.CreateTaskCron(taskName, TaskFlux, "0 2 * * *", _organization);

            var taskById = await _tasksApi.FindTaskById(task.Id);

            Assert.IsNotNull(taskById);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(task.Id, taskById.Id);
            Assert.AreEqual(task.Name, taskById.Name);
            Assert.AreEqual(task.OrgID, taskById.OrgID);
            Assert.AreEqual(task.Status, taskById.Status);
            Assert.AreEqual(task.Offset, taskById.Offset);
            Assert.AreEqual(task.Flux, taskById.Flux);
            Assert.AreEqual(task.Cron, taskById.Cron);
            Assert.IsNotNull(taskById.CreatedAt);
        }

        [Test]
        public void FindTaskByIdNull()
        {
            var ioe = Assert.ThrowsAsync<HttpException>(async () => await _tasksApi.FindTaskById("020f755c3d082000"));

            Assert.AreEqual("failed to find task", ioe.Message);
        }

        [Test]
        public async Task FindTasks()
        {
            var count = (await _tasksApi.FindTasks()).Count;

            var task = await _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "2h", _organization.Id);
            Assert.IsNotNull(task);

            var tasks = await _tasksApi.FindTasks();

            Assert.AreEqual(count + 1, tasks.Count);
        }

        [Test]
        public async Task FindTasksAfterSpecifiedId()
        {
            var task1 = await _tasksApi.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _organization);
            var task2 = await _tasksApi.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _organization);

            var tasks = await _tasksApi.FindTasks(task1.Id);

            Assert.AreEqual(1, tasks.Count);
            Assert.AreEqual(task2.Id, tasks[0].Id);
        }

        [Test]
        [Ignore("https://github.com/influxdata/influxdb/issues/11491")]
        //TODO
        public async Task FindTasksByOrganization()
        {
            var taskOrg = await Client.GetOrganizationsApi().CreateOrganization(GenerateName("Task user"));
            var authorization = await AddAuthorization(taskOrg);

            Client.Dispose();
            Client = InfluxDBClientFactory.Create(InfluxDbUrl, authorization.Token.ToCharArray());
            _tasksApi = Client.GetTasksApi();

            var count = (await _tasksApi.FindTasksByOrganization(taskOrg)).Count;
            Assert.AreEqual(0, count);

            await _tasksApi.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", taskOrg);

            var tasks = await _tasksApi.FindTasksByOrganization(taskOrg);

            Assert.AreEqual(1, tasks.Count);

            (await _tasksApi.FindTasks()).ForEach(async task => await _tasksApi.DeleteTask(task));
        }

        [Test]
        //TODO
        [Ignore("set user password -> https://github.com/influxdata/influxdb/issues/11590")]
        public async Task FindTasksByUser()
        {
            var taskUser = await Client.GetUsersApi().CreateUser(GenerateName("Task user"));

            var count = (await _tasksApi.FindTasksByUser(taskUser)).Count;
            Assert.AreEqual(0, count);

            await _tasksApi.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _organization);

            var tasks = await _tasksApi.FindTasksByUser(taskUser);

            Assert.AreEqual(1, tasks.Count);
        }

        [Test]
        public async Task GetLogs()
        {
            var task = await _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization.Id);

            Thread.Sleep(5_000);

            var logs = await _tasksApi.GetLogs(task);
            Assert.IsNotEmpty(logs);
            Assert.IsTrue(logs.First().Message.StartsWith("Started task from script:"));
            Assert.IsTrue(logs.Last().Message.EndsWith("Completed successfully"));
        }

        [Test]
        public void GetLogsNotExist()
        {
            var ioe = Assert.ThrowsAsync<HttpException>(async () => await _tasksApi.GetLogs("020f755c3c082000"));

            Assert.AreEqual("failed to find task logs", ioe.Message);
        }

        [Test]
        public async Task GetRun()
        {
            var task = await _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);
            Thread.Sleep(5_000);

            var runs = await _tasksApi.GetRuns(task, null, null, 1);
            Assert.AreEqual(1, runs.Count);

            var firstRun = runs[0];
            var runById = await _tasksApi.GetRun(task.Id, firstRun.Id);

            Assert.IsNotNull(runById);
            Assert.AreEqual(firstRun.Id, runById.Id);
        }

        [Test]
        public async Task GetRunLogs()
        {
            var task = await _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            Thread.Sleep(4_000);

            var runs = await _tasksApi.GetRuns(task, null, null, 1);
            Assert.AreEqual(1, runs.Count);

            var logs = await _tasksApi.GetRunLogs(runs[0], _organization.Id);
            Assert.IsNotNull(logs);
            Assert.IsTrue(logs.Last().Message.EndsWith("Completed successfully"));
        }

        [Test]
        public async Task GetRunLogsNotExist()
        {
            var task = await _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            var logs = await _tasksApi.GetRunLogs(task.Id, "020f755c3c082000", _organization.Id);
            Assert.IsEmpty(logs);
        }

        [Test]
        public async Task Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var task = await _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = await labelClient.CreateLabel(GenerateName("Cool Resource"), properties, _organization.Id);

            var labels = await _tasksApi.GetLabels(task);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = await _tasksApi.AddLabel(label, task);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = await _tasksApi.GetLabels(task);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            task = await _tasksApi.FindTaskById(task.Id);
            Assert.IsNotNull(task);
            Assert.AreEqual(1, task.Labels.Count);
            Assert.AreEqual(label.Id, task.Labels[0].Id);
            Assert.AreEqual(label.Name, task.Labels[0].Name);

            await _tasksApi.DeleteLabel(label, task);

            labels = await _tasksApi.GetLabels(task);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        [Ignore("https://github.com/influxdata/influxdb/issues/11491")]
        //TODO
        public async Task Member()
        {
            var task = await _tasksApi.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _organization);

            var members = await _tasksApi.GetMembers(task);
            Assert.AreEqual(0, members.Count);

            var user = await _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _tasksApi.AddMember(user, task);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.RoleEnum.Member);

            members = await _tasksApi.GetMembers(task);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].Id, user.Id);
            Assert.AreEqual(members[0].Name, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.RoleEnum.Member);
            await _tasksApi.DeleteMember(user, task);

            members = await _tasksApi.GetMembers(task);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        [Ignore("https://github.com/influxdata/influxdb/issues/11491")]
        //TODO
        public async Task Owner()
        {
            var task = await _tasksApi.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _organization.Id);

            var owners = await _tasksApi.GetOwners(task);
            Assert.AreEqual(0, owners.Count);

            var user = await _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _tasksApi.AddOwner(user, task);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceOwner.RoleEnum.Owner);

            owners = await _tasksApi.GetOwners(task);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual(owners[0].Id, user.Id);
            Assert.AreEqual(owners[0].Name, user.Name);
            Assert.AreEqual(owners[0].Role, ResourceOwner.RoleEnum.Owner);

            await _tasksApi.DeleteOwner(user, task);

            owners = await _tasksApi.GetOwners(task);
            Assert.AreEqual(0, owners.Count);
        }

        [Test]
        public async Task RetryRun()
        {
            var task = await _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);
            Thread.Sleep(5_000);

            var runs = await _tasksApi.GetRuns(task, null, null, 1);
            Assert.AreEqual(1, runs.Count);

            var run = runs[0];

            var retriedRun = await _tasksApi.RetryRun(run);

            Assert.IsNotNull(retriedRun);
            Assert.AreEqual(run.TaskID, retriedRun.TaskID);
            Assert.AreEqual(Run.StatusEnum.Scheduled, retriedRun.Status);
            Assert.AreEqual(task.Id, retriedRun.TaskID);
        }

        [Test]
        public async Task RetryRunNotExist()
        {
            var task = await _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            var ioe = Assert.ThrowsAsync<HttpException>(async () =>
                await _tasksApi.RetryRun(task.Id, "020f755c3c082000"));

            Assert.AreEqual("failed to retry run", ioe.Message);
        }

        [Test]
        public async Task RunNotExist()
        {
            var task = await _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            var ioe = Assert.ThrowsAsync<HttpException>(async () =>
                await _tasksApi.GetRun(task.Id, "020f755c3c082000"));

            Assert.AreEqual("failed to find run", ioe.Message);
        }

        [Test]
        public async Task Runs()
        {
            var task = await _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization.Id);

            Thread.Sleep(5_000);

            var runs = await _tasksApi.GetRuns(task);
            Assert.IsNotEmpty(runs);

            Assert.IsNotEmpty(runs[0].Id);
            Assert.AreEqual(task.Id, runs[0].TaskID);
            Assert.AreEqual(Run.StatusEnum.Success, runs[0].Status);
            Assert.Greater(DateTime.Now, runs[0].StartedAt);
            Assert.Greater(DateTime.Now, runs[0].FinishedAt);
            Assert.Greater(DateTime.Now, runs[0].ScheduledFor);
            Assert.IsNull(runs[0].RequestedAt);

            task = await _tasksApi.FindTaskById(task.Id);
            Assert.IsNotNull(task.LatestCompleted);
        }

        [Test]
        public async Task RunsByTime()
        {
            var now = DateTime.UtcNow;

            var task = await _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization.Id);

            Thread.Sleep(5_000);

            var runs = await _tasksApi.GetRuns(task, null, now, null);
            Assert.IsEmpty(runs);

            runs = await _tasksApi.GetRuns(task, now, null, null);
            Assert.IsNotEmpty(runs);
        }

        [Test]
        public async Task RunsLimit()
        {
            var task = await _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            Thread.Sleep(5_000);

            var runs = await _tasksApi.GetRuns(task, null, null, 1);
            Assert.AreEqual(1, runs.Count);

            runs = await _tasksApi.GetRuns(task, null, null, null);
            Assert.Greater(runs.Count, 1);
        }

        [Test]
        public void RunsNotExist()
        {
            var ioe = Assert.ThrowsAsync<HttpException>(async () =>
                await _tasksApi.GetRuns("020f755c3c082000", _organization.Id));

            Assert.AreEqual("failed to find runs", ioe.Message);
        }

        [Test]
        public async Task UpdateTask()
        {
            var taskName = GenerateName("it task");

            var cronTask =
                await _tasksApi.CreateTaskCron(taskName, TaskFlux, "0 2 * * *", _organization);

            var flux = $"option task = {{name: \"{taskName}\", every: 3m}}\n\n{TaskFlux}";

            cronTask.Every = "3m";
            cronTask.Cron = null;
            cronTask.Status = Api.Domain.Task.StatusEnum.Inactive;

            var updatedTask = await _tasksApi.UpdateTask(cronTask);

            Assert.IsNotNull(updatedTask);
            Assert.IsNotEmpty(updatedTask.Id);
            Assert.AreEqual(taskName, updatedTask.Name);
            Assert.AreEqual(_organization.Id, updatedTask.OrgID);
            Assert.AreEqual(Api.Domain.Task.StatusEnum.Inactive, updatedTask.Status);
            Assert.AreEqual("3m", updatedTask.Every);
            Assert.IsNull(updatedTask.Cron);
            Assert.AreEqual(updatedTask.Flux, flux);
            Assert.IsNotNull(updatedTask.UpdatedAt);
        }
    }
}