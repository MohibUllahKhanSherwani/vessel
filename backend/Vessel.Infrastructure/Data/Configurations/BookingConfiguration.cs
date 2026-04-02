using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vessel.Core.Entities;

namespace Vessel.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.IdempotencyKey).IsRequired().HasMaxLength(100);
        builder.Property(e => e.PricePerGallonSnapshot).HasPrecision(18, 4);
        builder.Property(e => e.TotalPrice).HasPrecision(18, 2);
        
        builder.HasIndex(e => new { e.ConsumerId, e.IdempotencyKey }).IsUnique();

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(e => e.ConsumerId)
               .OnDelete(DeleteBehavior.Restrict);

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
