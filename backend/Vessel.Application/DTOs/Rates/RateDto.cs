namespace Vessel.Application.DTOs.Rates;

public class RateDto
{
    public Guid ProviderId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public Guid AreaId { get; set; }
    public string AreaName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal PricePerGallon { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
}
