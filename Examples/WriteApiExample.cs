using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;

namespace Examples
{
    public static class WriteApiExample
    {
        [Measurement("influxPoint")]
        private class InfluxPoint
        {
            [Column("writeType", IsTag = true)] public string WriteType { get; set; }

            [Column("value")] public double Value { get; set; }

            [Column(IsTimestamp = true)] public DateTime Time { get; set; }
        }

        public static async Task Main()
        {
            Console.WriteLine(
                "================================= BasicEventHandler =================================");
            await BasicEventHandler();
            Console.WriteLine(
                "================================ CustomEventListener ================================");
            await CustomEventListener();
        }

        private static Task BasicEventHandler()
        {
            using var client = InfluxDBClientFactory.Create("http://localhost:9999",
                "my-user", "my-password".ToCharArray());

            var options = WriteOptions.CreateNew()
                .BatchSize(1)
                .FlushInterval(1000)
                .RetryInterval(2000)
                .MaxRetries(3)
                .Build();

            //
            // Write Data
            //
            using (var writeApi = client.GetWriteApi(options))
            {
                //
                // Handle the Events 
                //
                writeApi.EventHandler += (sender, eventArgs) =>
                {
                    switch (eventArgs)
                    {
                        // success response from server
                        case WriteSuccessEvent successEvent:
                            Console.WriteLine("WriteSuccessEvent: point was successfully written to InfluxDB");
                            Console.WriteLine($"  - {successEvent.LineProtocol}");
                            Console.WriteLine(
                                "-----------------------------------------------------------------------");
                            break;

                        // unhandled exception from server
                        case WriteErrorEvent errorEvent:
                            Console.WriteLine($"WriteErrorEvent: {errorEvent.Exception.Message}");
                            Console.WriteLine($"  - {errorEvent.LineProtocol}");
                            Console.WriteLine(
                                "-----------------------------------------------------------------------");
                            break;

                        // retrievable error from server
                        case WriteRetriableErrorEvent error:
                            Console.WriteLine($"WriteErrorEvent: {error.Exception.Message}");
                            Console.WriteLine($"  - {error.LineProtocol}");
                            Console.WriteLine(
                                "-----------------------------------------------------------------------");
                            break;

                        // runtime exception in background batch processing
                        case WriteRuntimeExceptionEvent error:
                            Console.WriteLine($"WriteRuntimeExceptionEvent: {error.Exception.Message}");
                            Console.WriteLine(
                                "-----------------------------------------------------------------------");
                            break;
                    }
                };

                //
                // Write by LineProtocol
                //
                var dateTimeOffset = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                writeApi.WriteRecord($"influxPoint,writeType=lineProtocol value=11.11 {dateTimeOffset}",
                    WritePrecision.Ns, "my-bucket", "my-org");

                //
                // Write by LineProtocol - bad timestamp
                //
                writeApi.WriteRecord($"influxPoint,writeType=lineProtocol value=11.11 {DateTime.UtcNow}",
                    WritePrecision.Ns, "my-bucket", "my-org");

                //
                // Write by Data Point
                //               
                var point = PointData.Measurement("influxPoint")
                    .Tag("writeType", "dataPoint")
                    .Field("value", 22.22)
                    .Timestamp(DateTime.UtcNow.AddSeconds(-10), WritePrecision.Ns);

                writeApi.WritePoint(point, "my-bucket", "my-org");

                //
                // Write by Data Point - bad "value" type
                //               
                var wrongPoint = PointData.Measurement("influxPoint")
                    .Tag("writeType", "dataPoint")
                    .Field("value", "22.22")
                    .Timestamp(DateTime.UtcNow.AddSeconds(-10), WritePrecision.Ns);

                writeApi.WritePoint(wrongPoint, "my-bucket", "my-org");

                //
                // Write by POCO
                //
                var influxPoint = new InfluxPoint { WriteType = "POCO", Value = 33.33, Time = DateTime.UtcNow };

                writeApi.WriteMeasurement(influxPoint, WritePrecision.Ns, "my-bucket", "my-org");

                //
                // Write by POCO - bucket not found
                //
                writeApi.WriteMeasurement(influxPoint, WritePrecision.Ns, "my-bucket2", "my-org");

                //
                // Write list of points by POCO 
                //
                var pointsToWrite = new List<InfluxPoint>();
                for (var i = 1; i <= 5; i++)
                    pointsToWrite.Add(new InfluxPoint
                        { WriteType = "POCO", Value = i, Time = DateTime.UtcNow.AddSeconds(-i) });

                writeApi.WriteMeasurements(pointsToWrite, WritePrecision.Ns, "my-bucket", "my-org");
            }

            return Task.CompletedTask;
        }

