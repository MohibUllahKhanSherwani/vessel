using Vessel.Core.Enums;

namespace Vessel.Application.DTOs.Alerts;

public class PriceAlertDto
{
    public Guid Id { get; set; }
    public Guid AreaId { get; set; }
    public decimal ThresholdTotalPrice { get; set; }
    public int TargetVolumeInGallons { get; set; }
    public AlertDirection Direction { get; set; }
    public bool IsActive { get; set; }
    public Guid? LastTriggeredRateId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
