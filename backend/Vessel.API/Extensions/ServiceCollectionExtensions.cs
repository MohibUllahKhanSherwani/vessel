using Vessel.Infrastructure.Extensions;

namespace Vessel.API.Extensions;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/> to register application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application-specific services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection instance.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The modified service collection instance.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureServices(configuration);
        return services;
    }
}