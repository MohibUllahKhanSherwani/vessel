using Microsoft.EntityFrameworkCore;
using Vessel.Application.DTOs.Analytics;
using Vessel.Application.Interfaces.Analytics;
using Vessel.Core.Enums;
using Vessel.Infrastructure.Data;

namespace Vessel.Infrastructure.Services.Analytics;

public class AdminAnalyticsService : IAdminAnalyticsService
{
    private readonly ApplicationDbContext _context;

    public AdminAnalyticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TopProviderDto>> GetTopProvidersAsync(int count = 5)
    {
        var query = from b in _context.Bookings
                    join p in _context.Providers on b.ProviderId equals p.Id
                    where b.Status == BookingStatus.Confirmed
                    group new { b, p } by new { b.ProviderId, p.CompanyName } into g
                    select new TopProviderDto
                    {
                        ProviderId = g.Key.ProviderId,
                        CompanyName = g.Key.CompanyName,
                        ConfirmedBookingCount = g.Count(),
                        TotalGallons = (decimal)g.Sum(x => x.b.VolumeInGallons)
                    };

        return await query
            .OrderByDescending(x => x.ConfirmedBookingCount)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<AveragePriceDto>> GetAveragePricesByCityAsync()
    {
        var query = from r in _context.ProviderRates
                    join a in _context.Areas on r.AreaId equals a.Id
                    where r.EffectiveTo == null
                    group new { r, a } by a.City into g
                    select new AveragePriceDto
                    {
                        City = g.Key,
                        AveragePricePerGallon = g.Average(x => x.r.PricePerGallon),
                        ActiveProviderCount = g.Select(x => x.r.ProviderId).Distinct().Count()
                    };

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<VolumeTrendDto>> GetVolumeTrendsAsync(int days = 30)
    {
        var startDate = DateTimeOffset.UtcNow.AddDays(-days);

        return await _context.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed && b.CreatedAt >= startDate)
            .GroupBy(b => b.CreatedAt.Date)
            .Select(g => new VolumeTrendDto
            {
                Date = new DateTimeOffset(g.Key, TimeSpan.Zero),
                ConfirmedBookingCount = g.Count(),
                TotalGallons = (decimal)g.Sum(b => b.VolumeInGallons)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<PriceTrendDto>> GetPriceTrendsAsync(int days = 30)
    {
        var startDate = DateTimeOffset.UtcNow.AddDays(-days);

        return await _context.Bookings
            .Where(b => b.CreatedAt >= startDate)
            .GroupBy(b => b.CreatedAt.Date)
            .Select(g => new PriceTrendDto
            {
                Date = new DateTimeOffset(g.Key, TimeSpan.Zero),
                AveragePricePerGallon = g.Average(b => b.PricePerGallonSnapshot)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
    }
}
