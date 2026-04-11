using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Vessel.Application.DTOs.Auth;
using Vessel.Application.Interfaces.Auth;
using Vessel.Application.Interfaces.Repositories;
using Vessel.Application.Services;
using Vessel.Core.Entities;
using Vessel.Core.Enums;
using Xunit;

namespace Vessel.Tests.Auth;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IPasswordHasherService> _passwordHasherMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _passwordHasherMock = new Mock<IPasswordHasherService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();

        var inMemorySettings = new System.Collections.Generic.Dictionary<string, string?>
        {
            {"Jwt:AccessTokenMinutes", "60"},
            {"Jwt:RefreshTokenDays", "7"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenServiceMock.Object,
            _configuration);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnAuthResponse_WhenDataIsValid()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "password123",
            FullName = "Test User"
        };

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _passwordHasherMock.Setup(h => h.HashPassword(request.Password))
            .Returns("hashed_password");

        _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        _jwtTokenServiceMock.Setup(s => s.GenerateAccessToken(It.IsAny<User>()))
            .Returns("mock_access_token");

        _jwtTokenServiceMock.Setup(s => s.GenerateRefreshToken())
            .Returns("mock_refresh_token");

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("mock_access_token");
        result.RefreshToken.Should().Be("mock_refresh_token");
        result.Role.Should().Be(UserRole.Consumer.ToString());
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResponse_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User { Id = Guid.NewGuid(), Email = request.Email, PasswordHash = "hashed_pw", Role = UserRole.Consumer };

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(h => h.VerifyPassword("hashed_pw", request.Password))
            .Returns(true);

        _jwtTokenServiceMock.Setup(s => s.GenerateAccessToken(user))
            .Returns("mock_access_token");

        _jwtTokenServiceMock.Setup(s => s.GenerateRefreshToken())
            .Returns("mock_refresh_token");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("mock_access_token");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenCredentialsAreInvalid()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "wrong_password"
        };

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> action = async () => await _authService.LoginAsync(request);

        // Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        var request = new RefreshTokenRequestDto { RefreshToken = "valid_refresh_token" };
        var userId = Guid.NewGuid();
        
        // Use the same hashing mechanism mock or rely on actual implementation
        // Since HashToken is private, we mock the repo to return a valid token for any string.
        var storedToken = new RefreshToken 
        { 
            Id = Guid.NewGuid(), 
            UserId = userId, 
            TokenHash = "hash", 
            ExpiresAt = DateTime.UtcNow.AddDays(1) 
        };

        _refreshTokenRepositoryMock.Setup(repo => repo.GetByTokenHashAsync(It.IsAny<string>()))
            .ReturnsAsync(storedToken);

        var user = new User { Id = userId, Email = "test@example.com", Role = UserRole.Consumer };
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _jwtTokenServiceMock.Setup(s => s.GenerateAccessToken(user))
            .Returns("new_access_token");

        _jwtTokenServiceMock.Setup(s => s.GenerateRefreshToken())
            .Returns("new_refresh_token");

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new_access_token");
        result.RefreshToken.Should().Be("new_refresh_token");
        storedToken.RevokedAt.Should().NotBeNull();
    }
}
