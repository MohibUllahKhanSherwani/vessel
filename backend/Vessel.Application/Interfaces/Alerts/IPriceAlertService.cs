using Vessel.Application.DTOs.Alerts;

namespace Vessel.Application.Interfaces.Alerts;

public interface IPriceAlertService
{
    Task<PriceAlertDto> CreateAlertAsync(Guid consumerId, CreatePriceAlertDto request);
    Task<IEnumerable<PriceAlertDto>> GetConsumerAlertsAsync(Guid consumerId);
    Task UpdateAlertAsync(Guid consumerId, Guid alertId, UpdatePriceAlertDto request);
    Task DeleteAlertAsync(Guid consumerId, Guid alertId);
    
    // This will be used by the background job
    Task ProcessAlertsForAreaAsync(Guid areaId, Guid rateId, decimal pricePerGallon);
}
