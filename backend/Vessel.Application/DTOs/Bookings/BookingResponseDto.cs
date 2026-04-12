using Vessel.Core.Enums;

namespace Vessel.Application.DTOs.Bookings;

public class BookingResponseDto
{
    public Guid Id { get; set; }
    public Guid ConsumerId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid AreaId { get; set; }
    public decimal VolumeInGallons { get; set; }
    public decimal PricePerGallonSnapshot { get; set; }
    public decimal TotalPrice { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime ScheduledFor { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
