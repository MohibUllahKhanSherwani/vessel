namespace Vessel.Application.DTOs.Analytics;

public class TopProviderDto
{
    public Guid ProviderId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int ConfirmedBookingCount { get; set; }
    public decimal TotalGallons { get; set; }
}
