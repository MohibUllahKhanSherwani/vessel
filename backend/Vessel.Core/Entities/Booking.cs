using Vessel.Core.Common.Interfaces;
using Vessel.Core.Enums;

namespace Vessel.Core.Entities;

public class Booking : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid ConsumerId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid AreaId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public int VolumeInGallons { get; set; }
    public decimal PricePerGallonSnapshot { get; set; }
    public decimal TotalPrice { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public BookingStatus Status { get; set; }
    public DateTimeOffset ScheduledFor { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
