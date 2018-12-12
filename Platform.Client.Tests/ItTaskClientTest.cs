using System;
using System.Linq;
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

        private Organization _organization;
        private User _user;

        [SetUp]
        public new async System.Threading.Tasks.Task SetUp()
        {
            _taskClient = PlatformClient.CreateTaskClient();
            _organization = (await PlatformClient.CreateOrganizationClient().FindOrganizations())
                .First(organization => organization.Name.Equals("my-org"));

            _user = await PlatformClient.CreateUserClient().Me();
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
    }
}