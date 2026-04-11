using FluentAssertions;
using Vessel.Application.DTOs.Providers;
using Vessel.Application.Validators.Providers;
using Xunit;

namespace Vessel.Tests.Providers;

public class SearchProvidersQueryDtoValidatorTests
{
    private readonly SearchProvidersQueryDtoValidator _validator;

    public SearchProvidersQueryDtoValidatorTests()
    {
        _validator = new SearchProvidersQueryDtoValidator();
    }

    [Theory]
    [InlineData(-91, 0, 10)]
    [InlineData(91, 0, 10)]
    [InlineData(0, -181, 10)]
    [InlineData(0, 181, 10)]
    [InlineData(0, 0, 0)]
    [InlineData(0, 0, 101)]
    public void Validator_ShouldHaveErrors_WhenCoordinatesOrRadiusAreInvalid(double lat, double lon, double radius)
    {
        // Arrange
        var query = new SearchProvidersQueryDto { Lat = lat, Lon = lon, RadiusKm = radius };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_ShouldBeValid_WhenDataIsCorrect()
    {
        // Arrange
        var query = new SearchProvidersQueryDto { Lat = 33.7, Lon = 73.0, RadiusKm = 50 };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
