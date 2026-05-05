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
               
        // The vector dimension for Gemini text-embedding-004 is 768.
        // We only apply the PostgreSQL vector type and converter if we are using Npgsql.
        if (builder.Metadata.Model.GetProductVersion()?.Contains("Npgsql") == true || 
            builder.Metadata.Model.FindAnnotation("Relational:DatabaseProvider")?.Value?.ToString() == "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            builder.Property(e => e.Embedding)
                   .HasColumnType("vector(768)")
                   .HasConversion(
                       v => new Pgvector.Vector(v),
                       v => v.ToArray()
                   );
        }
        else
        {
            // For In-Memory or other providers, store as a simple property (EF Core will handle float[] as a collection)
            // Or we can explicitly ignore it if tests don't use it, but keeping it as a property is safer.
            builder.Property(e => e.Embedding);
        }
               
        builder.HasIndex(e => new { e.SourceType, e.SourceId });
    }
}
