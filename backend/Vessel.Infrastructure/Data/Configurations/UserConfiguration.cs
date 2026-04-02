using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vessel.Core.Entities;

namespace Vessel.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.PasswordHash).IsRequired();
        builder.Property(e => e.FullName).IsRequired().HasMaxLength(150);

        builder.HasIndex(e => e.Email).IsUnique();

        builder.HasMany<RefreshToken>()
               .WithOne()
               .HasForeignKey(rt => rt.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
