using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItWriteApiRaceTest : AbstractItClientTest
    {
        private async Task<List<Bucket>> CreateBuckets(int count = 1)
        {
            var organization = await FindMyOrg();

            var loggedUser = await Client.GetUsersApi().MeAsync();
            Assert.IsNotNull(loggedUser);

            var buckets = new List<Bucket>();
            var permissions = new List<Permission>();

            for (var i = 1; i <= count; i++)
            {
                var retention = new BucketRetentionRules(BucketRetentionRules.TypeEnum.Expire, 0);

                var bucket = await Client.GetBucketsApi()
                    .CreateBucketAsync(GenerateName($"race{i}"), retention, organization);

                buckets.Add(bucket);
                //
                // Add Permissions to read and write to the Bucket
                //
                var resource = new PermissionResource(
                    PermissionResource.TypeBuckets, bucket.Id, null, organization.Id);

                var readBucket = new Permission(Permission.ActionEnum.Read, resource);
                var writeBucket = new Permission(Permission.ActionEnum.Write, resource);

                permissions.Add(readBucket);
                permissions.Add(writeBucket);
            }

            var authorization = await Client.GetAuthorizationsApi()
                .CreateAuthorizationAsync(await FindMyOrg(), permissions);

            Client.Dispose();
            var options = new InfluxDBClientOptions.Builder().Url(InfluxDbUrl).AuthenticateToken(authorization.Token)
                .Org(organization.Id).Bucket(buckets[0].Id).Build();
            Client = InfluxDBClientFactory.Create(options);

            return buckets;
        }

        [Test]
        public async Task Race()
        {
            await CreateBuckets();
            var point = PointData.Measurement("race-test")
                .Tag("test", "stress")
                .Field("value", 1);

            const int RANGE = 1000;
            using (var gateStart = new ManualResetEventSlim(false))
            using (var gate = new CountdownEvent(RANGE))
            using (var gateEnd = new CountdownEvent(RANGE))
            {
                for (var i = 0; i < RANGE; i++)
                {
                    var trd = new Thread(() =>
                    {
                        gateStart.Wait();
                        using (var writer = Client.GetWriteApi())
                        {
                            writer.WritePoint(point);
                            gate.Signal();
                            gate.Wait();
                        }

                        gateEnd.Signal();
                    });
                    trd.Start();
                }

                gateStart.Set();
                gateEnd.Wait();
            }
        }

        [Test]
        public async Task BatchConsistency()
        {
            var options = WriteOptions.CreateNew().BatchSize(1_555).FlushInterval(10_000).Build();

            var batches = new List<WriteSuccessEvent>();
            await StressfulWriteAndValidate(1, 5, options, (sender, eventArgs) =>
            {
                if (eventArgs is WriteSuccessEvent successEvent)
                {
                    batches.Add(successEvent);
                }
            });

            foreach (var batch in batches)
            {
                var length = batch.LineProtocol.Split("\n").Length;

                Trace.WriteLine($"Count: {length} {batch.Bucket}");

                // last element flush the rest
                if (batches.Last() != batch)
                {
                    Assert.AreEqual(1_555, length);
                }
            }
        }

        [Test]
        public async Task MultipleBuckets()
        {
            await StressfulWriteAndValidate();
        }

        [Test]
        public async Task MultipleBucketsWithFlush()
        {
            var writeOptions = WriteOptions.CreateNew().FlushInterval(100).Build();

            await StressfulWriteAndValidate(writeOptions: writeOptions);
        }

        private async Task StressfulWriteAndValidate(int writerCount = 4, int secondsCount = 5,
            WriteOptions writeOptions = null, EventHandler eventHandler = null)
        {
            var buckets = await CreateBuckets(writerCount);

            using var countdownEvent = new CountdownEvent(1);
            using var writeApi = Client
                .GetWriteApi(writeOptions ?? WriteOptions.CreateNew().FlushInterval(20_000).Build());
            writeApi.EventHandler += eventHandler;

            var writers = new List<Writer>();
            for (var i = 1; i <= writerCount; i++)
            {
                var writer = new Writer(i, writeApi, countdownEvent, buckets[i - 1]);
                writers.Add(writer);
                var thread = new Thread(writer.Do);
                thread.Start();
            }

            // wait 
            Thread.Sleep(secondsCount * 1_000);

            // stop
            countdownEvent.Signal();

            // wait to finish
            Console.WriteLine("Wait to finish the writer...");
            writeApi.ReleaseAndClose(180_000);
            Console.WriteLine("Finished");

            // check successfully written
            foreach (var writer in writers) await writer.Check(Client.GetQueryApi());
        }
    }

    internal class Writer
    {
        private int Identifier { get; }
        private IWriteApi WriteApi { get; }
        private CountdownEvent CountdownEvent { get; }
        private Bucket Bucket { get; }
        private long _time;

        public Writer(int identifier, IWriteApi writeApi, CountdownEvent countdownEvent, Bucket bucket)
        {
            Identifier = identifier;
            WriteApi = writeApi ?? throw new ArgumentNullException(nameof(writeApi));
            CountdownEvent = countdownEvent ?? throw new ArgumentNullException(nameof(countdownEvent));
            Bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));
        }

        internal void Do()
        {
            while (!CountdownEvent.IsSet)
            {
                _time++;

                var point = PointData.Measurement($"writer-{Identifier}")
                    .Tag("test", "stress")
                    .Field("value", _time)
                    .Timestamp(_time, WritePrecision.Ns);

                WriteApi.WritePoint(point, Bucket.Id);

                if (Identifier == 1 && _time % 100_000 == 0)
                {
                    Console.WriteLine($"Generated point: {point.ToLineProtocol()}, bucket: {Bucket.Name}");
                }
            }

            if (Identifier == 1)
            {
                Console.WriteLine($"Generated points: {_time}");
            }
        }

        public async Task Check(QueryApi queryApi)
        {
            var query = $"from(bucket: \"{Bucket.Name}\") |> range(start: 0) |> count()";
            var value = (long)(await queryApi.QueryAsync(query))[0].Records[0].GetValue();

            Console.WriteLine($"Written count [{Identifier}]: {value}");

            Assert.GreaterOrEqual(_time, value);
        }
    }
}