using Vessel.Core.Entities;

namespace Vessel.Application.Interfaces.Auth;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}