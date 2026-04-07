using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Vessel.API.Controllers;
using Vessel.API.Hubs;
using Vessel.Application.DTOs.Rates;
using Vessel.Application.Interfaces.Rates;
using Xunit;

namespace Vessel.Tests.Rates;

public class RatesControllerTests
{
    private readonly Mock<IRateService> _mockRateService;
    private readonly Mock<IHubContext<RateAlertHub>> _mockHubContext;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly RatesController _sut;

    public RatesControllerTests()
    {
        _mockRateService = new Mock<IRateService>();
        _mockHubContext = new Mock<IHubContext<RateAlertHub>>();
        _mockClientProxy = new Mock<IClientProxy>();
        
        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(c => c.All).Returns(_mockClientProxy.Object);
        _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        _sut = new RatesController(_mockRateService.Object, _mockHubContext.Object);
    }

    [Fact]
    public async Task Create_ProviderIdentityIsTakenFromJwt_NotRequestBody()
    {
        // Arrange
        var providerIdFromJwt = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, providerIdFromJwt.ToString())
        }, "mock"));

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var requestDto = new CreateRateDto { AreaId = Guid.NewGuid(), PricePerGallon = 2.5m };
        var createdRate = new RateDto { ProviderId = providerIdFromJwt, PricePerGallon = 2.5m, AreaId = requestDto.AreaId };

        _mockRateService.Setup(x => x.CreateRateAsync(providerIdFromJwt, requestDto))
            .ReturnsAsync(createdRate);

        // Act
        var result = await _sut.Create(requestDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdAtResult = result as CreatedAtActionResult;
        createdAtResult!.Value.Should().Be(createdRate);
        
        // Ensure the service was called with the JWT provider ID, not some arbitrary value.
        _mockRateService.Verify(x => x.CreateRateAsync(providerIdFromJwt, It.IsAny<CreateRateDto>()), Times.Once);
    }
}
