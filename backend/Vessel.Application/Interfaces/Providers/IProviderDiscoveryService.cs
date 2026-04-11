using Vessel.Application.DTOs.Providers;

namespace Vessel.Application.Interfaces.Providers;

public interface IProviderDiscoveryService
{
    Task<List<ProviderSearchResultDto>> SearchProvidersAsync(SearchProvidersQueryDto query);
}
