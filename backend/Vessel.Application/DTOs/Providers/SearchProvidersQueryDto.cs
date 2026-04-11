namespace Vessel.Application.DTOs.Providers;

public class SearchProvidersQueryDto
{
    public double Lat { get; set; }
    public double Lon { get; set; }
    public double RadiusKm { get; set; }
}
