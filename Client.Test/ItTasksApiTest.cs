using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItTasksApiTest : AbstractItClientTest
    {
        [SetUp]
        public new async Task SetUp()
        {
            _organization = await FindMyOrg();

            var authorization = await AddAuthorization(_organization);

            Client.Dispose();
            Client = InfluxDBClientFactory.Create(InfluxDbUrl, authorization.Token);

            _tasksApi = Client.GetTasksApi();

            _usersApi = Client.GetUsersApi();

            foreach (var task in (await _tasksApi.FindTasksAsync()).Where(task => task.Name.EndsWith("-IT")))
                await _tasksApi.DeleteTaskAsync(task);

            var organizationsApi = Client.GetOrganizationsApi();
            foreach (var org in (await organizationsApi.FindOrganizationsAsync()).Where(org =>
                         org.Name.EndsWith("-IT")))
                await organizationsApi.DeleteOrganizationAsync(org);
        }

        private const string TaskFlux = "from(bucket: \"my-bucket\")\n\t|> range(start: 0)\n\t|> last()";

        private TasksApi _tasksApi;
        private UsersApi _usersApi;

        private Organization _organization;

        private async Task<Authorization> AddAuthorization(Organization organization)
        {
            var resourceTask = new PermissionResource(PermissionResource.TypeTasks, null, null, organization.Id);
            var resourceBucket = new PermissionResource(PermissionResource.TypeBuckets,
                (await Client.GetBucketsApi().FindBucketByNameAsync("my-bucket")).Id, null, organization.Id);
            var resourceOrg = new PermissionResource(PermissionResource.TypeOrgs);
            var resourceUser = new PermissionResource(PermissionResource.TypeUsers);
            var resourceAuthorization = new PermissionResource(PermissionResource.TypeAuthorizations);
            var resourceLabels = new PermissionResource(PermissionResource.TypeLabels);


            var authorization = await Client.GetAuthorizationsApi()
                .CreateAuthorizationAsync(organization, new List<Permission>
                {
                    new Permission(Permission.ActionEnum.Read, resourceTask),
                    new Permission(Permission.ActionEnum.Write, resourceTask),
                    new Permission(Permission.ActionEnum.Read, resourceOrg),
                    new Permission(Permission.ActionEnum.Write, resourceOrg),
                    new Permission(Permission.ActionEnum.Write, resourceUser),
                    new Permission(Permission.ActionEnum.Write, resourceAuthorization),
                    new Permission(Permission.ActionEnum.Read, resourceBucket),
                    new Permission(Permission.ActionEnum.Write, resourceBucket),
                    new Permission(Permission.ActionEnum.Read, resourceLabels),
                    new Permission(Permission.ActionEnum.Write, resourceLabels)
                });

            return authorization;
        }

        [Test]
        public async Task CancelRunNotExist()
        {
            var task = await _tasksApi.CreateTaskEveryAsync(GenerateName("it task"), TaskFlux, "1s", _organization);

            Thread.Sleep(5_000);

            var runs = await _tasksApi.GetRunsAsync(task, null, null, null);
            Assert.IsNotEmpty(runs);

            var message = Assert
                .ThrowsAsync<NotFoundException>(async () => await _tasksApi.CancelRunAsync(runs.First()))
                ?.Message;

            Assert.AreEqual("failed to cancel run: run not found", message);
        }

        [Test]
        public void CancelRunTaskNotExist()
        {
            var message = Assert.ThrowsAsync<NotFoundException>(async () =>
                    await _tasksApi.CancelRunAsync("020f755c3c082000", "020f755c3c082000"))
                ?.Message;

            Assert.AreEqual("failed to cancel run: task not found", message);
        }

        [Test]
        public async Task CloneTask()
        {
            var source = await _tasksApi.CreateTaskEveryAsync(GenerateName("it task"), TaskFlux, "1s", _organization);

            var properties = new Dictionary<string, string> { { "color", "green" }, { "location", "west" } };

            var label = await Client.GetLabelsApi()
                .CreateLabelAsync(GenerateName("Cool Resource"), properties, _organization.Id);
            await _tasksApi.AddLabelAsync(label, source);

            var cloned = await _tasksApi.CloneTaskAsync(source);

            Assert.AreEqual(source.Name, cloned.Name);
            Assert.AreEqual(_organization.Id, cloned.OrgID);
            Assert.AreEqual(source.Flux, cloned.Flux);
            Assert.AreEqual("1s", cloned.Every);
            Assert.IsNull(cloned.Cron);
            Assert.IsNull(cloned.Offset);

            var labels = await _tasksApi.GetLabelsAsync(cloned);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
        }

        [Test]
        public void CloneTaskNotFound()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _tasksApi.CloneTaskAsync("020f755c3c082000"));

            Assert.AreEqual("failed to find task: task not found", ioe?.Message);
        }

        [Test]
        public async Task CreateTask()
        {
            var taskName = GenerateName("it task");

            var flux = $"option task = {{\nname: \"{taskName}\",\nevery: 1h\n}}\n\n{TaskFlux}";

            var task = new TaskType(orgID: _organization.Id, org: _organization.Name,
                name: taskName, description: "testing task", status: TaskStatusType.Active, flux: flux);

            task = await _tasksApi.CreateTaskAsync(task);

            Assert.IsNotNull(task);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(taskName, task.Name);
            Assert.AreEqual(_organization.Id, task.OrgID);
            Assert.AreEqual(TaskStatusType.Active, task.Status);
            Assert.AreEqual("1h", task.Every);
            Assert.AreEqual("testing task", task.Description);
            Assert.IsNull(task.Cron);
            Assert.That(task.Flux, Is.EqualTo(flux).IgnoreCase);
        }

        [Test]
        public async Task CreateTaskCron()
        {
            var taskName = GenerateName("it task");

            var task =
                await _tasksApi.CreateTaskCronAsync(taskName, TaskFlux, "0 2 * * *", _organization);

            Assert.IsNotNull(task);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(taskName, task.Name);
            Assert.AreEqual(_organization.Id, task.OrgID);
            Assert.AreEqual(TaskStatusType.Active, task.Status);
            Assert.AreEqual("0 2 * * *", task.Cron);
            Assert.IsNull(task.Every);
            Assert.IsTrue(task.Flux.EndsWith(TaskFlux, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async Task CreateTaskEvery()
        {
            var taskName = GenerateName("it task");

            var task =
                await _tasksApi.CreateTaskEveryAsync(taskName, TaskFlux, "1h", _organization);

            Assert.IsNotNull(task);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(taskName, task.Name);
            Assert.AreEqual(_organization.Id, task.OrgID);
            Assert.AreEqual(TaskStatusType.Active, task.Status);
            Assert.AreEqual("1h", task.Every);
            Assert.IsNull(task.Cron);
            Assert.IsTrue(task.Flux.EndsWith(TaskFlux, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async Task CreateTaskWithOffset()
        {
            var taskName = GenerateName("it task");

            var flux = $"option task = {{\nname: \"{taskName}\",\nevery: 1h,\noffset: 30m\n}}\n\n{TaskFlux}";

            var task = new TaskType(orgID: _organization.Id, org: _organization.Name,
                name: taskName, status: TaskStatusType.Active, flux: flux);

            task = await _tasksApi.CreateTaskAsync(task);

            Assert.IsNotNull(task);
            Assert.AreEqual("30m", task.Offset);
        }

        [Test]
        public async Task DeleteTask()
        {
            var task = await _tasksApi.CreateTaskCronAsync(GenerateName("it task"), TaskFlux, "0 2 * * *",
                _organization);

            var foundTask = await _tasksApi.FindTaskByIdAsync(task.Id);
            Assert.IsNotNull(foundTask);

            await _tasksApi.DeleteTaskAsync(task);

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _tasksApi.FindTaskByIdAsync(task.Id));

            Assert.AreEqual("failed to find task: task not found", ioe?.Message);
        }

        [Test]
        public async Task FindTaskById()
        {
            var taskName = GenerateName("it task");

            var task = await _tasksApi.CreateTaskCronAsync(taskName, TaskFlux, "0 2 * * *", _organization);

            var taskById = await _tasksApi.FindTaskByIdAsync(task.Id);

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
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _tasksApi.FindTaskByIdAsync("020f755c3d082000"));

            Assert.AreEqual("failed to find task: task not found", ioe?.Message);
        }

        [Test]
        public async Task FindTasks()
        {
            var count = (await _tasksApi.FindTasksAsync()).Count;

            var task = await _tasksApi.CreateTaskEveryAsync(GenerateName("it task"), TaskFlux, "2h", _organization.Id);
            Assert.IsNotNull(task);

            var tasks = await _tasksApi.FindTasksAsync();

            Assert.AreEqual(count + 1, tasks.Count);
        }

        [Test]
        public async Task FindTasksAfterSpecifiedId()
        {
            var task1 = await _tasksApi.CreateTaskCronAsync(GenerateName("it task"), TaskFlux, "0 2 * * *",
                _organization);
            var task2 = await _tasksApi.CreateTaskCronAsync(GenerateName("it task"), TaskFlux, "0 2 * * *",
                _organization);

            var tasks = await _tasksApi.FindTasksAsync(task1.Id);

            Assert.AreEqual(1, tasks.Count);
            Assert.AreEqual(task2.Id, tasks[0].Id);
        }

        [Test]
        public async Task FindTasksByOrganization()
        {
            var count = (await _tasksApi.FindTasksByOrganizationAsync(_organization)).Count;
            Assert.GreaterOrEqual(count, 0);

            await _tasksApi.CreateTaskCronAsync(GenerateName("it task"), TaskFlux, "0 2 * * *", _organization);

            var tasks = await _tasksApi.FindTasksByOrganizationAsync(_organization);
            Assert.AreEqual(count + 1, tasks.Count);
        }

        [Test]
        public async Task FindTasksByUser()
        {
            Client.Dispose();
            Client = InfluxDBClientFactory.Create(InfluxDbUrl, "my-user", "my-password".ToCharArray());
            _tasksApi = Client.GetTasksApi();

            var user = (await Client.GetUsersApi().FindUsersAsync(name: "my-user"))[0];

            var count = (await _tasksApi.FindTasksByUserAsync(user)).Count;
            Assert.GreaterOrEqual(count, 0);

            await _tasksApi.CreateTaskCronAsync(GenerateName("it task"), TaskFlux, "0 2 * * *", _organization);

            var tasks = await _tasksApi.FindTasksByUserAsync(user);
            Assert.AreEqual(count + 1, tasks.Count);
        }

        [Test]
        public async Task GetLogs()
        {
            var task = await _tasksApi.CreateTaskEveryAsync(GenerateName("it task"), TaskFlux, "1s", _organization.Id);

            Thread.Sleep(5_000);

            var logs = await _tasksApi.GetLogsAsync(task);
            Assert.IsNotEmpty(logs);
            Assert.IsTrue(logs.First().Message.StartsWith("Started task from script:"));
            Assert.That(logs.Any(p => p.Message.EndsWith("Completed(success)")));
        }

        [Test]
        public void GetLogsNotExist()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(
                async () => await _tasksApi.GetLogsAsync("020f755c3c082000"));

            Assert.NotNull(ioe, "ioe.InnerException != null");
            Assert.AreEqual("failed to find task logs: task not found", ioe.Message);
        }

        [Test]
        public async Task GetRun()
        {
            var task = await _tasksApi.CreateTaskEveryAsync(GenerateName("it task"), TaskFlux, "1s", _organization);
            Thread.Sleep(5_000);

            var runs = await _tasksApi.GetRunsAsync(task, null, null, 1);
            Assert.AreEqual(1, runs.Count);

            var firstRun = runs[0];
            var runById = await _tasksApi.GetRunAsync(task.Id, firstRun.Id);

            Assert.IsNotNull(runById);
            Assert.AreEqual(firstRun.Id, runById.Id);
        }

        [Test]
        public async Task GetRunLogs()
        {
            var task = await _tasksApi.CreateTaskEveryAsync(GenerateName("it task"), TaskFlux, "1s", _organization);

            Thread.Sleep(4_000);

            var runs = await _tasksApi.GetRunsAsync(task);

            var logs = await _tasksApi.GetRunLogsAsync(runs[0]);
            Assert.IsNotNull(logs);
            Assert.IsNotEmpty(logs);
            Assert.That(logs.Any(p => p.Message.EndsWith("Completed(success)")));
        }

        [Test]
        public async Task GetRunLogsNotExist()
        {
            var task = await _tasksApi.CreateTaskEveryAsync(GenerateName("it task"), TaskFlux, "1s", _organization);

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _tasksApi.GetRunLogsAsync(task.Id, "020f755c3c082000"));

            Assert.NotNull(ioe, "ioe.InnerException != null");
            Assert.AreEqual("failed to find task logs: run not found", ioe.Message);
        }

        [Test]
        public async Task Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var task = await _tasksApi.CreateTaskEveryAsync(GenerateName("it task"), TaskFlux, "1s", _organization);

            var properties = new Dictionary<string, string> { { "color", "green" }, { "location", "west" } };

            var label = await labelClient.CreateLabelAsync(GenerateName("Cool Resource"), properties, _organization.Id);

            var labels = await _tasksApi.GetLabelsAsync(task);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = await _tasksApi.AddLabelAsync(label, task);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = await _tasksApi.GetLabelsAsync(task);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            task = await _tasksApi.FindTaskByIdAsync(task.Id);
            Assert.IsNotNull(task);
            Assert.AreEqual(1, task.Labels.Count);
            Assert.AreEqual(label.Id, task.Labels[0].Id);
            Assert.AreEqual(label.Name, task.Labels[0].Name);

            await _tasksApi.DeleteLabelAsync(label, task);

            labels = await _tasksApi.GetLabelsAsync(task);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        public async Task Member()
        {
            var task = await _tasksApi.CreateTaskCronAsync(GenerateName("it task"), TaskFlux, "0 2 * * *",
                _organization);

            var members = await _tasksApi.GetMembersAsync(task);
            Assert.AreEqual(0, members.Count);

            var user = await _usersApi.CreateUserAsync(GenerateName("Luke Health"));

            var resourceMember = await _tasksApi.AddMemberAsync(user, task);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.RoleEnum.Member);

            members = await _tasksApi.GetMembersAsync(task);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].Id, user.Id);
            Assert.AreEqual(members[0].Name, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.RoleEnum.Member);
            await _tasksApi.DeleteMemberAsync(user, task);

            members = await _tasksApi.GetMembersAsync(task);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        [Ignore("TODO https://github.com/influxdata/influxdb/issues/19234")]
        public async Task Owner()
        {
            var task = await _tasksApi.CreateTaskCronAsync(GenerateName("it task"), TaskFlux, "0 2 * * *",
                _organization.Id);

            var owners = await _tasksApi.GetOwnersAsync(task);
            Assert.AreEqual(1, owners.Count);

            var user = await _usersApi.CreateUserAsync(GenerateName("Luke Health"));

            var resourceMember = await _tasksApi.AddOwnerAsync(user, task);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceOwner.RoleEnum.Owner);

            owners = await _tasksApi.GetOwnersAsync(task);
            Assert.AreEqual(2, owners.Count);

            var newOwner = owners.First(o => o.Id.Equals(user.Id));
            Assert.AreEqual(newOwner.Id, user.Id);
            Assert.AreEqual(newOwner.Name, user.Name);
            Assert.AreEqual(newOwner.Role, ResourceOwner.RoleEnum.Owner);

            await _tasksApi.DeleteOwnerAsync(user, task);

            owners = await _tasksApi.GetOwnersAsync(task);
            Assert.AreEqual(1, owners.Count);
        }

        [Test]
        public async Task RetryRun()
        {
            var task = await _tasksApi.CreateTaskEveryAsync(GenerateName("it task"), TaskFlux, "1s", _organization);
            Thread.Sleep(5_000);

            var runs = await _tasksApi.GetRunsAsync(task);
            var run = runs[0];

            var retriedRun = await _tasksApi.RetryRunAsync(run);

            Assert.IsNotNull(retriedRun);
            Assert.AreEqual(run.TaskID, retriedRun.TaskID);
            Assert.AreEqual(Run.StatusEnum.Scheduled, retriedRun.Status);
            Assert.AreEqual(task.Id, retriedRun.TaskID);
        }

        [Test]
        public async Task RetryRunNotExist()
        {
            var task = await _tasksApi.CreateTaskEveryAsync(GenerateName("it task"), TaskFlux, "1s", _organization);

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _tasksApi.RetryRunAsync(task.Id, "020f755c3c082000"));

            Assert.AreEqual("failed to retry run: run not found", ioe?.Message);
        }

        [Test]
        public async Task RunNotExist()
        {
            var task = await _tasksApi.CreateTaskEveryAsync(GenerateName("it task"), TaskFlux, "1s", _organization);

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _tasksApi.GetRunAsync(task.Id, "020f755c3c082000"));

            Assert.AreEqual("failed to find run: run not found", ioe?.Message);
        }

        [Test]
        public async Task Runs()
        {
            var task = await _tasksApi.CreateTaskEveryAsync(GenerateName("it task"), TaskFlux, "1s", _organization.Id);

            Thread.Sleep(5_000);

            var runs = await _tasksApi.GetRunsAsync(task);
            Assert.IsNotEmpty(runs);

            var run = runs.First(it => it.Status.Equals(Run.StatusEnum.Success));
            Assert.IsNotEmpty(run.Id);
            Assert.AreEqual(task.Id, run.TaskID);
            Assert.AreEqual(Run.StatusEnum.Success, run.Status);
            Assert.Greater(DateTime.UtcNow, run.StartedAt);
            Assert.Greater(DateTime.UtcNow, run.FinishedAt);
            Assert.Greater(DateTime.UtcNow, run.ScheduledFor);
            Assert.IsNull(run.RequestedAt);

            task = await _tasksApi.FindTaskByIdAsync(task.Id);
            Assert.IsNotNull(task.LatestCompleted);
        }

        [Test]
        [Ignore("https://github.com/influxdata/influxdb/issues/13577")]
        public async Task RunsByTime()
        {
            var now = DateTime.UtcNow;

            var task = await _tasksApi.CreateTaskEveryAsync(GenerateName("it task"), TaskFlux, "1s", _organization.Id);

            Thread.Sleep(5_000);

            var runs = await _tasksApi.GetRunsAsync(task, null, now, null);
            Assert.IsEmpty(runs);

            runs = await _tasksApi.GetRunsAsync(task, now, null, null);
            Assert.IsNotEmpty(runs);
        }

        [Test]
        [Ignore("https://github.com/influxdata/influxdb/issues/13577")]
        public async Task RunsLimit()
        {
            var task = await _tasksApi.CreateTaskEveryAsync(GenerateName("it task"), TaskFlux, "1s", _organization);

            Thread.Sleep(5_000);

            var runs = await _tasksApi.GetRunsAsync(task, null, null, 1);
            Assert.AreEqual(1, runs.Count);

            runs = await _tasksApi.GetRunsAsync(task, null, null, null);
            Assert.Greater(runs.Count, 1);
        }

        [Test]
        public void RunsNotExist()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _tasksApi.GetRunsAsync("020f755c3c082000"));

            Assert.NotNull(ioe, "ioe.InnerException != null");
            Assert.AreEqual("failed to find runs: task not found", ioe.Message);
        }

        [Test]
        public async Task UpdateTask()
        {
            var taskName = GenerateName("it task");

            var cronTask =
                await _tasksApi.CreateTaskCronAsync(taskName, TaskFlux, "0 2 * * *", _organization);

            var flux = $"option task = {{name: \"{taskName}\", every: 3m}}\n\n{TaskFlux}";

            cronTask.Every = "3m";
            cronTask.Cron = null;
            cronTask.Status = TaskStatusType.Inactive;

            var updatedTask = await _tasksApi.UpdateTaskAsync(cronTask);

            Assert.IsNotNull(updatedTask);
            Assert.IsNotEmpty(updatedTask.Id);
            Assert.AreEqual(taskName, updatedTask.Name);
            Assert.AreEqual(_organization.Id, updatedTask.OrgID);
            Assert.AreEqual(TaskStatusType.Inactive, updatedTask.Status);
            Assert.AreEqual("3m", updatedTask.Every);
            Assert.IsNull(updatedTask.Cron);
            Assert.AreEqual(0, CultureInfo.CurrentCulture.CompareInfo.Compare(
                    updatedTask.Flux, flux, CompareOptions.IgnoreSymbols),
                $"Queries are not same: '{updatedTask.Flux}', '{flux}'.");

            Assert.IsNotNull(updatedTask.UpdatedAt);
        }
    }
}