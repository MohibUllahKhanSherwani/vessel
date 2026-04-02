using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vessel.Core.Entities;

namespace Vessel.Infrastructure.Data.Configurations;

public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.City).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);

        builder.HasIndex(e => new { e.City, e.Name }).IsUnique();

        var launchDate = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero);

        builder.HasData(
            new Area { Id = Guid.Parse("ebf6fb5d-aa84-47ca-53f3-b723656d6758"), City = "Islamabad", Name = "Blue Area", Latitude = 33.7133, Longitude = 73.0619, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("57e9f6aa-d280-db23-9ba7-c7e91dcd4fb3"), City = "Islamabad", Name = "F-6 Markaz", Latitude = 33.7294, Longitude = 73.0932, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("8019e41f-6ad6-a993-79a4-d146e81d1d5d"), City = "Islamabad", Name = "F-10 Markaz", Latitude = 33.6952, Longitude = 73.0129, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("7a63151e-a977-efa9-d57a-ab1612e6a93e"), City = "Lahore", Name = "Gulberg III", Latitude = 31.5060, Longitude = 74.3556, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("e5bb14b7-bfcd-46b1-903c-5a0d678cdd69"), City = "Lahore", Name = "DHA Phase 6", Latitude = 31.4945, Longitude = 74.3534, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("83dfdacb-eae8-6d60-eddd-0d8cae0b20bb"), City = "Lahore", Name = "Model Town", Latitude = 31.4844, Longitude = 74.3244, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("20efca73-66e5-6f6f-87cc-6245405bdd62"), City = "Karachi", Name = "Clifton Block 4", Latitude = 24.8064, Longitude = 67.0301, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("93c8ebd6-4324-e55d-4c12-9c3fffc23554"), City = "Karachi", Name = "DHA Phase 6", Latitude = 24.7967, Longitude = 67.0495, CreatedAt = launchDate, UpdatedAt = launchDate },
            new Area { Id = Guid.Parse("3a3f69c7-13c9-9a81-52d6-9940e12e7752"), City = "Karachi", Name = "Gulshan-e-Iqbal", Latitude = 24.9167, Longitude = 67.0833, CreatedAt = launchDate, UpdatedAt = launchDate }
        );
    }
}
