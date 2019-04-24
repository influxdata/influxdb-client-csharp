using System.Collections.Generic;
using System.Linq;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Api.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItLabelsApiTest : AbstractItClientTest
    {
        [SetUp]
        public new void SetUp()
        {
            _labelsApi = Client.GetLabelsApi();

            foreach (var label in _labelsApi.FindLabels().Where(label => label.Name.EndsWith("-IT")))
                _labelsApi.DeleteLabel(label.Id);

            _organization = FindMyOrg();
        }

        private LabelsApi _labelsApi;
        private Organization _organization;

        [Test]
        public void CloneLabel()
        {
            var name = GenerateName("cloned");

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = _labelsApi.CreateLabel(GenerateName("Cool Resource"), properties, _organization.Id);

            var cloned = _labelsApi.CloneLabel(name, label);

            Assert.AreEqual(name, cloned.Name);

            Assert.AreEqual(2, cloned.Properties.Count);
            Assert.AreEqual("green", cloned.Properties["color"]);
            Assert.AreEqual("west", cloned.Properties["location"]);
        }

        [Test]
        public void CloneLabelNotFound()
        {
            var exception =
                Assert.Throws<HttpException>(() => _labelsApi.CloneLabel(GenerateName("bucket"), "020f755c3c082000"));

            Assert.IsNotNull(exception);
            Assert.AreEqual("label not found", exception.Message);
        }

        [Test]
        public void CreateLabel()
        {
            var name = GenerateName("Cool Resource");

            var properties = new Dictionary<string, string> {{"color", "red"}, {"source", "remote api"}};

            var label = _labelsApi.CreateLabel(name, properties, _organization.Id);

            Assert.IsNotNull(label);
            Assert.IsNotEmpty(label.Id);
            Assert.AreEqual(name, label.Name);
            Assert.AreEqual(2, label.Properties.Count);
            Assert.AreEqual("red", label.Properties["color"]);
            Assert.AreEqual("remote api", label.Properties["source"]);
        }

        [Test]
        public void CreateLabelEmptyProperties()
        {
            var name = GenerateName("Cool Resource");

            var request = new LabelCreateRequest(_organization.Id, name);

            var label = _labelsApi.CreateLabel(request);

            Assert.IsNotNull(label);
            Assert.IsNotEmpty(label.Id);
            Assert.AreEqual(name, label.Name);
        }

        [Test]
        public void DeleteLabel()
        {
            var createdLabel =
                _labelsApi.CreateLabel(GenerateName("Cool Resource"), new Dictionary<string, string>(),
                    _organization.Id);
            Assert.IsNotNull(createdLabel);

            var foundLabel = _labelsApi.FindLabelById(createdLabel.Id);
            Assert.IsNotNull(foundLabel);

            // delete user
            _labelsApi.DeleteLabel(createdLabel);

            var exception = Assert.Throws<HttpException>(() => _labelsApi.FindLabelById(createdLabel.Id));

            Assert.IsNotNull(exception);
            Assert.AreEqual("label not found", exception.Message);
        }

        [Test]
        public void FindLabelById()
        {
            var label = _labelsApi.CreateLabel(GenerateName("Cool Resource"), new Dictionary<string, string>(),
                _organization.Id);

            var labelById = _labelsApi.FindLabelById(label.Id);

            Assert.IsNotNull(label);
            Assert.AreEqual(label.Id, labelById.Id);
            Assert.AreEqual(label.Name, labelById.Name);
        }

        [Test]
        public void FindLabelByIdNull()
        {
            var exception = Assert.Throws<HttpException>(() => _labelsApi.FindLabelById("020f755c3c082000"));

            Assert.IsNotNull(exception);
            Assert.AreEqual("label not found", exception.Message);
        }

        [Test]
        public void FindLabels()
        {
            var size = _labelsApi.FindLabels().Count;

            _labelsApi.CreateLabel(GenerateName("Cool Resource"), new Dictionary<string, string>(), _organization.Id);

            var labels = _labelsApi.FindLabels();
            Assert.AreEqual(size + 1, labels.Count);
        }

        [Test]
        public void FindLabelsByOrganization()
        {
            var organization = Client.GetOrganizationsApi().CreateOrganization(GenerateName("org"));

            var labels = _labelsApi.FindLabelsByOrgId(organization.Id);
            Assert.AreEqual(0, labels.Count);

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            _labelsApi.CreateLabel(GenerateName("Cool Resource"), properties, organization.Id);

            labels = _labelsApi.FindLabelsByOrgId(organization.Id);
            Assert.AreEqual(1, labels.Count);

            _labelsApi.DeleteLabel(labels.First());

            labels = _labelsApi.FindLabelsByOrgId(organization.Id);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        public void UpdateLabel()
        {
            var label = _labelsApi.CreateLabel(GenerateName("Cool Resource"), new Dictionary<string, string>(),
                _organization.Id);
            Assert.IsNull(label.Properties);

            label.Properties = new Dictionary<string, string> {{"color", "blue"}};

            label = _labelsApi.UpdateLabel(label);
            Assert.AreEqual(1, label.Properties.Count);
            Assert.AreEqual("blue", label.Properties["color"]);

            label.Properties.Add("type", "free");

            label = _labelsApi.UpdateLabel(label);
            Assert.AreEqual(2, label.Properties.Count);
            Assert.AreEqual("blue", label.Properties["color"]);
            Assert.AreEqual("free", label.Properties["type"]);

            label.Properties["type"] = "paid";
            label.Properties["color"] = "";

            label = _labelsApi.UpdateLabel(label);
            Assert.AreEqual(1, label.Properties.Count);
            Assert.AreEqual("paid", label.Properties["type"]);
        }
    }
}