using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NUnit.Framework;
using Task = InfluxData.Platform.Client.Domain.Task;

namespace Platform.Client.Tests
{
    [TestFixture]
    public class ItTaskClientTest : AbstractItClientTest
    {
        private static string TASK_FLUX = "from(bucket:\"my-bucket\") |> range(start: 0) |> last()";

        private TaskClient _taskClient;
        private UserClient _userClient;

        private Organization _organization;
        private User _user;

        [SetUp]
        public new async System.Threading.Tasks.Task SetUp()
        {
            var token = (await PlatformClient.CreateAuthorizationClient().FindAuthorizations())
                .First(authorization => authorization.Permissions.Count.Equals(4))
                .Token;

            PlatformClient.Dispose();
            PlatformClient = PlatformClientFactory.Create(PlatformUrl, token.ToCharArray());

            _taskClient = PlatformClient.CreateTaskClient();
            _organization = (await PlatformClient.CreateOrganizationClient().FindOrganizations())
                .First(organization => organization.Name.Equals("my-org"));

            _userClient = PlatformClient.CreateUserClient();
            _user = await _userClient.Me();
            
            (await _taskClient.FindTasks()).ForEach(async task => await _taskClient.DeleteTask(task));
        }

        [Test]
        public async System.Threading.Tasks.Task CreateTask()
        {
            string taskName = GenerateName("it task");

            string flux = $"option task = {{\nname: \"{taskName}\",\nevery: 1h\n}}\n\n{TASK_FLUX}";

            Task task = new Task
            {
                Name = taskName, OrganizationId = _organization.Id, Owner = _user, Flux = flux, Status = Status.Active
            };

            task = await _taskClient.CreateTask(task);

            Assert.IsNotNull(task);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(taskName, task.Name);
            Assert.IsNotNull(task.Owner);
            Assert.AreEqual(_user.Id, task.Owner.Id);
            Assert.AreEqual(_user.Name, task.Owner.Name);
            Assert.AreEqual(_organization.Id, task.OrganizationId);
            Assert.AreEqual(Status.Active, task.Status);
            Assert.AreEqual("1h0m0s", task.Every);
            Assert.IsNull(task.Cron);
            Assert.IsTrue(task.Flux.Equals(flux, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async System.Threading.Tasks.Task CreateTaskWithOffset()
        {
            string taskName = GenerateName("it task");

            string flux = $"option task = {{\nname: \"{taskName}\",\nevery: 1h\n}}\n\n{TASK_FLUX}";

            var task = new Task
            {
                Name = taskName, OrganizationId = _organization.Id, Owner = _user, Flux = flux, Status = Status.Active,
                Offset = "30m"
            };

            task = await _taskClient.CreateTask(task);

            Assert.IsNotNull(task);
            Assert.AreEqual("30m", task.Offset);
        }

        [Test]
        public async System.Threading.Tasks.Task CreateTaskEvery()
        {
            string taskName = GenerateName("it task");


            Task task =
                await _taskClient.CreateTaskEvery(taskName, TASK_FLUX, "1h", _user, _organization);

            Assert.IsNotNull(task);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(taskName, task.Name);
            Assert.IsNotNull(task.Owner);
            Assert.AreEqual(_user.Id, task.Owner.Id);
            Assert.AreEqual(_user.Name, task.Owner.Name);
            Assert.AreEqual(_organization.Id, task.OrganizationId);
            Assert.AreEqual(Status.Active, task.Status);
            Assert.AreEqual("1h0m0s", task.Every);
            Assert.IsNull(task.Cron);
            Assert.IsTrue(task.Flux.EndsWith(TASK_FLUX, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async System.Threading.Tasks.Task CreateTaskCron()
        {
            string taskName = GenerateName("it task");


            Task task =
                await _taskClient.CreateTaskCron(taskName, TASK_FLUX, "0 2 * * *", _user, _organization);

            Assert.IsNotNull(task);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(taskName, task.Name);
            Assert.IsNotNull(task.Owner);
            Assert.AreEqual(_user.Id, task.Owner.Id);
            Assert.AreEqual(_user.Name, task.Owner.Name);
            Assert.AreEqual(_organization.Id, task.OrganizationId);
            Assert.AreEqual(Status.Active, task.Status);
            Assert.AreEqual("0 2 * * *", task.Cron);
            Assert.AreEqual("0s", task.Every);
            Assert.IsTrue(task.Flux.EndsWith(TASK_FLUX, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        //TODO
        [Ignore("Enable after implement mapping background Task to Task /platform/task/platform_adapter.go:89")]
        public async System.Threading.Tasks.Task UpdateTask()
        {
            string taskName = GenerateName("it task");

            Task cronTask =
                await _taskClient.CreateTaskCron(taskName, TASK_FLUX, "0 2 * * *", _user, _organization);

            String flux = $"option task = {{\n    name: \"{taskName}\",\n    every: 2m\n}}\n\n{TASK_FLUX}";

            cronTask.Flux = flux;
            cronTask.Status = Status.Inactive;

            Task updatedTask = await _taskClient.UpdateTask(cronTask);

            Assert.IsNotNull(updatedTask);
            Assert.IsNotEmpty(updatedTask.Id);
            Assert.AreEqual(taskName, updatedTask.Name);
            Assert.IsNotNull(updatedTask.Owner);
            Assert.AreEqual(_user.Id, updatedTask.Owner.Id);
            Assert.AreEqual(_user.Name, updatedTask.Owner.Name);
            Assert.AreEqual(_organization.Id, updatedTask.OrganizationId);
            Assert.AreEqual(Status.Inactive, updatedTask.Status);
            Assert.IsNull(updatedTask.Cron);
            Assert.AreEqual("2m0s", updatedTask.Every);
            Assert.IsTrue(updatedTask.Flux.Equals(flux, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public async System.Threading.Tasks.Task FindTaskById()
        {
            string taskName = GenerateName("it task");

            Task task = await _taskClient.CreateTaskCron(taskName, TASK_FLUX, "0 2 * * *", _user, _organization);
            
            Task taskById = await _taskClient.FindTaskById(task.Id);
            
            Assert.IsNotNull(taskById);
            Assert.IsNotEmpty(task.Id);
            Assert.AreEqual(task.Id, taskById.Id);
            Assert.AreEqual(task.Name, taskById.Name);
            Assert.AreEqual(task.Owner.Id, taskById.Owner.Id);
            Assert.AreEqual(task.OrganizationId, taskById.OrganizationId);
            Assert.AreEqual(task.Status, taskById.Status);
            Assert.AreEqual(task.Offset, taskById.Offset);
            Assert.AreEqual(task.Flux, taskById.Flux);
            Assert.AreEqual(task.Cron, taskById.Cron);
        }
        
        [Test]
        public async System.Threading.Tasks.Task FindTaskByIdNull()
        {
            Task task = await _taskClient.FindTaskById("020f755c3d082000");
            
            Assert.IsNull(task);
        }

        [Test]
        public async System.Threading.Tasks.Task FindTasks()
        {
            var count = (await _taskClient.FindTasks()).Count;

            await _taskClient.CreateTaskCron(GenerateName("it task"), TASK_FLUX, "0 2 * * *", _user, _organization);

            var tasks = await _taskClient.FindTasks();

            Assert.AreEqual(count + 1, tasks.Count);
        }
        
        [Test]
        public async System.Threading.Tasks.Task FindTasksByUser()
        {
            var taskUser = await PlatformClient.CreateUserClient().CreateUser(GenerateName("Task user"));

            var count = (await _taskClient.FindTasksByUser(taskUser)).Count;
            Assert.AreEqual(0, count);

            await _taskClient.CreateTaskCron(GenerateName("it task"), TASK_FLUX, "0 2 * * *", taskUser, _organization);

            var tasks = await _taskClient.FindTasksByUser(taskUser);

            Assert.AreEqual(1, tasks.Count);
        }
        
        [Test]
        public async System.Threading.Tasks.Task FindTasksByOrganization()
        {
            var taskOrg = await PlatformClient.CreateOrganizationClient().CreateOrganization(GenerateName("Task user"));

            var count = (await _taskClient.FindTasksByOrganization(taskOrg)).Count;
            Assert.AreEqual(0, count);

            await _taskClient.CreateTaskCron(GenerateName("it task"), TASK_FLUX, "0 2 * * *", _user, taskOrg);

            var tasks = await _taskClient.FindTasksByOrganization(taskOrg);

            Assert.AreEqual(1, tasks.Count);
        }
        
        [Test]
        public async System.Threading.Tasks.Task FindTasksAfterSpecifiedId()
        {
            var task1 = await _taskClient.CreateTaskCron(GenerateName("it task"), TASK_FLUX, "0 2 * * *", _user, _organization);
            var task2 = await _taskClient.CreateTaskCron(GenerateName("it task"), TASK_FLUX, "0 2 * * *", _user, _organization);

            List<Task> tasks = await  _taskClient.FindTasks(task1.Id, null, null);
            
            Assert.AreEqual(1, tasks.Count);
            Assert.AreEqual(task2.Id, tasks[0].Id);
        }
        
        [Test]
        public async System.Threading.Tasks.Task DeleteTask()
        {
            var task = await _taskClient.CreateTaskCron(GenerateName("it task"), TASK_FLUX, "0 2 * * *", _user, _organization);

            var foundTask = await _taskClient.FindTaskById(task.Id);
            Assert.IsNotNull(foundTask);

            await _taskClient.DeleteTask(task);
            foundTask = await _taskClient.FindTaskById(task.Id);

            Assert.IsNull(foundTask);
        }
        
        [Test]
        //TODO
        [Ignore("wait to fix task path :tid => :id")]
        public async System.Threading.Tasks.Task Member() {

            var task = await _taskClient.CreateTaskCron(GenerateName("it task"), TASK_FLUX, "0 2 * * *", _user, _organization);

            List<UserResourceMapping> members =  await _taskClient.GetMembers(task);
            Assert.AreEqual(0, members.Count);

            User user = await _userClient.CreateUser(GenerateName("Luke Health"));

            UserResourceMapping userResourceMapping = await _taskClient.AddMember(user, task);
            Assert.IsNotNull(userResourceMapping);
            Assert.AreEqual(userResourceMapping.ResourceId, task.Id);
            Assert.AreEqual(userResourceMapping.ResourceType, ResourceType.TaskResourceType);
            Assert.AreEqual(userResourceMapping.UserId, user.Id);
            Assert.AreEqual(userResourceMapping.UserType, UserResourceMapping.MemberType.Member);

            members = await _taskClient.GetMembers(task);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].ResourceId, task.Id);
            Assert.AreEqual(members[0].ResourceType, ResourceType.TaskResourceType);
            Assert.AreEqual(members[0].UserId, user.Id);
            Assert.AreEqual(members[0].UserType, UserResourceMapping.MemberType.Member);

            await _taskClient.DeleteMember(user, task);

            members = await _taskClient.GetMembers(task);
            Assert.AreEqual(0, members.Count);
        }
        
        [Test]
        //TODO
        [Ignore("wait to fix task path :tid => :id")]
        public async System.Threading.Tasks.Task Owner() {

            var task = await _taskClient.CreateTaskCron(GenerateName("it task"), TASK_FLUX, "0 2 * * *", _user.Id, _organization.Id);

            List<UserResourceMapping> owners =  await _taskClient.GetOwners(task);
            Assert.AreEqual(0, owners.Count);

            User user = await _userClient.CreateUser(GenerateName("Luke Health"));

            UserResourceMapping userResourceMapping = await _taskClient.AddOwner(user, task);
            Assert.IsNotNull(userResourceMapping);
            Assert.AreEqual(userResourceMapping.ResourceId, task.Id);
            Assert.AreEqual(userResourceMapping.ResourceType, ResourceType.TaskResourceType);
            Assert.AreEqual(userResourceMapping.UserId, user.Id);
            Assert.AreEqual(userResourceMapping.UserType, UserResourceMapping.MemberType.Owner);

            owners = await _taskClient.GetOwners(task);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual(owners[0].ResourceId, task.Id);
            Assert.AreEqual(owners[0].ResourceType, ResourceType.TaskResourceType);
            Assert.AreEqual(owners[0].UserId, user.Id);
            Assert.AreEqual(owners[0].UserType, UserResourceMapping.MemberType.Owner);

            await _taskClient.DeleteOwner(user, task);

            owners = await _taskClient.GetOwners(task);
            Assert.AreEqual(0, owners.Count);
        }

        [Test]
        public async System.Threading.Tasks.Task Runs()
        {
            var task = await _taskClient.CreateTaskEvery(GenerateName("it task"), TASK_FLUX, "1s", _user.Id, _organization.Id);

            Thread.Sleep(5_000);

            List<Run> runs = await _taskClient.GetRuns(task);
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
        public async System.Threading.Tasks.Task RunsNotExist()
        {
            List<Run> runs = await _taskClient.GetRuns("020f755c3c082000", _organization.Id);
            Assert.IsEmpty(runs);
        }

        [Test]
        public async System.Threading.Tasks.Task RunsByTime()
        {
            var now = DateTime.UtcNow;
            
            var task = await _taskClient.CreateTaskEvery(GenerateName("it task"), TASK_FLUX, "1s", _user.Id, _organization.Id);

            Thread.Sleep(5_000);

            List<Run> runs = await _taskClient.GetRuns(task, null, now, null);
            Assert.IsEmpty(runs);
            
            runs = await _taskClient.GetRuns(task, now, null, null);
            Assert.IsNotEmpty(runs);
        }

        [Test]
        public async System.Threading.Tasks.Task RunsLimit()
        {
            var task = await _taskClient.CreateTaskEvery(GenerateName("it task"), TASK_FLUX, "1s", _user, _organization);
            
            Thread.Sleep(5_000);
            
            List<Run> runs = await _taskClient.GetRuns(task, null, null, 1);
            Assert.AreEqual(1, runs.Count);
            
            runs = await _taskClient.GetRuns(task, null, null, null);
            Assert.Greater(runs.Count, 1);
        }
    }
}