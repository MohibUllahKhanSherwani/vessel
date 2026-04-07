namespace Vessel.Application.DTOs.Rates;

public class RateHistoryDto
{
    public decimal PricePerGallon { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
