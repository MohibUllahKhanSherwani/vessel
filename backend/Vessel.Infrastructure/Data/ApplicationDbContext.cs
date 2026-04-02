using Microsoft.EntityFrameworkCore;
using Vessel.Core.Common.Interfaces;
using Vessel.Core.Entities;

namespace Vessel.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<ProviderRate> ProviderRates => Set<ProviderRate>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<PriceAlert> PriceAlerts => Set<PriceAlert>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IAuditableEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var auditable = (IAuditableEntity)entityEntry.Entity;
            auditable.UpdatedAt = DateTimeOffset.UtcNow;

            if (entityEntry.State == EntityState.Added)
            {
                auditable.CreatedAt = DateTimeOffset.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
