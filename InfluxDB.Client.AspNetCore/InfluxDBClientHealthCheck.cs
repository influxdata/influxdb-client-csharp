using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace InfluxDB.Client.AspNetCore
{

    public class InfluxDBClientHealthCheck : IHealthCheck
	{

		private readonly InfluxDBClient influxDBClient;

		public InfluxDBClientHealthCheck(InfluxDBClient influxDBClient)
		{
			this.influxDBClient = influxDBClient;
		}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext healthCheckContext, CancellationToken cancellationToken = default)
		{
			var healthCheck = await this.influxDBClient.HealthAsync();
			return healthCheck.Status == HealthCheck.StatusEnum.Pass
				? HealthCheckResult.Healthy()
				: HealthCheckResult.Unhealthy();
		}

	}

}
