using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    [Ignore("https://github.com/influxdata/influxdb/issues/13576")]
    public class ItTasksApiTest : AbstractItClientTest
    {
        [SetUp]
        public new void SetUp()
        {
            _organization = FindMyOrg();

            var authorization = AddAuthorization(_organization);

            Client.Dispose();
            Client = InfluxDBClientFactory.Create(InfluxDbUrl, authorization.Token.ToCharArray());

            _tasksApi = Client.GetTasksApi();

            _usersApi = Client.GetUsersApi();

            _tasksApi.FindTasks().ForEach(task => _tasksApi.DeleteTask(task));
        }

        private const string TaskFlux = "from(bucket: \"my-bucket\")\n\t|> range(start: 0)\n\t|> last()";

        private TasksApi _tasksApi;
        private UsersApi _usersApi;

        private Organization _organization;

        private Authorization AddAuthorization(Organization organization)
        {
            var resourceTask = new PermissionResource(PermissionResource.TypeEnum.Tasks, null, null, organization.Id);
            var resourceBucket = new PermissionResource(PermissionResource.TypeEnum.Buckets,
                Client.GetBucketsApi().FindBucketByName("my-bucket").Id, null, organization.Id);
            var resourceOrg = new PermissionResource(PermissionResource.TypeEnum.Orgs);
            var resourceUser = new PermissionResource(PermissionResource.TypeEnum.Users);
            var resourceAuthorization = new PermissionResource(PermissionResource.TypeEnum.Authorizations);


            var authorization = Client.GetAuthorizationsApi()
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
        public void CancelRunNotExist()
        {
            var task = _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            Thread.Sleep(5_000);

            var runs = _tasksApi.GetRuns(task, null, null, 1);
            Assert.IsNotEmpty(runs);

            var message = Assert.Throws<HttpException>(() => _tasksApi.CancelRun(runs[0]))
                .ErrorBody["error"].ToString();

            Assert.AreEqual(message, "run not found");
        }

        [Test]
        public void CancelRunTaskNotExist()
        {
            var message = Assert.Throws<HttpException>(() =>
                _tasksApi.CancelRun("020f755c3c082000", "020f755c3c082000")).ErrorBody["error"].ToString();

            Assert.AreEqual(message, "task not found");
        }

        [Test]
        public void CloneTask()
        {
            var source = _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = Client.GetLabelsApi().CreateLabel(GenerateName("Cool Resource"), properties, _organization.Id);
            _tasksApi.AddLabel(label, source);

            var cloned = _tasksApi.CloneTask(source);

            Assert.AreEqual(source.Name, cloned.Name);
            Assert.AreEqual(_organization.Id, cloned.OrgID);
            Assert.AreEqual(source.Flux, cloned.Flux);
            Assert.AreEqual("1s", cloned.Every);
            Assert.IsNull(cloned.Cron);
            Assert.IsNull(cloned.Offset);

            var labels = _tasksApi.GetLabels(cloned);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
        }

        [Test]
        public void CloneTaskNotFound()
        {
            var ioe = Assert.Throws<HttpException>(() => _tasksApi.CloneTask("020f755c3c082000"));

            Assert.AreEqual("failed to find task", ioe.Message);
        }

        [Test]
        public void CreateTask()
        {
            var taskName = GenerateName("it task");

            var flux = $"option task = {{\nname: \"{taskName}\",\nevery: 1h\n}}\n\n{TaskFlux}";

            var task = new Task(_organization.Id, _organization.Name,
                taskName, "testing task", Task.StatusEnum.Active, null, null, flux);

            task = _tasksApi.CreateTask(task);

            Assert.IsNotNull(task);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(taskName, task.Name);
            Assert.AreEqual(_organization.Id, task.OrgID);
            Assert.AreEqual(Task.StatusEnum.Active, task.Status);
            Assert.AreEqual("1h", task.Every);
            Assert.AreEqual("testing task", task.Description);
            Assert.IsNull(task.Cron);
            Assert.IsTrue(task.Flux.Equals(flux, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void CreateTaskCron()
        {
            var taskName = GenerateName("it task");


            var task =
                _tasksApi.CreateTaskCron(taskName, TaskFlux, "0 2 * * *", _organization);

            Assert.IsNotNull(task);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(taskName, task.Name);
            Assert.AreEqual(_organization.Id, task.OrgID);
            Assert.AreEqual(Task.StatusEnum.Active, task.Status);
            Assert.AreEqual("0 2 * * *", task.Cron);
            Assert.IsNull(task.Every);
            Assert.IsTrue(task.Flux.EndsWith(TaskFlux, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void CreateTaskEvery()
        {
            var taskName = GenerateName("it task");


            var task =
                _tasksApi.CreateTaskEvery(taskName, TaskFlux, "1h", _organization);

            Assert.IsNotNull(task);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(taskName, task.Name);
            Assert.AreEqual(_organization.Id, task.OrgID);
            Assert.AreEqual(Task.StatusEnum.Active, task.Status);
            Assert.AreEqual("1h", task.Every);
            Assert.IsNull(task.Cron);
            Assert.IsTrue(task.Flux.EndsWith(TaskFlux, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void CreateTaskWithOffset()
        {
            Client.SetLogLevel(LogLevel.Body);

            var taskName = GenerateName("it task");

            var flux = $"option task = {{\nname: \"{taskName}\",\nevery: 1h,\noffset: 30m\n}}\n\n{TaskFlux}";

            var task = new Task(_organization.Id, _organization.Name, taskName,
                null, Task.StatusEnum.Active, null, null, flux);

            task = _tasksApi.CreateTask(task);

            Assert.IsNotNull(task);
            Assert.AreEqual("30m", task.Offset);
        }

        [Test]
        public void DeleteTask()
        {
            var task = _tasksApi.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _organization);

            var foundTask = _tasksApi.FindTaskById(task.Id);
            Assert.IsNotNull(foundTask);

            _tasksApi.DeleteTask(task);

            var ioe = Assert.Throws<HttpException>(() => _tasksApi.FindTaskById(task.Id));

            Assert.AreEqual("failed to find task", ioe.Message);
        }

        [Test]
        public void FindTaskById()
        {
            var taskName = GenerateName("it task");

            var task = _tasksApi.CreateTaskCron(taskName, TaskFlux, "0 2 * * *", _organization);

            var taskById = _tasksApi.FindTaskById(task.Id);

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
            var ioe = Assert.Throws<HttpException>(() => _tasksApi.FindTaskById("020f755c3d082000"));

            Assert.AreEqual("failed to find task", ioe.Message);
        }

        [Test]
        public void FindTasks()
        {
            var count = _tasksApi.FindTasks().Count;

            var task = _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "2h", _organization.Id);
            Assert.IsNotNull(task);

            var tasks = _tasksApi.FindTasks();

            Assert.AreEqual(count + 1, tasks.Count);
        }

        [Test]
        public void FindTasksAfterSpecifiedId()
        {
            var task1 = _tasksApi.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _organization);
            var task2 = _tasksApi.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _organization);

            var tasks = _tasksApi.FindTasks(task1.Id);

            Assert.AreEqual(1, tasks.Count);
            Assert.AreEqual(task2.Id, tasks[0].Id);
        }

        [Test]
        [Ignore("https://github.com/influxdata/influxdb/issues/11491")]
        //TODO
        public void FindTasksByOrganization()
        {
            var taskOrg = Client.GetOrganizationsApi().CreateOrganization(GenerateName("Task user"));
            var authorization = AddAuthorization(taskOrg);

            Client.Dispose();
            Client = InfluxDBClientFactory.Create(InfluxDbUrl, authorization.Token.ToCharArray());
            _tasksApi = Client.GetTasksApi();

            var count = _tasksApi.FindTasksByOrganization(taskOrg).Count;
            Assert.AreEqual(0, count);

            _tasksApi.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", taskOrg);

            var tasks = _tasksApi.FindTasksByOrganization(taskOrg);

            Assert.AreEqual(1, tasks.Count);

            _tasksApi.FindTasks().ForEach(task => _tasksApi.DeleteTask(task));
        }

        [Test]
        //TODO
        [Ignore("set user password -> https://github.com/influxdata/influxdb/issues/11590")]
        public void FindTasksByUser()
        {
            var taskUser = Client.GetUsersApi().CreateUser(GenerateName("Task user"));

            var count = _tasksApi.FindTasksByUser(taskUser).Count;
            Assert.AreEqual(0, count);

            _tasksApi.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _organization);

            var tasks = _tasksApi.FindTasksByUser(taskUser);

            Assert.AreEqual(1, tasks.Count);
        }

        [Test]
        public void GetLogs()
        {
            var task = _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization.Id);

            Thread.Sleep(5_000);

            var logs = _tasksApi.GetLogs(task);
            Assert.IsNotEmpty(logs);
            Assert.IsTrue(logs.First().Message.StartsWith("Started task from script:"));
            Assert.IsTrue(logs.Last().Message.EndsWith("Completed successfully"));
        }

        [Test]
        public void GetLogsNotExist()
        {
            var ioe = Assert.Throws<HttpException>(() => _tasksApi.GetLogs("020f755c3c082000"));

            Assert.AreEqual("failed to find task logs", ioe.Message);
        }

        [Test]
        public void GetRun()
        {
            var task = _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);
            Thread.Sleep(5_000);

            var runs = _tasksApi.GetRuns(task, null, null, 1);
            Assert.AreEqual(1, runs.Count);

            var firstRun = runs[0];
            var runById = _tasksApi.GetRun(task.Id, firstRun.Id);

            Assert.IsNotNull(runById);
            Assert.AreEqual(firstRun.Id, runById.Id);
        }

        [Test]
        public void GetRunLogs()
        {
            var task = _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            Thread.Sleep(4_000);

            var runs = _tasksApi.GetRuns(task, null, null, 1);
            Assert.AreEqual(1, runs.Count);

            var logs = _tasksApi.GetRunLogs(runs[0], _organization.Id);
            Assert.IsNotNull(logs);
            Assert.IsTrue(logs.Last().Message.EndsWith("Completed successfully"));
        }

        [Test]
        public void GetRunLogsNotExist()
        {
            var task = _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            var logs = _tasksApi.GetRunLogs(task.Id, "020f755c3c082000", _organization.Id);
            Assert.IsEmpty(logs);
        }

        [Test]
        public void Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var task = _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = labelClient.CreateLabel(GenerateName("Cool Resource"), properties, _organization.Id);

            var labels = _tasksApi.GetLabels(task);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = _tasksApi.AddLabel(label, task);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = _tasksApi.GetLabels(task);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            task = _tasksApi.FindTaskById(task.Id);
            Assert.IsNotNull(task);
            Assert.AreEqual(1, task.Labels.Count);
            Assert.AreEqual(label.Id, task.Labels[0].Id);
            Assert.AreEqual(label.Name, task.Labels[0].Name);

            _tasksApi.DeleteLabel(label, task);

            labels = _tasksApi.GetLabels(task);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        [Ignore("https://github.com/influxdata/influxdb/issues/11491")]
        //TODO
        public void Member()
        {
            var task = _tasksApi.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _organization);

            var members = _tasksApi.GetMembers(task);
            Assert.AreEqual(0, members.Count);

            var user = _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = _tasksApi.AddMember(user, task);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.RoleEnum.Member);

            members = _tasksApi.GetMembers(task);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].Id, user.Id);
            Assert.AreEqual(members[0].Name, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.RoleEnum.Member);
            _tasksApi.DeleteMember(user, task);

            members = _tasksApi.GetMembers(task);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        [Ignore("https://github.com/influxdata/influxdb/issues/11491")]
        //TODO
        public void Owner()
        {
            var task = _tasksApi.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _organization.Id);

            var owners = _tasksApi.GetOwners(task);
            Assert.AreEqual(0, owners.Count);

            var user = _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = _tasksApi.AddOwner(user, task);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceOwner.RoleEnum.Owner);

            owners = _tasksApi.GetOwners(task);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual(owners[0].Id, user.Id);
            Assert.AreEqual(owners[0].Name, user.Name);
            Assert.AreEqual(owners[0].Role, ResourceOwner.RoleEnum.Owner);

            _tasksApi.DeleteOwner(user, task);

            owners = _tasksApi.GetOwners(task);
            Assert.AreEqual(0, owners.Count);
        }

        [Test]
        public void RetryRun()
        {
            var task = _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);
            Thread.Sleep(5_000);

            var runs = _tasksApi.GetRuns(task, null, null, 1);
            Assert.AreEqual(1, runs.Count);

            var run = runs[0];

            var retriedRun = _tasksApi.RetryRun(run);

            Assert.IsNotNull(retriedRun);
            Assert.AreEqual(run.TaskID, retriedRun.TaskID);
            Assert.AreEqual(Run.StatusEnum.Scheduled, retriedRun.Status);
            Assert.AreEqual(task.Id, retriedRun.TaskID);
        }

        [Test]
        public void RetryRunNotExist()
        {
            var task = _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            var ioe = Assert.Throws<HttpException>(() => _tasksApi.RetryRun(task.Id, "020f755c3c082000"));

            Assert.AreEqual("failed to retry run", ioe.Message);
        }

        [Test]
        public void RunNotExist()
        {
            var task = _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            var ioe = Assert.Throws<HttpException>(() => _tasksApi.GetRun(task.Id, "020f755c3c082000"));

            Assert.AreEqual("failed to find run", ioe.Message);
        }

        [Test]
        public void Runs()
        {
            var task = _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization.Id);

            Thread.Sleep(5_000);

            var runs = _tasksApi.GetRuns(task);
            Assert.IsNotEmpty(runs);

            Assert.IsNotEmpty(runs[0].Id);
            Assert.AreEqual(task.Id, runs[0].TaskID);
            Assert.AreEqual(Run.StatusEnum.Success, runs[0].Status);
            Assert.Greater(DateTime.Now, runs[0].StartedAt);
            Assert.Greater(DateTime.Now, runs[0].FinishedAt);
            Assert.Greater(DateTime.Now, runs[0].ScheduledFor);
            Assert.IsNull(runs[0].RequestedAt);

            task = _tasksApi.FindTaskById(task.Id);
            Assert.IsNotNull(task.LatestCompleted);
        }

        [Test]
        public void RunsByTime()
        {
            var now = DateTime.UtcNow;

            var task = _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization.Id);

            Thread.Sleep(5_000);

            var runs = _tasksApi.GetRuns(task, null, now, null);
            Assert.IsEmpty(runs);

            runs = _tasksApi.GetRuns(task, now, null, null);
            Assert.IsNotEmpty(runs);
        }

        [Test]
        public void RunsLimit()
        {
            var task = _tasksApi.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _organization);

            Thread.Sleep(5_000);

            var runs = _tasksApi.GetRuns(task, null, null, 1);
            Assert.AreEqual(1, runs.Count);

            runs = _tasksApi.GetRuns(task, null, null, null);
            Assert.Greater(runs.Count, 1);
        }

        [Test]
        public void RunsNotExist()
        {
            var ioe = Assert.Throws<HttpException>(() => _tasksApi.GetRuns("020f755c3c082000", _organization.Id));

            Assert.AreEqual("failed to find runs", ioe.Message);
        }

        [Test]
        public void UpdateTask()
        {
            var taskName = GenerateName("it task");

            var cronTask =
                _tasksApi.CreateTaskCron(taskName, TaskFlux, "0 2 * * *", _organization);

            var flux = $"option task = {{name: \"{taskName}\", every: 3m}}\n\n{TaskFlux}";

            cronTask.Every = "3m";
            cronTask.Cron = null;
            cronTask.Status = Task.StatusEnum.Inactive;

            var updatedTask = _tasksApi.UpdateTask(cronTask);

            Assert.IsNotNull(updatedTask);
            Assert.IsNotEmpty(updatedTask.Id);
            Assert.AreEqual(taskName, updatedTask.Name);
            Assert.AreEqual(_organization.Id, updatedTask.OrgID);
            Assert.AreEqual(Task.StatusEnum.Inactive, updatedTask.Status);
            Assert.AreEqual("3m", updatedTask.Every);
            Assert.IsNull(updatedTask.Cron);
            Assert.AreEqual(updatedTask.Flux, flux);
            Assert.IsNotNull(updatedTask.UpdatedAt);
        }
    }
}