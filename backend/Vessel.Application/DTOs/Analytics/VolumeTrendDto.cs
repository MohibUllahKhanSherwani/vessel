namespace Vessel.Application.DTOs.Analytics;

public class VolumeTrendDto
{
    public DateTimeOffset Date { get; set; }
    public int ConfirmedBookingCount { get; set; }
    public decimal TotalGallons { get; set; }
}
