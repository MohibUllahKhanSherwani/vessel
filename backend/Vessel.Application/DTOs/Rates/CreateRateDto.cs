namespace Vessel.Application.DTOs.Rates;

public class CreateRateDto
{
    public Guid AreaId { get; set; }
    public decimal PricePerGallon { get; set; }
}
