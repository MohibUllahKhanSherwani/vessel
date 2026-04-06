using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Vessel.Application.DTOs.Auth;
using Vessel.Application.Interfaces.Auth;
using Vessel.Application.Interfaces.Repositories;
using Vessel.Core.Entities;
using Vessel.Core.Enums;

namespace Vessel.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasherService passwordHasher,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new Exception("User already exists."); // Note: Will map to 409 Conflict later

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = UserRole.Consumer, 
            IsActive = true
        };

        var createdUser = await _userRepository.AddAsync(user);
        return await GenerateAuthResponseAsync(createdUser);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
            throw new Exception("Invalid email or password.");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (storedToken == null || storedToken.RevokedAt != null || storedToken.ExpiresAt < DateTime.UtcNow)
            throw new Exception("Invalid refresh token."); // Note: Maps to 401 Unauthorized

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user == null)
            throw new Exception("User not found.");

        storedToken.RevokedAt = DateTime.UtcNow;
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        storedToken.ReplacedByTokenHash = HashToken(newRefreshToken);
        await _refreshTokenRepository.UpdateAsync(storedToken);

        return await GenerateAuthResponseAsync(user, newRefreshToken);
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user, string? newRefreshToken = null)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = newRefreshToken ?? _jwtTokenService.GenerateRefreshToken();
        
        var accessTokenMinutes = int.Parse(_configuration["Jwt:AccessTokenMinutes"]!);
        var refreshTokenDays = int.Parse(_configuration["Jwt:RefreshTokenDays"]!);

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays)
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(accessTokenMinutes),
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(refreshTokenDays),
            UserId = user.Id,
            Role = user.Role.ToString()
        };
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
