namespace Vessel.Application.DTOs.Providers;

public class ProviderSearchResultDto
{
    public Guid ProviderId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public Guid AreaId { get; set; }
    public string AreaName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public decimal CurrentPricePerGallon { get; set; }
}
