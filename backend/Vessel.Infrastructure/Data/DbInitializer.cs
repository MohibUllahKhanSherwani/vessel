using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vessel.Core.Entities;
using Vessel.Core.Enums;

namespace Vessel.Infrastructure.Data;

public class DbInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(ApplicationDbContext context, IConfiguration configuration, ILogger<DbInitializer> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();

            _logger.LogInformation("Starting database seeding...");

            await SeedUsersAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database seeding completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        // 1. Seed Admin
        var adminEmail = "admin@vessel.com";
        if (!await _context.Users.AnyAsync(u => u.Email == adminEmail))
        {
            var adminPassword = _configuration["SeedConfigs:AdminPassword"] ?? "Admin123!";
            var admin = new User
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000001"),
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                FullName = "System Administrator",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _context.Users.Add(admin);
            _logger.LogInformation("Seeding Admin account: {Email}", adminEmail);
        }

        // 2. Seed Provider
        var providerEmail = "provider@vessel.com";
        if (!await _context.Users.AnyAsync(u => u.Email == providerEmail))
        {
            var providerPassword = _configuration["SeedConfigs:ProviderPassword"] ?? "Provider123!";
            var providerUserId = Guid.Parse("b2000000-0000-0000-0000-000000000001");
            
            var providerUser = new User
            {
                Id = providerUserId,
                Email = providerEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(providerPassword),
                FullName = "Puma Pride",
                Role = UserRole.Provider,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            var providerRecord = new Provider
            {
                Id = Guid.Parse("c3000000-0000-0000-0000-000000000001"),
                UserId = providerUserId,
                CompanyName = "Puma Pride Fueling Services",
                ContactNumber = "021-34567890",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Users.Add(providerUser);
            _context.Providers.Add(providerRecord);
            _logger.LogInformation("Seeding Provider account: {Email}", providerEmail);
        }
    }
}
