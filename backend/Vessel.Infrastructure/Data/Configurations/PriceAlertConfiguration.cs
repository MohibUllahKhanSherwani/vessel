using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vessel.Core.Entities;

namespace Vessel.Infrastructure.Data.Configurations;

public class PriceAlertConfiguration : IEntityTypeConfiguration<PriceAlert>
{
    public void Configure(EntityTypeBuilder<PriceAlert> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ThresholdTotalPrice).HasPrecision(18, 4);
        
        builder.HasIndex(e => new { e.ConsumerId, e.AreaId });

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(e => e.ConsumerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Area>()
               .WithMany()
               .HasForeignKey(e => e.AreaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
