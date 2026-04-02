using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vessel.Core.Entities;

namespace Vessel.Infrastructure.Data.Configurations;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.ContactNumber).IsRequired().HasMaxLength(50);
        
        builder.HasIndex(e => e.UserId).IsUnique();
    }
}
