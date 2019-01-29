using System.Collections.Generic;
using System.Linq;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    [TestFixture]
    public class ItLabelClientTest : AbstractItClientTest
    {
        private LabelClient _labelClient;

        [SetUp]
        public new async Task SetUp()
        {
            _labelClient = PlatformClient.CreateLabelClient();

            foreach (var bucket in (await _labelClient.FindLabels()).Where(label => label.Name.EndsWith("-IT")))
            {
                await _labelClient.DeleteLabel(bucket);
            }
        }

        [Test]
        public async Task CreateLabel()
        {
            var name = GenerateName("Cool Resource");

            var properties = new Dictionary<string, string> {{"color", "red"}, {"source", "remote api"}};

            var label = await _labelClient.CreateLabel(name, properties);

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

            var label = new Label {Name = name};

            label = await _labelClient.CreateLabel(label);

            Assert.IsNotNull(label);
            Assert.IsNotEmpty(label.Id);
            Assert.AreEqual(name, label.Name);
        }

        [Test]
        public async Task FindLabelById()
        {
            var label = await _labelClient.CreateLabel(GenerateName("Cool Resource"), new Dictionary<string, string> ());

            var labelById = await _labelClient.FindLabelById(label.Id);

            Assert.IsNotNull(label);
            Assert.AreEqual(label.Id, labelById.Id);
            Assert.AreEqual(label.Name, labelById.Name);
        }

        [Test]
        public async Task FindLabelByIdNull()
        {
            var labelById =  await _labelClient.FindLabelById("020f755c3c082000");

            Assert.IsNull(labelById);
        }

        [Test]
        public async Task FindLabels()
        {
            var size = (await _labelClient.FindLabels()).Count;

            await _labelClient.CreateLabel(GenerateName("Cool Resource"), new Dictionary<string, string>());

            var labels = await _labelClient.FindLabels();
            Assert.AreEqual(size + 1, labels.Count);
        }

        [Test]
        public async Task DeleteLabel()
        {
            var createdLabel =  await _labelClient.CreateLabel(GenerateName("Cool Resource"), new Dictionary<string, string>());
            Assert.IsNotNull(createdLabel);

            var foundLabel = await _labelClient.FindLabelById(createdLabel.Id);
            Assert.IsNotNull(foundLabel);

            // delete user
            await _labelClient.DeleteLabel(createdLabel);

            foundLabel = await _labelClient.FindLabelById(createdLabel.Id);
            Assert.IsNull(foundLabel);
        }

        [Test]
        public async Task UpdateLabel()
        {
            var label = await _labelClient.CreateLabel(GenerateName("Cool Resource"), new Dictionary<string, string>());
            Assert.AreEqual(0, label.Properties.Count);

            label.Properties.Add("color", "blue");

            label = await _labelClient.UpdateLabel(label);
            Assert.AreEqual(1, label.Properties.Count);
            Assert.AreEqual("blue", label.Properties["color"]);

            label.Properties.Add("type", "free");

            label = await _labelClient.UpdateLabel(label);
            Assert.AreEqual(2, label.Properties.Count);
            Assert.AreEqual("blue", label.Properties["color"]);
            Assert.AreEqual("free", label.Properties["type"]);

            label.Properties["type"] = "paid";
            label.Properties["color"] = "";

            label = await _labelClient.UpdateLabel(label);
            Assert.AreEqual(1, label.Properties.Count);
            Assert.AreEqual("paid", label.Properties["type"]);
        }
    }
}