namespace Vessel.Application.DTOs.Rates;

public class RateHistoryDto
{
    public decimal PricePerGallon { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
}
