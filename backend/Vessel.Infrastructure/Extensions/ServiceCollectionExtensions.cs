using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vessel.Infrastructure.Data;

namespace Vessel.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<DbInitializer>();

        // Repositories
        services.AddScoped<Vessel.Application.Interfaces.Repositories.IAreaRepository, Vessel.Infrastructure.Repositories.AreaRepository>();
        services.AddScoped<Vessel.Application.Interfaces.Repositories.IBookingRepository, Vessel.Infrastructure.Repositories.BookingRepository>();
        services.AddScoped<Vessel.Application.Interfaces.Repositories.IPriceAlertRepository, Vessel.Infrastructure.Repositories.PriceAlertRepository>();
        services.AddScoped<Vessel.Application.Interfaces.Repositories.IProviderRateRepository, Vessel.Infrastructure.Repositories.ProviderRateRepository>();
        services.AddScoped<Vessel.Application.Interfaces.Repositories.IProviderRepository, Vessel.Infrastructure.Repositories.ProviderRepository>();
        services.AddScoped<Vessel.Application.Interfaces.Repositories.IRefreshTokenRepository, Vessel.Infrastructure.Repositories.RefreshTokenRepository>();
        services.AddScoped<Vessel.Application.Interfaces.Repositories.IUserRepository, Vessel.Infrastructure.Repositories.UserRepository>();

        return services;
    }
}
