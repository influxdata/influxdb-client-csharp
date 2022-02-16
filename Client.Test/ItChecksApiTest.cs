using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Domain;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItChecksApiTest : AbstractItClientTest
    {
        private string _orgId;
        private ChecksApi _checksApi;

        [SetUp]
        public new async Task SetUp()
        {
            _orgId = (await FindMyOrg()).Id;
            _checksApi = Client.GetChecksApi();

            var checks = await _checksApi.FindChecksAsync(_orgId, new FindOptions());
            foreach (var check in checks._Checks.Where(ne => ne.Name.EndsWith("-IT")))
                await _checksApi.DeleteCheckAsync(check);
        }

        [Test]
        public async Task CreateThresholdCheck()
        {
            var now = DateTime.UtcNow;

            var greater = new GreaterThreshold(value: 80F, level: CheckStatusLevel.CRIT, allValues: true,
                type: GreaterThreshold.TypeEnum.Greater);

            var lesser = new LesserThreshold(value: 20F, level: CheckStatusLevel.OK,
                type: LesserThreshold.TypeEnum.Lesser);

            var range = new RangeThreshold(min: 50F, max: 70F, level: CheckStatusLevel.WARN, within: false,
                type: RangeThreshold.TypeEnum.Range);

            var thresholds = new List<Threshold> { greater, lesser, range };
            var name = GenerateName("th-check");
            var threshold = await _checksApi.CreateThresholdCheckAsync(name,
                "from(bucket: \"foo\") |> range(start: -1d, stop: now()) |> aggregateWindow(every: 1m, fn: mean) |> filter(fn: (r) => r._field == \"usage_user\") |> yield()",
                "1h",
                "Check: ${ r._check_name } is: ${ r._level }",
                thresholds,
                _orgId);

            Assert.IsNotNull(threshold);
            Assert.AreEqual(ThresholdCheck.TypeEnum.Threshold, threshold.Type);
            Assert.AreEqual(3, threshold.Thresholds.Count);

            var greaterThreshold = (GreaterThreshold)threshold.Thresholds[0];
            Assert.AreEqual(GreaterThreshold.TypeEnum.Greater, greaterThreshold.Type);
            Assert.AreEqual(80F, greaterThreshold.Value);
            Assert.AreEqual(CheckStatusLevel.CRIT, greater.Level);
            Assert.IsTrue(greaterThreshold.AllValues);

            var lesserThreshold = (LesserThreshold)threshold.Thresholds[1];
            Assert.AreEqual(LesserThreshold.TypeEnum.Lesser, lesserThreshold.Type);
            Assert.AreEqual(20F, lesserThreshold.Value);
            Assert.AreEqual(CheckStatusLevel.OK, lesserThreshold.Level);
            Assert.IsFalse(lesserThreshold.AllValues);

            var rangeThreshold = (RangeThreshold)threshold.Thresholds[2];
            Assert.AreEqual(RangeThreshold.TypeEnum.Range, rangeThreshold.Type);
            Assert.AreEqual(50F, rangeThreshold.Min);
            Assert.AreEqual(70F, rangeThreshold.Max);
            Assert.AreEqual(CheckStatusLevel.WARN, rangeThreshold.Level);
            Assert.IsFalse(rangeThreshold.AllValues);
            Assert.IsFalse(rangeThreshold.Within);

            Assert.IsNotEmpty(threshold.Id);
            Assert.AreEqual(name, threshold.Name);
            Assert.AreEqual(_orgId, threshold.OrgID);
            Assert.Greater(threshold.CreatedAt, now);
            Assert.Greater(threshold.UpdatedAt, now);
            Assert.IsNotNull(threshold.Query);
            Assert.AreEqual(
                "from(bucket: \"foo\") |> range(start: -1d, stop: now()) |> aggregateWindow(every: 1m, fn: mean) |> filter(fn: (r) => r._field == \"usage_user\") |> yield()",
                threshold.Query.Text);

            Assert.AreEqual(TaskStatusType.Active, threshold.Status);
            Assert.AreEqual("1h", threshold.Every);
            Assert.IsNull(threshold.Offset);
            Assert.IsNull(threshold.Tags);
            Assert.IsNull(threshold.Description);
            Assert.AreEqual("Check: ${ r._check_name } is: ${ r._level }", threshold.StatusMessageTemplate);
            Assert.IsEmpty(threshold.Labels);
            Assert.IsNotNull(threshold.Links);
            Assert.AreEqual($"/api/v2/checks/{threshold.Id}", threshold.Links.Self);
            Assert.AreEqual($"/api/v2/checks/{threshold.Id}/labels", threshold.Links.Labels);
            Assert.AreEqual($"/api/v2/checks/{threshold.Id}/members", threshold.Links.Members);
            Assert.AreEqual($"/api/v2/checks/{threshold.Id}/owners", threshold.Links.Owners);
        }

        [Test]
        public async Task CreateDeadmanCheck()
        {
            var now = DateTime.UtcNow;

            var name = GenerateName("deadman-check");
            var deadman = await _checksApi.CreateDeadmanCheckAsync(name,
                "from(bucket: \"foo\") |> range(start: -1d, stop: now()) |> aggregateWindow(every: 1m, fn: mean) |> filter(fn: (r) => r._field == \"usage_user\") |> yield()",
                "15m",
                "90s",
                "10m",
                "Check: ${ r._check_name } is: ${ r._level }",
                CheckStatusLevel.CRIT,
                _orgId);

            Assert.IsNotNull(deadman);
            Assert.AreEqual(DeadmanCheck.TypeEnum.Deadman, deadman.Type);
            Assert.AreEqual("90s", deadman.TimeSince);
            Assert.AreEqual("10m", deadman.StaleTime);
            Assert.AreEqual(CheckStatusLevel.CRIT, deadman.Level);

            Assert.IsNotEmpty(deadman.Id);
            Assert.AreEqual(name, deadman.Name);
            Assert.AreEqual(_orgId, deadman.OrgID);
            Assert.Greater(deadman.CreatedAt, now);
            Assert.Greater(deadman.UpdatedAt, now);
            Assert.IsNotNull(deadman.Query);
            Assert.AreEqual(
                "from(bucket: \"foo\") |> range(start: -1d, stop: now()) |> aggregateWindow(every: 1m, fn: mean) |> filter(fn: (r) => r._field == \"usage_user\") |> yield()",
                deadman.Query.Text);

            Assert.AreEqual(TaskStatusType.Active, deadman.Status);
            Assert.AreEqual("15m", deadman.Every);
            Assert.IsNull(deadman.Offset);
            Assert.IsNull(deadman.Tags);
            Assert.IsNull(deadman.Description);
            Assert.AreEqual("Check: ${ r._check_name } is: ${ r._level }", deadman.StatusMessageTemplate);
            Assert.IsEmpty(deadman.Labels);
            Assert.IsNotNull(deadman.Links);
            Assert.AreEqual($"/api/v2/checks/{deadman.Id}", deadman.Links.Self);
            Assert.AreEqual($"/api/v2/checks/{deadman.Id}/labels", deadman.Links.Labels);
            Assert.AreEqual($"/api/v2/checks/{deadman.Id}/members", deadman.Links.Members);
            Assert.AreEqual($"/api/v2/checks/{deadman.Id}/owners", deadman.Links.Owners);
        }

        [Test]
        public async Task UpdateCheck()
        {
            var greater = new GreaterThreshold(value: 80F, level: CheckStatusLevel.CRIT, allValues: true,
                type: GreaterThreshold.TypeEnum.Greater);

            var name = GenerateName("th-check");
            var threshold = await _checksApi.CreateThresholdCheckAsync(name,
                "from(bucket: \"foo\") |> range(start: -1d, stop: now()) |> aggregateWindow(every: 1m, fn: mean) |> filter(fn: (r) => r._field == \"usage_user\") |> yield()",
                "1h",
                "Check: ${ r._check_name } is: ${ r._level }",
                greater,
                _orgId);

            var updatedName = GenerateName("updated name");
            threshold.Name = updatedName;
            threshold.Description = "updated description";
            threshold.Status = TaskStatusType.Inactive;

            threshold = (ThresholdCheck)await _checksApi.UpdateCheckAsync(threshold);

            Assert.AreEqual(updatedName, threshold.Name);
            Assert.AreEqual("updated description", threshold.Description);
            Assert.AreEqual(TaskStatusType.Inactive, threshold.Status);
        }

        [Test]
        public void UpdateCheckNotExists()
        {
            var update = new CheckPatch("not exits name", "not exists update",
                CheckPatch.StatusEnum.Active);

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _checksApi
                .UpdateCheckAsync("020f755c3c082000", update));

            Assert.AreEqual("check not found for key \"020f755c3c082000\"", ioe.Message);
        }

        [Test]
        public async Task DeleteCheck()
        {
            var greater = new GreaterThreshold(value: 80F, level: CheckStatusLevel.CRIT, allValues: true,
                type: GreaterThreshold.TypeEnum.Greater);

            var name = GenerateName("th-check");
            var created = await _checksApi.CreateThresholdCheckAsync(name,
                "from(bucket: \"foo\") |> range(start: -1d, stop: now()) |> aggregateWindow(every: 1m, fn: mean) |> filter(fn: (r) => r._field == \"usage_user\") |> yield()",
                "1h",
                "Check: ${ r._check_name } is: ${ r._level }",
                greater,
                _orgId);

            var found = await _checksApi.FindCheckByIdAsync(created.Id);

            await _checksApi.DeleteCheckAsync(found);

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _checksApi
                .FindCheckByIdAsync("020f755c3c082000"));

            Assert.AreEqual("check not found for key \"020f755c3c082000\"", ioe.Message);
        }

        [Test]
        public void DeleteCheckNotFound()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _checksApi
                .DeleteCheckAsync("020f755c3c082000"));

            Assert.AreEqual("check not found for key \"020f755c3c082000\"", ioe.Message);
        }

        [Test]
        public async Task FindCheckById()
        {
            var greater = new GreaterThreshold(value: 80F, level: CheckStatusLevel.CRIT, allValues: true,
                type: GreaterThreshold.TypeEnum.Greater);

            var name = GenerateName("th-check");
            var check = await _checksApi.CreateThresholdCheckAsync(name,
                "from(bucket: \"foo\") |> range(start: -1d, stop: now()) |> aggregateWindow(every: 1m, fn: mean) |> filter(fn: (r) => r._field == \"usage_user\") |> yield()",
                "1h",
                "Check: ${ r._check_name } is: ${ r._level }",
                greater,
                _orgId);

            var found = (ThresholdCheck)await _checksApi.FindCheckByIdAsync(check.Id);

            Assert.AreEqual(check.Id, found.Id);
        }

        [Test]
        public void FindCheckByIdNotFound()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _checksApi
                .FindCheckByIdAsync("020f755c3c082000"));

            Assert.AreEqual("check not found for key \"020f755c3c082000\"", ioe.Message);
        }

        [Test]
        public async Task Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var greater = new GreaterThreshold(value: 80F, level: CheckStatusLevel.CRIT, allValues: true,
                type: GreaterThreshold.TypeEnum.Greater);

            var name = GenerateName("th-check");
            var check = await _checksApi.CreateThresholdCheckAsync(name,
                "from(bucket: \"foo\") |> range(start: -1d, stop: now()) |> aggregateWindow(every: 1m, fn: mean) |> filter(fn: (r) => r._field == \"usage_user\") |> yield()",
                "1h",
                "Check: ${ r._check_name } is: ${ r._level }",
                greater,
                _orgId);

            var properties = new Dictionary<string, string> { { "color", "green" }, { "location", "west" } };

            var label = await labelClient.CreateLabelAsync(GenerateName("Cool Resource"), properties, _orgId);

            var labels = await _checksApi.GetLabelsAsync(check);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = await _checksApi.AddLabelAsync(label, check);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = await _checksApi.GetLabelsAsync(check);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            await _checksApi.DeleteLabelAsync(label, check);

            labels = await _checksApi.GetLabelsAsync(check);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        public async Task FindChecks()
        {
            var size = (await _checksApi.FindChecksAsync(_orgId)).Count;

            var greater = new GreaterThreshold(value: 80F, level: CheckStatusLevel.CRIT, allValues: true,
                type: GreaterThreshold.TypeEnum.Greater);

            await _checksApi.CreateThresholdCheckAsync(GenerateName("th-check"),
                "from(bucket: \"foo\") |> range(start: -1d, stop: now()) |> aggregateWindow(every: 1m, fn: mean) |> filter(fn: (r) => r._field == \"usage_user\") |> yield()",
                "1h",
                "Check: ${ r._check_name } is: ${ r._level }",
                greater,
                _orgId);

            var checks = await _checksApi.FindChecksAsync(_orgId);
            Assert.AreEqual(size + 1, checks.Count);
        }

        [Test]
        public async Task FindChecksPaging()
        {
            var greater = new GreaterThreshold(value: 80F, level: CheckStatusLevel.CRIT, allValues: true,
                type: GreaterThreshold.TypeEnum.Greater);

            foreach (var unused in Enumerable.Range(0,
                         20 - (await _checksApi.FindChecksAsync(_orgId, new FindOptions()))
                         ._Checks.Count))
                await _checksApi.CreateThresholdCheckAsync(GenerateName("th-check"),
                    "from(bucket: \"foo\") |> range(start: -1d, stop: now()) |> aggregateWindow(every: 1m, fn: mean) |> filter(fn: (r) => r._field == \"usage_user\") |> yield()",
                    "1h",
                    "Check: ${ r._check_name } is: ${ r._level }",
                    greater,
                    _orgId);

            var findOptions = new FindOptions { Limit = 5 };

            var checks = await _checksApi.FindChecksAsync(_orgId, findOptions);
            Assert.AreEqual(5, checks._Checks.Count);
            Assert.AreEqual($"/api/v2/checks?descending=false&limit=5&offset=5&orgID={_orgId}", checks.Links.Next);

            checks = await _checksApi.FindChecksAsync(_orgId,
                FindOptions.GetFindOptions(checks.Links.Next));
            Assert.AreEqual(5, checks._Checks.Count);
            Assert.AreEqual($"/api/v2/checks?descending=false&limit=5&offset=10&orgID={_orgId}", checks.Links.Next);

            checks = await _checksApi.FindChecksAsync(_orgId,
                FindOptions.GetFindOptions(checks.Links.Next));
            Assert.AreEqual(5, checks._Checks.Count);
            Assert.AreEqual($"/api/v2/checks?descending=false&limit=5&offset=15&orgID={_orgId}", checks.Links.Next);

            checks = await _checksApi.FindChecksAsync(_orgId,
                FindOptions.GetFindOptions(checks.Links.Next));
            Assert.AreEqual(5, checks._Checks.Count);
            Assert.AreEqual($"/api/v2/checks?descending=false&limit=5&offset=20&orgID={_orgId}", checks.Links.Next);

            checks = await _checksApi.FindChecksAsync(_orgId,
                FindOptions.GetFindOptions(checks.Links.Next));
            Assert.AreEqual(0, checks._Checks.Count);
            Assert.IsNull(checks.Links.Next);
        }
    }
}