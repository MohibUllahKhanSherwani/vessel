namespace Vessel.Application.DTOs.Analytics;

public class AveragePriceDto
{
    public string City { get; set; } = string.Empty;
    public decimal AveragePricePerGallon { get; set; }
    public int ActiveProviderCount { get; set; }
}
