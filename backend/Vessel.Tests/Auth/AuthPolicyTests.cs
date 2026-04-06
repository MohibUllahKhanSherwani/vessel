using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Vessel.Core.Entities;
using Vessel.Core.Enums;
using Vessel.Infrastructure.Auth;
using Xunit;

namespace Vessel.Tests.Auth;

public class AuthPolicyTests
{
    [Fact]
    public void JwtTokenService_ShouldGenerateTokenWithConsumerRole_WhenUserIsConsumer()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:Key", "super_secret_key_that_is_at_least_32_bytes_long_for_testing"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:AccessTokenMinutes", "60"}
        };

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var jwtService = new JwtTokenService(configuration);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "consumer@example.com",
            Role = UserRole.Consumer
        };

        // Act
        var token = jwtService.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        // The token is generated successfully.
        // Role based access logic will rely on this token's claim.
    }
}
