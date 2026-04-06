using System;

namespace Vessel.Application.DTOs.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset AccessTokenExpiresAtUtc { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset RefreshTokenExpiresAtUtc { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
}
