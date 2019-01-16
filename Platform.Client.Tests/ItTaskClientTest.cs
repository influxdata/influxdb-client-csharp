using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NUnit.Framework;
using Platform.Common.Flux.Error;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    [TestFixture]
    public class ItTaskClientTest : AbstractItClientTest
    {
        private const string TaskFlux = "from(bucket:\"my-bucket\") |> range(start: 0) |> last()";

        private TaskClient _taskClient;
        private UserClient _userClient;

        private Organization _organization;
        private User _user;

        [SetUp]
        public new async Task SetUp()
        {
            _organization = await FindMyOrg();

            _user = await PlatformClient.CreateUserClient().Me();
            
            var authorization = await AddAuthorization(_organization);

            PlatformClient.Dispose();
            PlatformClient = PlatformClientFactory.Create(PlatformUrl, authorization.Token.ToCharArray());

            _taskClient = PlatformClient.CreateTaskClient();

            _userClient = PlatformClient.CreateUserClient();
            
            (await _taskClient.FindTasks()).ForEach(async task => await _taskClient.DeleteTask(task));
        }

        [Test]
        public async Task CreateTask()
        {
            var taskName = GenerateName("it task");

            var flux = $"option task = {{\nname: \"{taskName}\",\nevery: 1h\n}}\n\n{TaskFlux}";

            var task = new InfluxData.Platform.Client.Domain.Task
            {
                Name = taskName, OrgId = _organization.Id, Owner = _user, Flux = flux, Status = Status.Active
            };

            task = await _taskClient.CreateTask(task);

            Assert.IsNotNull(task);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(taskName, task.Name);
            Assert.IsNotNull(task.Owner);
            Assert.AreEqual(_user.Id, task.Owner.Id);
            Assert.AreEqual(_user.Name, task.Owner.Name);
            Assert.AreEqual(_organization.Id, task.OrgId);
            Assert.AreEqual(Status.Active, task.Status);
            Assert.AreEqual("1h0m0s", task.Every);
            Assert.IsNull(task.Cron);
            Assert.IsTrue(task.Flux.Equals(flux, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async Task CreateTaskWithOffset()
        {
            var taskName = GenerateName("it task");

            var flux = $"option task = {{\nname: \"{taskName}\",\nevery: 1h\n}}\n\n{TaskFlux}";

            var task = new InfluxData.Platform.Client.Domain.Task
            {
                Name = taskName, OrgId = _organization.Id, Owner = _user, Flux = flux, Status = Status.Active,
                Offset = "30m"
            };

            task = await _taskClient.CreateTask(task);

            Assert.IsNotNull(task);
            Assert.AreEqual("30m", task.Offset);
        }

        [Test]
        public async Task CreateTaskEvery()
        {
            var taskName = GenerateName("it task");


            var task =
                await _taskClient.CreateTaskEvery(taskName, TaskFlux, "1h", _user, _organization);

            Assert.IsNotNull(task);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(taskName, task.Name);
            Assert.IsNotNull(task.Owner);
            Assert.AreEqual(_user.Id, task.Owner.Id);
            Assert.AreEqual(_user.Name, task.Owner.Name);
            Assert.AreEqual(_organization.Id, task.OrgId);
            Assert.AreEqual(Status.Active, task.Status);
            Assert.AreEqual("1h0m0s", task.Every);
            Assert.IsNull(task.Cron);
            Assert.IsTrue(task.Flux.EndsWith(TaskFlux, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async Task CreateTaskCron()
        {
            var taskName = GenerateName("it task");


            var task =
                await _taskClient.CreateTaskCron(taskName, TaskFlux, "0 2 * * *", _user, _organization);

            Assert.IsNotNull(task);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(taskName, task.Name);
            Assert.IsNotNull(task.Owner);
            Assert.AreEqual(_user.Id, task.Owner.Id);
            Assert.AreEqual(_user.Name, task.Owner.Name);
            Assert.AreEqual(_organization.Id, task.OrgId);
            Assert.AreEqual(Status.Active, task.Status);
            Assert.AreEqual("0 2 * * *", task.Cron);
            Assert.AreEqual("0s", task.Every);
            Assert.IsTrue(task.Flux.EndsWith(TaskFlux, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        //TODO
        [Ignore("Enable after implement mapping background Task to Task /platform/task/platform_adapter.go:89")]
        public async Task UpdateTask()
        {
            var taskName = GenerateName("it task");

            var cronTask =
                await _taskClient.CreateTaskCron(taskName, TaskFlux, "0 2 * * *", _user, _organization);

            var flux = $"option task = {{\n    name: \"{taskName}\",\n    every: 2m\n}}\n\n{TaskFlux}";

            cronTask.Flux = flux;
            cronTask.Status = Status.Inactive;

            var updatedTask = await _taskClient.UpdateTask(cronTask);

            Assert.IsNotNull(updatedTask);
            Assert.IsNotEmpty(updatedTask.Id);
            Assert.AreEqual(taskName, updatedTask.Name);
            Assert.IsNotNull(updatedTask.Owner);
            Assert.AreEqual(_user.Id, updatedTask.Owner.Id);
            Assert.AreEqual(_user.Name, updatedTask.Owner.Name);
            Assert.AreEqual(_organization.Id, updatedTask.OrgId);
            Assert.AreEqual(Status.Inactive, updatedTask.Status);
            Assert.IsNull(updatedTask.Cron);
            Assert.AreEqual("2m0s", updatedTask.Every);
            Assert.IsTrue(updatedTask.Flux.Equals(flux, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async Task FindTaskById()
        {
            var taskName = GenerateName("it task");

            var task = await _taskClient.CreateTaskCron(taskName, TaskFlux, "0 2 * * *", _user, _organization);
            
            var taskById = await _taskClient.FindTaskById(task.Id);
            
            Assert.IsNotNull(taskById);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(task.Id, taskById.Id);
            Assert.AreEqual(task.Name, taskById.Name);
            Assert.AreEqual(task.Owner.Id, taskById.Owner.Id);
            Assert.AreEqual(task.OrgId, taskById.OrgId);
            Assert.AreEqual(task.Status, taskById.Status);
            Assert.AreEqual(task.Offset, taskById.Offset);
            Assert.AreEqual(task.Flux, taskById.Flux);
            Assert.AreEqual(task.Cron, taskById.Cron);
        }
        
        [Test]
        public async Task FindTaskByIdNull()
        {
            var task = await _taskClient.FindTaskById("020f755c3d082000");
            
            Assert.IsNull(task);
        }

        [Test]
        public async Task FindTasks()
        {
            var count = (await _taskClient.FindTasks()).Count;

            var task = await _taskClient.CreateTaskEvery(GenerateName("it task"), TaskFlux, "2h", _user.Id, _organization.Id);
            Assert.IsNotNull(task);

            var tasks = await _taskClient.FindTasks();

            Assert.AreEqual(count + 1, tasks.Count);
        }
        
        [Test]
        public async Task FindTasksByUser()
        {
            var taskUser = await PlatformClient.CreateUserClient().CreateUser(GenerateName("Task user"));

            var count = (await _taskClient.FindTasksByUser(taskUser)).Count;
            Assert.AreEqual(0, count);

            await _taskClient.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", taskUser, _organization);

            var tasks = await _taskClient.FindTasksByUser(taskUser);

            Assert.AreEqual(1, tasks.Count);
        }
        
        [Test]
        public async Task FindTasksByOrganization()
        {
            var taskOrg = await PlatformClient.CreateOrganizationClient().CreateOrganization(GenerateName("Task user"));
            var authorization = await AddAuthorization(taskOrg);

            PlatformClient.Dispose();
            PlatformClient = PlatformClientFactory.Create(PlatformUrl, authorization.Token.ToCharArray());
            _taskClient = PlatformClient.CreateTaskClient();
            
            var count = (await _taskClient.FindTasksByOrganization(taskOrg)).Count;
            Assert.AreEqual(0, count);

            await _taskClient.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _user, taskOrg);

            var tasks = await _taskClient.FindTasksByOrganization(taskOrg);

            Assert.AreEqual(1, tasks.Count);
            
            (await _taskClient.FindTasks()).ForEach(async task => await _taskClient.DeleteTask(task));
        }
        
        [Test]
        public async Task FindTasksAfterSpecifiedId()
        {
            var task1 = await _taskClient.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _user, _organization);
            var task2 = await _taskClient.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _user, _organization);

            var tasks = await  _taskClient.FindTasks(task1.Id, null, null);
            
            Assert.AreEqual(1, tasks.Count);
            Assert.AreEqual(task2.Id, tasks[0].Id);
        }
        
        [Test]
        public async Task DeleteTask()
        {
            var task = await _taskClient.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _user, _organization);

            var foundTask = await _taskClient.FindTaskById(task.Id);
            Assert.IsNotNull(foundTask);

            await _taskClient.DeleteTask(task);
            foundTask = await _taskClient.FindTaskById(task.Id);

            Assert.IsNull(foundTask);
        }
        
        [Test]
        public async Task Member() {

            var task = await _taskClient.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _user, _organization);

            var members =  await _taskClient.GetMembers(task);
            Assert.AreEqual(0, members.Count);

            var user = await _userClient.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _taskClient.AddMember(user, task);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.UserId, user.Id);
            Assert.AreEqual(resourceMember.UserName, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.UserType.Member);

            members = await _taskClient.GetMembers(task);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].UserId, user.Id);
            Assert.AreEqual(members[0].UserName, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.UserType.Member);
            await _taskClient.DeleteMember(user, task);

            members = await _taskClient.GetMembers(task);
            Assert.AreEqual(0, members.Count);
        }
        
        [Test]
        public async Task Owner() {

            var task = await _taskClient.CreateTaskCron(GenerateName("it task"), TaskFlux, "0 2 * * *", _user.Id, _organization.Id);

            var owners =  await _taskClient.GetOwners(task);
            Assert.AreEqual(0, owners.Count);

            var user = await _userClient.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _taskClient.AddOwner(user, task);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.UserId, user.Id);
            Assert.AreEqual(resourceMember.UserName, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.UserType.Owner);

            owners = await _taskClient.GetOwners(task);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual(owners[0].UserId, user.Id);
            Assert.AreEqual(owners[0].UserName, user.Name);
            Assert.AreEqual(owners[0].Role, ResourceMember.UserType.Owner);

            await _taskClient.DeleteOwner(user, task);

            owners = await _taskClient.GetOwners(task);
            Assert.AreEqual(0, owners.Count);
        }

        [Test]
        public async Task GetLogs()
        {
            var task = await _taskClient.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _user.Id, _organization.Id);

            Thread.Sleep(5_000);

            var logs = await _taskClient.GetLogs(task);
            Assert.IsNotEmpty(logs);
            Assert.IsTrue(logs[0].EndsWith("Completed successfully"));
        }
        
        [Test]
        public async Task GetLogsNotExist()
        {
            var logs = await _taskClient.GetLogs("020f755c3c082000", _organization.Id);

            Assert.IsEmpty(logs);
        }

        [Test]
        public async Task Runs()
        {
            var task = await _taskClient.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _user.Id, _organization.Id);

            Thread.Sleep(5_000);

            var runs = await _taskClient.GetRuns(task);
            Assert.IsNotEmpty(runs);

            Assert.IsNotEmpty(runs[0].Id);
            Assert.AreEqual(task.Id, runs[0].TaskId);
            Assert.AreEqual(RunStatus.Success, runs[0].Status);
            Assert.Greater(DateTime.Now, runs[0].StartedAt);
            Assert.Greater(DateTime.Now, runs[0].FinishedAt);
            Assert.Greater(DateTime.Now, runs[0].ScheduledFor);
            Assert.IsNull(runs[0].RequestedAt);
            Assert.IsEmpty(runs[0].Log);
        }

        [Test]
        public async Task RunsNotExist()
        {
            var runs = await _taskClient.GetRuns("020f755c3c082000", _organization.Id);
            Assert.IsEmpty(runs);
        }

        [Test]
        public async Task RunsByTime()
        {
            var now = DateTime.UtcNow;
            
            var task = await _taskClient.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _user.Id, _organization.Id);

            Thread.Sleep(5_000);

            var runs = await _taskClient.GetRuns(task, null, now, null);
            Assert.IsEmpty(runs);
            
            runs = await _taskClient.GetRuns(task, now, null, null);
            Assert.IsNotEmpty(runs);
        }

        [Test]
        public async Task RunsLimit()
        {
            var task = await _taskClient.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _user, _organization);
            
            Thread.Sleep(5_000);
            
            var runs = await _taskClient.GetRuns(task, null, null, 1);
            Assert.AreEqual(1, runs.Count);
            
            runs = await _taskClient.GetRuns(task, null, null, null);
            Assert.Greater(runs.Count, 1);
        }

        [Test]
        //TODO
        [Ignore("avoid panic: column _measurement is not of type time goroutine")]
        public async Task GetRun()
        {
            var task = await _taskClient.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _user, _organization);
            Thread.Sleep(5_000);
            
            var runs = await _taskClient.GetRuns(task, null, null, 1);
            Assert.AreEqual(1, runs.Count);
            
            var firstRun = runs[0];
            var runById = await _taskClient.GetRun(task.Id, firstRun.Id);
            
            Assert.IsNotNull(runById);
            Assert.AreEqual(firstRun.Id, runById.Id);
        }
        
        [Test]
        public async Task RunNotExist()
        {
            var task = await _taskClient.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _user, _organization);
            
            var run = await  _taskClient.GetRun(task.Id, "020f755c3c082000");
            Assert.IsNull(run);
        }

        [Test]
        //TODO
        [Ignore("avoid panic: column _measurement is not of type time goroutine")]
        public async Task RetryRun()
        {
            var task = await _taskClient.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _user, _organization);
            Thread.Sleep(5_000);
            
            var runs = await _taskClient.GetRuns(task, null, null, 1);
            Assert.AreEqual(1, runs.Count);
            
            var run = runs[0];

            var retriedRun = await _taskClient.RetryRun(run);
            
            Assert.IsNotNull(retriedRun);
            Assert.AreEqual(run.Id, retriedRun.Id);
            Assert.Less(runs.Count, (await _taskClient.GetRuns(task)).Count);
        }
        
        [Test]
        public async Task RetryRunNotExist()
        {
            var task = await _taskClient.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _user, _organization);
            
            var retriedRun = await _taskClient.RetryRun(task.Id, "020f755c3c082000");
            
            Assert.IsNull(retriedRun);
        }

        [Test]
        public async Task CancelRunNotExist()
        {
            var task = await _taskClient.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _user, _organization);

            Thread.Sleep(5_000);

            var runs = await _taskClient.GetRuns(task, null, null, 1);
            Assert.IsNotEmpty(runs);

            var message = Assert.ThrowsAsync<HttpException>(async () => await _taskClient.CancelRun(runs[0])).Message;
            
            Assert.AreEqual(message, "run not found");
        }

        [Test]
        public void CancelRunTaskNotExist()
        {
            var message = Assert.ThrowsAsync<HttpException>(async () =>
                await _taskClient.CancelRun("020f755c3c082000", "020f755c3c082000")).Message;

            Assert.AreEqual(message, "task not found");
        }

        [Test]
        public async Task GetRunLogs()
        {
            var task = await _taskClient.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _user, _organization);
            
            Thread.Sleep(4_000);
            
            var runs = await _taskClient.GetRuns(task, null, null, 1);
            Assert.AreEqual(1, runs.Count);
            
            var logs = await _taskClient.GetRunLogs(runs[0], _organization.Id);
            Assert.AreEqual(1, logs.Count);
            Assert.IsTrue(logs[0].EndsWith("Completed successfully"));
        }
        
        [Test]
        public async Task GetRunLogsNotExist()
        {
            var task = await _taskClient.CreateTaskEvery(GenerateName("it task"), TaskFlux, "1s", _user, _organization);
            
            var logs = await _taskClient.GetRunLogs(task.Id,"020f755c3c082000",  _organization.Id);
            Assert.IsEmpty(logs);
        }
        
        private async Task<Authorization> AddAuthorization(Organization organization)
        {
            var resource = new PermissionResource {Type = PermissionResourceType.Task, OrgId = organization.Id};

            var createTask = new Permission
                { 
                    Resource = resource, 
                    Action = Permission.ReadAction
                    
                };
            
            var deleteTask = new Permission
            {
                Resource = resource, 
                Action = Permission.WriteAction
            };

            var authorization = await PlatformClient.CreateAuthorizationClient()
                .CreateAuthorization(organization, new List<Permission> {createTask, deleteTask});
            
            return authorization;
        }

    }
}