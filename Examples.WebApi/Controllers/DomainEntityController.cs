using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Client;
using InfluxDB.Client.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Examples.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DomainEntityController : ControllerBase
    {
        private readonly ILogger<DomainEntityController> _logger;
        private readonly QueryApi _queryApi;
        private readonly DomainEntityConverter _converter;

        public DomainEntityController(ILogger<DomainEntityController> logger, InfluxDBClient client, DomainEntityConverter converter)
        {
            _logger = logger;
            _converter = converter;
            _queryApi = client.GetQueryApi(converter);
        }

        [HttpGet]
        public IEnumerable<DomainEntity> Get()
        {
            var query = from s in InfluxDBQueryable<DomainEntity>
                    .Queryable("my-bucket", "my-org", _queryApi, _converter)
                select s;
            
            return query.ToList();
        }

        [HttpGet]
        [Route("{id?}")]
        public IEnumerable<DomainEntity> Get(string id)
        {
            var query = from s in InfluxDBQueryable<DomainEntity>
                    .Queryable("my-bucket", "my-org", _queryApi, _converter)
                where s.SeriesId == Guid.Parse(id)
                select s;
            
            return query.ToList();
        }
    }
}