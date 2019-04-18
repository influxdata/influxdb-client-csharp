using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Internal;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Generated.Domain;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Internal
{
    public class AbstractInfluxDBClient : AbstractClient
    {
        protected AbstractInfluxDBClient()
        {
        }

        protected AbstractInfluxDBClient(DefaultClientIo client) : base(client)
        {
        }

        protected Check GetHealth(Task<Check> task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            try
            {
                return task.Result;
            }
            catch (Exception e)
            {
                return new Check("influxdb", e.GetBaseException().Message, default(List<Check>), Check.StatusEnum.Fail);
            }
        }
    }
}