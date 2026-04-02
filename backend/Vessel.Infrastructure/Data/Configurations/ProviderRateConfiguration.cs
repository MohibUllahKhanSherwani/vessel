using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vessel.Core.Entities;

namespace Vessel.Infrastructure.Data.Configurations;

public class ProviderRateConfiguration : IEntityTypeConfiguration<ProviderRate>
{
    public void Configure(EntityTypeBuilder<ProviderRate> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.PricePerGallon).HasPrecision(18, 4);
        
        builder.HasIndex(e => new { e.ProviderId, e.AreaId })
               .IsUnique()
               .HasFilter("\"EffectiveTo\" IS NULL");

        builder.HasOne<Provider>()
               .WithMany()
               .HasForeignKey(e => e.ProviderId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Area>()
               .WithMany()
               .HasForeignKey(e => e.AreaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
