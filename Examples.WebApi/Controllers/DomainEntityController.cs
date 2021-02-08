using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Client;
using InfluxDB.Client.Linq;
using Microsoft.AspNet.OData.Query;
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

        public DomainEntityController(ILogger<DomainEntityController> logger, InfluxDBClient client,
            DomainEntityConverter converter)
        {
            _logger = logger;
            _converter = converter;
            _queryApi = client.GetQueryApi(converter);
        }

        [HttpGet]
        public IQueryable<DomainEntity> Get()
        {
            var query = from s in InfluxDBQueryable<DomainEntity>
                    .Queryable("my-bucket", "my-org", _queryApi, _converter)
                select s;

            return query;
        }

        /// <summary>
        /// All: http://localhost:5000/domainEntity/odata
        /// Time range: http://localhost:5000/domainEntity/odata?$filter=Timestamp+gt+2020-01-01T00:00:00.000Z+and+Timestamp+le+2020-12-31T23:59:59.999Z
        /// Any + Convert: http://localhost:5000/domainEntity/odata?$filter=Timestamp+gt+2020-01-01T00:00:00.000Z+and+Timestamp+le+2020-12-31T23:59:59.999Z+and+Attributes/any(a:a/Name+eq+%27Quality%27+and+a/Value+eq+%27Good%27)
        /// </summary>
        /// 
        [HttpGet]
        [Route("odata")]
        public IQueryable<DomainEntity> OData(ODataQueryOptions<DomainEntity> options)
        {
            var query = from s in InfluxDBQueryable<DomainEntity>
                    .Queryable("my-bucket", "my-org", _queryApi, _converter)
                where s.SeriesId == Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e")
                select s;
            
            var filtered = options.ApplyTo(query) as IQueryable<DomainEntity>;
            
            _logger.LogInformation($"The query with applied OData filter: {filtered?.Expression}");
            
            return filtered;
        }

        [HttpGet]
        [Route("one/{id?}")]
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