using Vessel.Core.Enums;

namespace Vessel.Application.DTOs.Alerts;

public class UpdatePriceAlertDto
{
    public decimal ThresholdTotalPrice { get; set; }
    public int TargetVolumeInGallons { get; set; }
    public AlertDirection Direction { get; set; }
    public bool IsActive { get; set; }
}
