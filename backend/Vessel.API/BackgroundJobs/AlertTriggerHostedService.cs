using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vessel.API.Hubs;
using Vessel.Core.Enums;
using Vessel.Infrastructure.Data;

namespace Vessel.API.BackgroundJobs;

/// <summary>
/// Timer-based hosted service that checks price alerts every 5 minutes.
/// Replaces the Hangfire recurring job — no Docker or external DB required.
/// </summary>
public class AlertTriggerHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AlertTriggerHostedService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public AlertTriggerHostedService(IServiceScopeFactory scopeFactory, ILogger<AlertTriggerHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AlertTriggerHostedService started. Checking every {Interval}.", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_interval, stoppingToken);

            try
            {
                await RunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while running alert trigger check.");
            }
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<RateAlertHub>>();

        var activeAlerts = await context.PriceAlerts
            .Where(a => a.IsActive)
            .ToListAsync(cancellationToken);

        if (!activeAlerts.Any()) return;

        var areaIds = activeAlerts.Select(a => a.AreaId).Distinct();

        var currentRates = await context.ProviderRates
            .Where(r => areaIds.Contains(r.AreaId) && r.EffectiveTo == null)
            .ToListAsync(cancellationToken);

        foreach (var alert in activeAlerts)
        {
            var bestRate = currentRates
                .Where(r => r.AreaId == alert.AreaId)
                .OrderBy(r => r.PricePerGallon)
                .FirstOrDefault();

            if (bestRate == null) continue;
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
                alert.LastTriggeredRateId = bestRate.Id;
                alert.UpdatedAt = DateTimeOffset.UtcNow;

                await hubContext.Clients.User(alert.ConsumerId.ToString())
                    .SendAsync("AlertTriggered", new
                    {
                        AlertId = alert.Id,
                        AreaId = alert.AreaId,
                        CurrentPrice = bestRate.PricePerGallon,
                        TotalAtCurrentPrice = currentTotalPrice,
                        Threshold = alert.ThresholdTotalPrice
                    }, cancellationToken);

                _logger.LogInformation("Alert {AlertId} triggered for consumer {ConsumerId}.", alert.Id, alert.ConsumerId);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
