using InfluxDB.Client;
using InfluxDB.Client.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection
{

    public static class HealthChecksBuilderExtensions
	{

		public static IHealthChecksBuilder AddInfluxDBCheck(this IHealthChecksBuilder healthChecksBuilder)
		{
			return healthChecksBuilder.AddCheck<InfluxDBClientHealthCheck>(nameof(InfluxDBClient));
		}

	}

}
