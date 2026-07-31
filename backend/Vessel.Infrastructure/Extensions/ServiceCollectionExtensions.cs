using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vessel.Infrastructure.Data;
using Vessel.Infrastructure.Services.Caching;

namespace Vessel.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Use EF Core In-Memory database — no Postgres, no Docker required
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("VesselLocalDemo"));

        // In-memory cache — no Redis required
        services.AddSingleton<Vessel.Application.Interfaces.Caching.ICacheService, MemoryCacheService>();

        services.AddScoped<DbInitializer>();

        // Repositories
        services.AddScoped<Vessel.Application.Interfaces.Repositories.IAreaRepository, Vessel.Infrastructure.Repositories.AreaRepository>();
        services.AddScoped<Vessel.Application.Interfaces.Repositories.IBookingRepository, Vessel.Infrastructure.Repositories.BookingRepository>();
        services.AddScoped<Vessel.Application.Interfaces.Repositories.IPriceAlertRepository, Vessel.Infrastructure.Repositories.PriceAlertRepository>();
        services.AddScoped<Vessel.Application.Interfaces.Repositories.IProviderRateRepository, Vessel.Infrastructure.Repositories.ProviderRateRepository>();
        services.AddScoped<Vessel.Application.Interfaces.Repositories.IProviderRepository, Vessel.Infrastructure.Repositories.ProviderRepository>();
        services.AddScoped<Vessel.Application.Interfaces.Repositories.IRefreshTokenRepository, Vessel.Infrastructure.Repositories.RefreshTokenRepository>();
        services.AddScoped<Vessel.Application.Interfaces.Repositories.IRateEmbeddingRepository, Vessel.Infrastructure.Repositories.RateEmbeddingRepository>();
        services.AddScoped<Vessel.Application.Interfaces.Repositories.IUserRepository, Vessel.Infrastructure.Repositories.UserRepository>();

        return services;
    }
}
