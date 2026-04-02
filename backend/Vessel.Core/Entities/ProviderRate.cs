using Vessel.Core.Common.Interfaces;

namespace Vessel.Core.Entities;

public class ProviderRate : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public Guid AreaId { get; set; }
    public decimal PricePerGallon { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
