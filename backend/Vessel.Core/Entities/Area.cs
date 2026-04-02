using Vessel.Core.Common.Interfaces;

namespace Vessel.Core.Entities;

public class Area : IAuditableEntity
{
    public Guid Id { get; set; }
    public string City { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
