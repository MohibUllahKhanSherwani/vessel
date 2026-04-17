using Vessel.Application.DTOs.Analytics;

namespace Vessel.Application.Interfaces.Analytics;

public interface IAdminAnalyticsService
{
    Task<IEnumerable<TopProviderDto>> GetTopProvidersAsync(int count = 5);
    Task<IEnumerable<AveragePriceDto>> GetAveragePricesByCityAsync();
    Task<IEnumerable<VolumeTrendDto>> GetVolumeTrendsAsync(int days = 30);
    Task<IEnumerable<PriceTrendDto>> GetPriceTrendsAsync(int days = 30);
}
