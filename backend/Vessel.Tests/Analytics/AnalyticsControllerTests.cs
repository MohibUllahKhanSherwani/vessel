using Microsoft.AspNetCore.Authorization;
using Vessel.API.Controllers;
using FluentAssertions;

namespace Vessel.Tests.Analytics;

public class AnalyticsControllerTests
{
    [Fact]
    public void AnalyticsController_ShouldHaveAdminOnlyPolicy()
    {
        // Arrange
        var type = typeof(AnalyticsController);

        // Act
        var attribute = type.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Policy.Should().Be("AdminOnly");
    }

    [Fact]
    public void AnalyticsController_ShouldHaveApiControllerAttribute()
    {
        // Arrange
        var type = typeof(AnalyticsController);

        // Act
        var hasAttribute = type.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.ApiControllerAttribute), true).Any();

        // Assert
        hasAttribute.Should().BeTrue();
    }
}
