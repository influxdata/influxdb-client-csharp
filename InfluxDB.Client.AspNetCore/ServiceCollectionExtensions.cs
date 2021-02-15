using System.ComponentModel;
using InfluxDB.Client;

namespace Microsoft.Extensions.DependencyInjection
{

    public static class ServiceCollectionExtensions
	{

		public static IServiceCollection AddInfluxDBClient(this IServiceCollection serviceCollection, string connectionString, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
		{
			switch (serviceLifetime)
            {
				case ServiceLifetime.Singleton:
					return serviceCollection.AddSingleton((serviceProvider) => InfluxDBClientFactory.Create(connectionString));
				case ServiceLifetime.Scoped:
					return serviceCollection.AddScoped((serviceProvider) => InfluxDBClientFactory.Create(connectionString));
				case ServiceLifetime.Transient:
					return serviceCollection.AddTransient((serviceProvider) => InfluxDBClientFactory.Create(connectionString));
				default:
					throw new InvalidEnumArgumentException(nameof(serviceLifetime), (int) serviceLifetime, typeof(ServiceLifetime));
			}
		}

	}

}
