using Vessel.Application.DTOs.Alerts;
using Vessel.Application.Interfaces.Alerts;
using Vessel.Application.Interfaces.Repositories;
using Vessel.Core.Entities;
using Vessel.Core.Enums;

namespace Vessel.Application.Services;

public class PriceAlertService : IPriceAlertService
{
    private readonly IPriceAlertRepository _alertRepository;
    private readonly IAreaRepository _areaRepository;

    public PriceAlertService(IPriceAlertRepository alertRepository, IAreaRepository areaRepository)
    {
        _alertRepository = alertRepository;
        _areaRepository = areaRepository;
    }

    public async Task<PriceAlertDto> CreateAlertAsync(Guid consumerId, CreatePriceAlertDto request)
    {
        var area = await _areaRepository.GetByIdAsync(request.AreaId);
        if (area == null) throw new ArgumentException("Invalid Area Id.");

        var alert = new PriceAlert
        {
            Id = Guid.NewGuid(),
            ConsumerId = consumerId,
            AreaId = request.AreaId,
            ThresholdTotalPrice = request.ThresholdTotalPrice,
            TargetVolumeInGallons = request.TargetVolumeInGallons,
            Direction = request.Direction,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _alertRepository.AddAsync(alert);

        return MapToDto(alert);
    }

    public async Task<IEnumerable<PriceAlertDto>> GetConsumerAlertsAsync(Guid consumerId)
    {
        var alerts = await _alertRepository.GetByConsumerIdAsync(consumerId);
        return alerts.Select(MapToDto);
    }

    public async Task UpdateAlertAsync(Guid consumerId, Guid alertId, UpdatePriceAlertDto request)
    {
        var alert = await _alertRepository.GetByIdAsync(alertId);
        if (alert == null || alert.ConsumerId != consumerId)
            throw new KeyNotFoundException("Alert not found.");

        alert.ThresholdTotalPrice = request.ThresholdTotalPrice;
        alert.TargetVolumeInGallons = request.TargetVolumeInGallons;
        alert.Direction = request.Direction;
        alert.IsActive = request.IsActive;
        alert.UpdatedAt = DateTimeOffset.UtcNow;

        await _alertRepository.UpdateAsync(alert);
    }

    public async Task DeleteAlertAsync(Guid consumerId, Guid alertId)
    {
        var alert = await _alertRepository.GetByIdAsync(alertId);
        if (alert == null || alert.ConsumerId != consumerId)
            throw new KeyNotFoundException("Alert not found.");

        await _alertRepository.DeleteAsync(alertId);
    }

    public async Task ProcessAlertsForAreaAsync(Guid areaId, Guid rateId, decimal pricePerGallon)
    {
        var activeAlerts = await _alertRepository.GetActiveAlertsByAreaAsync(areaId);

        foreach (var alert in activeAlerts)
        {
            // Total price for the user's target volume at this new rate
            decimal currentTotalPrice = pricePerGallon * alert.TargetVolumeInGallons;
            
            bool triggered = alert.Direction switch
            {
                AlertDirection.BelowOrEqual => currentTotalPrice <= alert.ThresholdTotalPrice,
                AlertDirection.AboveOrEqual => currentTotalPrice >= alert.ThresholdTotalPrice,
                _ => false
            };

            // Only trigger if it hasn't been triggered for this specific rate already
            if (triggered && alert.LastTriggeredRateId != rateId)
            {
                // For now, we just update the last triggered rate. 
                // Actual notification (Email/Push) will be added in subsequent tasks.
                alert.LastTriggeredRateId = rateId;
                alert.UpdatedAt = DateTimeOffset.UtcNow;
                await _alertRepository.UpdateAsync(alert);
                
                Console.WriteLine($"ALERT TRIGGERED: Consumer {alert.ConsumerId} - Price of {currentTotalPrice:C} is {alert.Direction} {alert.ThresholdTotalPrice:C}");
            }
        }
    }

    private static PriceAlertDto MapToDto(PriceAlert alert) => new()
    {
        Id = alert.Id,
        AreaId = alert.AreaId,
        ThresholdTotalPrice = alert.ThresholdTotalPrice,
        TargetVolumeInGallons = alert.TargetVolumeInGallons,
        Direction = alert.Direction,
        IsActive = alert.IsActive,
        LastTriggeredRateId = alert.LastTriggeredRateId,
        CreatedAt = alert.CreatedAt,
        UpdatedAt = alert.UpdatedAt
    };
}
