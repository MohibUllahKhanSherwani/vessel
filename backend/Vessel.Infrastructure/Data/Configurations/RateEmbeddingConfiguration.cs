using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vessel.Core.Entities;

namespace Vessel.Infrastructure.Data.Configurations;

public class RateEmbeddingConfiguration : IEntityTypeConfiguration<RateEmbedding>
{
    public void Configure(EntityTypeBuilder<RateEmbedding> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.SourceType)
               .IsRequired();
               
        builder.Property(e => e.SourceId)
               .IsRequired();
               
        builder.Property(e => e.ContentText)
               .IsRequired();
               
        builder.Property(e => e.Embedding);
               
        builder.HasIndex(e => new { e.SourceType, e.SourceId });
    }
}
