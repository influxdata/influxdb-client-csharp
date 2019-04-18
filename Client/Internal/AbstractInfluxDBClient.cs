using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Internal;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Api.Domain;
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

    }
}