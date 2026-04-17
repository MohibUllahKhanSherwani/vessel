namespace Vessel.Application.DTOs.Analytics;

public class PriceTrendDto
{
    public DateTimeOffset Date { get; set; }
    public decimal AveragePricePerGallon { get; set; }
}
