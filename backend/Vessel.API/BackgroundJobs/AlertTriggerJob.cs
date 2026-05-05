using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Vessel.API.Hubs;
using Vessel.Core.Enums;
using Vessel.Infrastructure.Data;
using Vessel.Application.Services.AI;
using Hangfire;

namespace Vessel.API.BackgroundJobs;

/// <summary>
/// Background job to check price alerts and notify consumers via SignalR.
/// </summary>
public class AlertTriggerJob
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<RateAlertHub> _hubContext;
    private readonly Hangfire.IBackgroundJobClient _backgroundJobClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlertTriggerJob"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="hubContext">The SignalR hub context used for alert notifications.</param>
    /// <param name="backgroundJobClient">The Hangfire client used to enqueue follow-up jobs.</param>
    public AlertTriggerJob(ApplicationDbContext context, IHubContext<RateAlertHub> hubContext, Hangfire.IBackgroundJobClient backgroundJobClient)
    {
        _context = context;
        _hubContext = hubContext;
        _backgroundJobClient = backgroundJobClient;
    }

    /// <summary>
    /// Runs the alert trigger check.
    /// </summary>
    public async Task RunAsync()
    {
        var activeAlerts = await _context.PriceAlerts
            .Where(a => a.IsActive)
            .ToListAsync();

        if (!activeAlerts.Any()) return;

        var areaIds = activeAlerts.Select(a => a.AreaId).Distinct();
        
        var currentRates = await _context.ProviderRates
            .Where(r => areaIds.Contains(r.AreaId) && r.EffectiveTo == null)
            .ToListAsync();

        foreach (var alert in activeAlerts)
        {
            // Find the best rate for this area (lowest price)
            var bestRate = currentRates
                .Where(r => r.AreaId == alert.AreaId)
                .OrderBy(r => r.PricePerGallon)
                .FirstOrDefault();

            if (bestRate == null) continue;

            // Skip if already triggered for this specific rate record
            if (alert.LastTriggeredRateId == bestRate.Id) continue;

            decimal currentTotalPrice = bestRate.PricePerGallon * alert.TargetVolumeInGallons;

            bool isTriggered = alert.Direction switch
            {
                AlertDirection.BelowOrEqual => currentTotalPrice <= alert.ThresholdTotalPrice,
                AlertDirection.AboveOrEqual => currentTotalPrice >= alert.ThresholdTotalPrice,
                _ => false
            };

            if (isTriggered)
            {
                // Update alert state
                alert.LastTriggeredRateId = bestRate.Id;
                alert.UpdatedAt = DateTimeOffset.UtcNow;

                // Notify user via SignalR targeted to their User ID
                await _hubContext.Clients.User(alert.ConsumerId.ToString())
                    .SendAsync("AlertTriggered", new
                    {
                        AlertId = alert.Id,
                        AreaId = alert.AreaId,
                        CurrentPrice = bestRate.PricePerGallon,
                        TotalAtCurrentPrice = currentTotalPrice,
                        Threshold = alert.ThresholdTotalPrice
                    });

                _backgroundJobClient.Enqueue<AiEmbeddingJob>(
                    j => j.ProcessAlertTriggerAsync(alert.Id, bestRate.Id));
            }
        }

        await _context.SaveChangesAsync();
    }
}
