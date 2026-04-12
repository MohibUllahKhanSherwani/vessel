using Vessel.Core.Enums;

namespace Vessel.Application.DTOs.Alerts;

public class CreatePriceAlertDto
{
    public Guid AreaId { get; set; }
    public decimal ThresholdTotalPrice { get; set; }
    public int TargetVolumeInGallons { get; set; }
    public AlertDirection Direction { get; set; }
}
