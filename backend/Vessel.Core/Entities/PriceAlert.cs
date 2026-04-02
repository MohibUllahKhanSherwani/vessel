using Vessel.Core.Common.Interfaces;
using Vessel.Core.Enums;

namespace Vessel.Core.Entities;

public class PriceAlert : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid ConsumerId { get; set; }
    public Guid AreaId { get; set; }
    public decimal ThresholdTotalPrice { get; set; }
    public int TargetVolumeInGallons { get; set; }
    public AlertDirection Direction { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? LastTriggeredRateId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
