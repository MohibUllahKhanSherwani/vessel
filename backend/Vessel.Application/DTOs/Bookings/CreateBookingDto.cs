using System.ComponentModel.DataAnnotations;

namespace Vessel.Application.DTOs.Bookings;

public class CreateBookingDto
{
    [Required]
    public Guid ProviderId { get; set; }

    [Required]
    public Guid AreaId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Volume must be greater than zero.")]
    public decimal VolumeInGallons { get; set; }

    [Required]
    public DateTime ScheduledFor { get; set; }

    [Required]
    [StringLength(500)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }
}