        private static Task CustomEventListener()
        {
            using var client = InfluxDBClientFactory.Create("http://localhost:9999/",
                "my-user", "my-password".ToCharArray());

            var options = WriteOptions.CreateNew()
                .BatchSize(5)
                .FlushInterval(1000)
                .RetryInterval(2000)
                .MaxRetries(3)
                .Build();
            //
            // Write Data
            //
            using (var writeApi = client.GetWriteApi(options))
            {
                //
                // Init EventListener
                //
                var listener = new EventListener(writeApi);

                //
                // Write by POCO 
                //
                var pointsToWrite = new List<InfluxPoint>();
                for (var i = 1; i <= 5; i++)
                    pointsToWrite.Add(new InfluxPoint
                        { WriteType = "POCO", Value = i, Time = DateTime.UtcNow.AddSeconds(-i) });

                writeApi.WriteMeasurements(pointsToWrite, WritePrecision.Ns, "my-bucket", "my-org");

                //
                // Wait to Success Response
                //
                listener.WaitToSuccess();

                //
                // Write by Data Point
                //               
                var point = PointData.Measurement("influxPoint")
                    .Tag("writeType", "dataPoint")
                    .Field("value", 22.22)
                    .Timestamp(DateTime.UtcNow.AddSeconds(-10), WritePrecision.Ns);

                writeApi.WritePoint(point, "my-bucket", "my-org");
                listener.WaitToResponse();

                //
                // Write by LineProtocol - bad timestamp
                //
                writeApi.WriteRecord($"influxPoint,writeType=lineProtocol value=11.11 {DateTime.UtcNow}",
                    WritePrecision.Ns, "my-bucket", "my-org");
                listener.WaitToResponse();

                //
                // Write by Data Point - bad "value" type
                //               
                var wrongPoint = PointData.Measurement("influxPoint")
                    .Tag("writeType", "dataPoint")
                    .Field("value", "22.22")
                    .Timestamp(DateTime.UtcNow.AddSeconds(-10), WritePrecision.Ns);

                writeApi.WritePoint(wrongPoint, "my-bucket", "my-org");
                listener.WaitToResponse();

                //
                // Write by POCO
                //
                var influxPoint = new InfluxPoint { WriteType = "POCO", Value = 33.33, Time = DateTime.UtcNow };

                writeApi.WriteMeasurement(influxPoint, WritePrecision.Ns, "my-bucket", "my-org");
                listener.WaitToResponse();

                //
                // Write by POCO - bucket not found
                //
                writeApi.WriteMeasurement(influxPoint, WritePrecision.Ns, "my-bucket2", "my-org");
                listener.WaitToResponse();
            }

            return Task.CompletedTask;
        }

        internal class EventListener
        {
            private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

            private readonly List<EventArgs> _events = new List<EventArgs>();

            internal EventListener(WriteApi writeApi)
            {
                writeApi.EventHandler += (sender, args) =>
                {
                    _events.Add(args);
                    _autoResetEvent.Set();

                    //
                    // Set output to console for different events types
                    //
                    switch (args)
                    {
                        // success response from server
                        case WriteSuccessEvent successEvent:
                            var dataList = successEvent.LineProtocol.Split(Environment.NewLine);

                            Console.WriteLine("WriteSuccessEvent: point was successfully written to InfluxDB");
                            foreach (var data in dataList)
                                Console.WriteLine($"  - {data}");
                            Console.WriteLine(
                                "-----------------------------------------------------------------------");
                            break;

                        // unhandled exception from server
                        case WriteErrorEvent errorEvent:
                            Console.WriteLine($"WriteErrorEvent: {errorEvent.Exception.Message}");
                            Console.WriteLine($"  - {errorEvent.LineProtocol}");
                            Console.WriteLine(
                                "-----------------------------------------------------------------------");
                            break;

                        // retrievable error from server
                        case WriteRetriableErrorEvent error:
                            Console.WriteLine($"WriteErrorEvent: {error.Exception.Message}");
                            Console.WriteLine(
                                "-----------------------------------------------------------------------");
                            break;

                        // runtime exception in background batch processing
                        case WriteRuntimeExceptionEvent error:
                            Console.WriteLine($"WriteRuntimeExceptionEvent: {error.Exception.Message}");
                            Console.WriteLine(
                                "-----------------------------------------------------------------------");
                            break;
                    }
                };
            }

            private T Get<T>() where T : EventArgs
            {
                if (EventCount() == 0)
                {
                    _autoResetEvent.Reset();
                    var timeout = TimeSpan.FromSeconds(15);
                    _autoResetEvent.WaitOne(timeout);
                }

                var args = _events[0];
                _events.RemoveAt(0);
                Trace.WriteLine(args);

                return args as T ?? throw new InvalidCastException(
                    $"{args.GetType().FullName} cannot be cast to {typeof(T).FullName}");
            }

            private int EventCount()
            {
                return _events.Count;
            }

            internal EventListener WaitToSuccess()
            {
                Get<WriteSuccessEvent>();
                return this;
            }

            internal EventListener WaitToResponse()
            {
                Get<EventArgs>();
                return this;
            }
        }
    }
}