using Vessel.Application.DTOs.Areas;
using Vessel.Application.Interfaces.Areas;

namespace Vessel.Infrastructure.Mocks;

public class MockAreaService : IAreaService
{
    public Task<List<AreaDto>> GetAllAreasAsync()
    {
        return Task.FromResult(new List<AreaDto>
        {
            new AreaDto { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), City = "Islamabad", Name = "F-8", Latitude = 33.71, Longitude = 73.02 },
            new AreaDto { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), City = "Lahore", Name = "DHA Phase 5", Latitude = 31.47, Longitude = 74.41 },
            new AreaDto { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), City = "Karachi", Name = "Clifton", Latitude = 24.82, Longitude = 67.03 }
        });
    }
}
