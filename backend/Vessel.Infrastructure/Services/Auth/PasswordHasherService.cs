using BCrypt.Net;
using Vessel.Application.Interfaces.Auth;

namespace Vessel.Infrastructure.Services.Auth;

public class PasswordHasherService : IPasswordHasherService
{
    private const int WorkFactor = 11;

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string hashedPassword, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
