using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vessel.Core.Entities;
using Vessel.Core.Enums;

namespace Vessel.Infrastructure.Data;

/// <summary>
/// Seeds the in-memory database with realistic demo data on every startup.
/// No migrations are needed — EF In-Memory creates the schema automatically.
/// </summary>
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
            // EnsureCreated is a no-op for In-Memory but good practice to keep
            await _context.Database.EnsureCreatedAsync();

            _logger.LogInformation("Seeding in-memory database with demo data…");

            await SeedAreasAsync();
            await SeedUsersAsync();
            await SeedProvidersAndRatesAsync();
            await SeedBookingsAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Demo data seeding completed. API is ready.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    // ─── Areas ────────────────────────────────────────────────────────────────

    private async Task SeedAreasAsync()
    {
        if (await _context.Areas.AnyAsync()) return;

        var launchDate = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero);

        _context.Areas.AddRange(
            new Area { Id = Guid.Parse("ebf6fb5d-aa84-47ca-53f3-b723656d6758"), City = "Islamabad", Name = "Blue Area",        Latitude = 33.7133, Longitude = 73.0619, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("57e9f6aa-d280-db23-9ba7-c7e91dcd4fb3"), City = "Islamabad", Name = "F-6 Markaz",      Latitude = 33.7294, Longitude = 73.0932, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("8019e41f-6ad6-a993-79a4-d146e81d1d5d"), City = "Islamabad", Name = "F-10 Markaz",     Latitude = 33.6952, Longitude = 73.0129, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("7a63151e-a977-efa9-d57a-ab1612e6a93e"), City = "Lahore",    Name = "Gulberg III",     Latitude = 31.5060, Longitude = 74.3556, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("e5bb14b7-bfcd-46b1-903c-5a0d678cdd69"), City = "Lahore",    Name = "DHA Phase 6",    Latitude = 31.4945, Longitude = 74.3534, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("83dfdacb-eae8-6d60-eddd-0d8cae0b20bb"), City = "Lahore",    Name = "Model Town",     Latitude = 31.4844, Longitude = 74.3244, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("20efca73-66e5-6f6f-87cc-6245405bdd62"), City = "Karachi",   Name = "Clifton Block 4",Latitude = 24.8064, Longitude = 67.0301, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("93c8ebd6-4324-e55d-4c12-9c3fffc23554"), City = "Karachi",   Name = "DHA Phase 6",   Latitude = 24.7967, Longitude = 67.0495, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("3a3f69c7-13c9-9a81-52d6-9940e12e7752"), City = "Karachi",   Name = "Gulshan-e-Iqbal",Latitude = 24.9167, Longitude = 67.0833, CreatedAt = launchDate, UpdatedAt = launchDate }
        );

        await _context.SaveChangesAsync();
    }

    // ─── Users ────────────────────────────────────────────────────────────────

    private async Task SeedUsersAsync()
    {
        if (await _context.Users.AnyAsync()) return;

        var now = DateTimeOffset.UtcNow;

        var adminPassword    = _configuration["SeedConfigs:AdminPassword"]    ?? "Admin123!";
        var providerPassword = _configuration["SeedConfigs:ProviderPassword"] ?? "Provider123!";
        var consumerPassword = _configuration["SeedConfigs:ConsumerPassword"] ?? "Consumer123!";

        _context.Users.AddRange(
            new User
            {
                Id           = Guid.Parse("a1000000-0000-0000-0000-000000000001"),
                Email        = "admin@vessel.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                FullName     = "System Administrator",
                Role         = UserRole.Admin,
                IsActive     = true,
                CreatedAt    = now,
                UpdatedAt    = now
            },
            // Provider users
            new User
            {
                Id           = Guid.Parse("b2000000-0000-0000-0000-000000000001"),
                Email        = "provider@vessel.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(providerPassword),
                FullName     = "Puma Pride",
                Role         = UserRole.Provider,
                IsActive     = true,
                CreatedAt    = now,
                UpdatedAt    = now
            },
            new User
            {
                Id           = Guid.Parse("b2000000-0000-0000-0000-000000000002"),
                Email        = "skyoil@vessel.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(providerPassword),
                FullName     = "Sky Oil Ltd",
                Role         = UserRole.Provider,
                IsActive     = true,
                CreatedAt    = now,
                UpdatedAt    = now
            },
            new User
            {
                Id           = Guid.Parse("b2000000-0000-0000-0000-000000000003"),
                Email        = "rapidfuel@vessel.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(providerPassword),
                FullName     = "Rapid Fuel Co",
                Role         = UserRole.Provider,
                IsActive     = true,
                CreatedAt    = now,
                UpdatedAt    = now
            },
            // Consumer users
            new User
            {
                Id           = Guid.Parse("c3000000-0000-0000-0000-000000000001"),
                Email        = "consumer@vessel.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(consumerPassword),
                FullName     = "Ali Khan",
                Role         = UserRole.Consumer,
                IsActive     = true,
                CreatedAt    = now,
                UpdatedAt    = now
            },
            new User
            {
                Id           = Guid.Parse("c3000000-0000-0000-0000-000000000002"),
                Email        = "sara@vessel.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(consumerPassword),
                FullName     = "Sara Ahmed",
                Role         = UserRole.Consumer,
                IsActive     = true,
                CreatedAt    = now,
                UpdatedAt    = now
            }
        );

        await _context.SaveChangesAsync();
    }

    // ─── Providers & Rates ────────────────────────────────────────────────────

    private async Task SeedProvidersAndRatesAsync()
    {
        if (await _context.Providers.AnyAsync()) return;

        var now = DateTimeOffset.UtcNow;

        // Provider entities
        var pumaPrideId   = Guid.Parse("d4000000-0000-0000-0000-000000000001");
        var skyOilId      = Guid.Parse("d4000000-0000-0000-0000-000000000002");
        var rapidFuelId   = Guid.Parse("d4000000-0000-0000-0000-000000000003");

        _context.Providers.AddRange(
            new Provider { Id = pumaPrideId, UserId = Guid.Parse("b2000000-0000-0000-0000-000000000001"), CompanyName = "Puma Pride Fueling Services", ContactNumber = "021-34567890", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Provider { Id = skyOilId,    UserId = Guid.Parse("b2000000-0000-0000-0000-000000000002"), CompanyName = "Sky Oil Ltd",                 ContactNumber = "051-11223344", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Provider { Id = rapidFuelId, UserId = Guid.Parse("b2000000-0000-0000-0000-000000000003"), CompanyName = "Rapid Fuel Co",              ContactNumber = "042-99887766", IsActive = true, CreatedAt = now, UpdatedAt = now }
        );

        await _context.SaveChangesAsync();

        // Active rates (EffectiveTo = null means current)
        var islamabadBlue = Guid.Parse("ebf6fb5d-aa84-47ca-53f3-b723656d6758");
        var islamabadF6   = Guid.Parse("57e9f6aa-d280-db23-9ba7-c7e91dcd4fb3");
        var lahoreDHA     = Guid.Parse("e5bb14b7-bfcd-46b1-903c-5a0d678cdd69");
        var karachiClifton = Guid.Parse("20efca73-66e5-6f6f-87cc-6245405bdd62");

        _context.ProviderRates.AddRange(
            new ProviderRate { Id = Guid.NewGuid(), ProviderId = pumaPrideId,  AreaId = islamabadBlue,   PricePerGallon = 320.50m, EffectiveFrom = now.AddDays(-10), EffectiveTo = null, CreatedAt = now, UpdatedAt = now },
            new ProviderRate { Id = Guid.NewGuid(), ProviderId = skyOilId,     AreaId = islamabadBlue,   PricePerGallon = 318.00m, EffectiveFrom = now.AddDays(-5),  EffectiveTo = null, CreatedAt = now, UpdatedAt = now },
            new ProviderRate { Id = Guid.NewGuid(), ProviderId = rapidFuelId,  AreaId = islamabadBlue,   PricePerGallon = 322.75m, EffectiveFrom = now.AddDays(-3),  EffectiveTo = null, CreatedAt = now, UpdatedAt = now },
            new ProviderRate { Id = Guid.NewGuid(), ProviderId = pumaPrideId,  AreaId = islamabadF6,     PricePerGallon = 315.00m, EffectiveFrom = now.AddDays(-7),  EffectiveTo = null, CreatedAt = now, UpdatedAt = now },
            new ProviderRate { Id = Guid.NewGuid(), ProviderId = skyOilId,     AreaId = lahoreDHA,       PricePerGallon = 310.25m, EffectiveFrom = now.AddDays(-8),  EffectiveTo = null, CreatedAt = now, UpdatedAt = now },
            new ProviderRate { Id = Guid.NewGuid(), ProviderId = rapidFuelId,  AreaId = karachiClifton,  PricePerGallon = 325.00m, EffectiveFrom = now.AddDays(-2),  EffectiveTo = null, CreatedAt = now, UpdatedAt = now }
        );

        await _context.SaveChangesAsync();
    }

    // ─── Sample Bookings ──────────────────────────────────────────────────────

    private async Task SeedBookingsAsync()
    {
        if (await _context.Bookings.AnyAsync()) return;

        var now        = DateTimeOffset.UtcNow;
        var consumerId = Guid.Parse("c3000000-0000-0000-0000-000000000001");
        var sara       = Guid.Parse("c3000000-0000-0000-0000-000000000002");
        var pumaPrideId = Guid.Parse("d4000000-0000-0000-0000-000000000001");
        var skyOilId    = Guid.Parse("d4000000-0000-0000-0000-000000000002");
        var islamabadBlue  = Guid.Parse("ebf6fb5d-aa84-47ca-53f3-b723656d6758");
        var lahoreDHA      = Guid.Parse("e5bb14b7-bfcd-46b1-903c-5a0d678cdd69");

        _context.Bookings.AddRange(
            new Booking
            {
                Id = Guid.NewGuid(), ConsumerId = consumerId, ProviderId = pumaPrideId,
                AreaId = islamabadBlue, VolumeInGallons = 50, PricePerGallonSnapshot = 320.50m,
                TotalPrice = 16025.00m, DeliveryAddress = "Plot 12, Blue Area, Islamabad",
                Status = BookingStatus.Confirmed, ScheduledFor = now.AddDays(-5),
                IdempotencyKey = "demo-booking-1", CreatedAt = now.AddDays(-6), UpdatedAt = now.AddDays(-5)
            },
            new Booking
            {
                Id = Guid.NewGuid(), ConsumerId = sara, ProviderId = skyOilId,
                AreaId = lahoreDHA, VolumeInGallons = 30, PricePerGallonSnapshot = 310.25m,
                TotalPrice = 9307.50m, DeliveryAddress = "Street 3, DHA Phase 6, Lahore",
                Status = BookingStatus.Confirmed, ScheduledFor = now.AddDays(-3),
                IdempotencyKey = "demo-booking-2", CreatedAt = now.AddDays(-4), UpdatedAt = now.AddDays(-3)
            },
            new Booking
            {
                Id = Guid.NewGuid(), ConsumerId = consumerId, ProviderId = pumaPrideId,
                AreaId = islamabadBlue, VolumeInGallons = 100, PricePerGallonSnapshot = 318.00m,
                TotalPrice = 31800.00m, DeliveryAddress = "Plot 12, Blue Area, Islamabad",
                Status = BookingStatus.Pending, ScheduledFor = now.AddDays(2),
                IdempotencyKey = "demo-booking-3", CreatedAt = now.AddDays(-1), UpdatedAt = now.AddDays(-1)
            }
        );

        await _context.SaveChangesAsync();
    }
}
