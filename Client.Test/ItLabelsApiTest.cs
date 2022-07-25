using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItLabelsApiTest : AbstractItClientTest
    {
        [SetUp]
        public new async Task SetUp()
        {
            _labelsApi = Client.GetLabelsApi();

            foreach (var label in (await _labelsApi.FindLabelsAsync()).Where(label => label.Name.EndsWith("-IT")))
                await _labelsApi.DeleteLabelAsync(label.Id);

            _organization = await FindMyOrg();
        }

        private LabelsApi _labelsApi;
        private Organization _organization;

        [Test]
        public async Task CloneLabel()
        {
            var name = GenerateName("cloned");

            var properties = new Dictionary<string, string> { { "color", "green" }, { "location", "west" } };

            var label = await _labelsApi.CreateLabelAsync(GenerateName("Cool Resource"), properties, _organization.Id);

            var cloned = await _labelsApi.CloneLabelAsync(name, label);

            Assert.AreEqual(name, cloned.Name);

            Assert.AreEqual(2, cloned.Properties.Count);
            Assert.AreEqual("green", cloned.Properties["color"]);
            Assert.AreEqual("west", cloned.Properties["location"]);
        }

        [Test]
        public void CloneLabelNotFound()
        {
            var exception =
                Assert.ThrowsAsync<NotFoundException>(async () =>
                    await _labelsApi.CloneLabelAsync(GenerateName("bucket"), "020f755c3c082000"));

            Assert.IsNotNull(exception);
            Assert.AreEqual(typeof(NotFoundException), exception.GetType());
            Assert.AreEqual("label not found", exception.Message);
        }

        [Test]
        public async Task CreateLabel()
        {
            var name = GenerateName("Cool Resource");

            var properties = new Dictionary<string, string> { { "color", "red" }, { "source", "remote api" } };

            var label = await _labelsApi.CreateLabelAsync(name, properties, _organization.Id);

            Assert.IsNotNull(label);
            Assert.IsNotEmpty(label.Id);
            Assert.AreEqual(name, label.Name);
            Assert.AreEqual(2, label.Properties.Count);
            Assert.AreEqual("red", label.Properties["color"]);
            Assert.AreEqual("remote api", label.Properties["source"]);
        }

        [Test]
        public async Task CreateLabelEmptyProperties()
        {
            var name = GenerateName("Cool Resource");

            var request = new LabelCreateRequest(_organization.Id, name);

            var label = await _labelsApi.CreateLabelAsync(request);

            Assert.IsNotNull(label);
            Assert.IsNotEmpty(label.Id);
            Assert.AreEqual(name, label.Name);
        }

        [Test]
        public async Task DeleteLabel()
        {
            var createdLabel =
                await _labelsApi.CreateLabelAsync(GenerateName("Cool Resource"), new Dictionary<string, string>(),
                    _organization.Id);
            Assert.IsNotNull(createdLabel);

            var foundLabel = await _labelsApi.FindLabelByIdAsync(createdLabel.Id);
            Assert.IsNotNull(foundLabel);

            // delete user
            await _labelsApi.DeleteLabelAsync(createdLabel);

            var exception =
                Assert.ThrowsAsync<NotFoundException>(async () => await _labelsApi.FindLabelByIdAsync(createdLabel.Id));

            Assert.IsNotNull(exception);
            Assert.AreEqual("label not found", exception.Message);
            Assert.AreEqual(typeof(NotFoundException), exception.GetType());
        }

        [Test]
        public async Task FindLabelById()
        {
            var label = await _labelsApi.CreateLabelAsync(GenerateName("Cool Resource"),
                new Dictionary<string, string>(),
                _organization.Id);

            var labelById = await _labelsApi.FindLabelByIdAsync(label.Id);

            Assert.IsNotNull(label);
            Assert.AreEqual(label.Id, labelById.Id);
            Assert.AreEqual(label.Name, labelById.Name);
        }

        [Test]
        public void FindLabelByIdNull()
        {
            var exception =
                Assert.ThrowsAsync<NotFoundException>(async () =>
                    await _labelsApi.FindLabelByIdAsync("020f755c3c082000"));

            Assert.IsNotNull(exception);
            Assert.AreEqual("label not found", exception.Message);
            Assert.AreEqual(typeof(NotFoundException), exception.GetType());
        }

        [Test]
        public async Task FindLabels()
        {
            var size = (await _labelsApi.FindLabelsAsync()).Count;

            await _labelsApi.CreateLabelAsync(GenerateName("Cool Resource"), new Dictionary<string, string>(),
                _organization.Id);

            var labels = await _labelsApi.FindLabelsAsync();
            Assert.AreEqual(size + 1, labels.Count);
        }

        [Test]
        public async Task FindLabelsByOrganization()
        {
            var organization = await Client.GetOrganizationsApi().CreateOrganizationAsync(GenerateName("org"));

            var labels = await _labelsApi.FindLabelsByOrgIdAsync(organization.Id);
            Assert.AreEqual(0, labels.Count);

            var properties = new Dictionary<string, string> { { "color", "green" }, { "location", "west" } };

            await _labelsApi.CreateLabelAsync(GenerateName("Cool Resource"), properties, organization.Id);

            labels = await _labelsApi.FindLabelsByOrgAsync(organization);
            Assert.AreEqual(1, labels.Count);

            await _labelsApi.DeleteLabelAsync(labels.First());

            labels = await _labelsApi.FindLabelsByOrgIdAsync(organization.Id);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        public async Task UpdateLabel()
        {
            var label = await _labelsApi.CreateLabelAsync(GenerateName("Cool Resource"),
                new Dictionary<string, string>(),
                _organization.Id);
            Assert.IsNull(label.Properties);

            label.Properties = new Dictionary<string, string> { { "color", "blue" } };

            label = await _labelsApi.UpdateLabelAsync(label);
            Assert.AreEqual(1, label.Properties.Count);
            Assert.AreEqual("blue", label.Properties["color"]);

            label.Properties.Add("type", "free");

            label = await _labelsApi.UpdateLabelAsync(label);
            Assert.AreEqual(2, label.Properties.Count);
            Assert.AreEqual("blue", label.Properties["color"]);
            Assert.AreEqual("free", label.Properties["type"]);

            label.Properties["type"] = "paid";
            label.Properties["color"] = "";

            label = await _labelsApi.UpdateLabelAsync(label);
            Assert.AreEqual(1, label.Properties.Count);
            Assert.AreEqual("paid", label.Properties["type"]);
        }
    }
}