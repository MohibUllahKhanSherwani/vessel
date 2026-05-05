using Vessel.Core.Common.Interfaces;
using Vessel.Core.Enums;

namespace Vessel.Core.Entities;

public class RateEmbedding : IAuditableEntity
{
    public Guid Id { get; set; }
    public SourceType SourceType { get; set; }
    public Guid SourceId { get; set; }
    public string ContentText { get; set; } = string.Empty;
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
