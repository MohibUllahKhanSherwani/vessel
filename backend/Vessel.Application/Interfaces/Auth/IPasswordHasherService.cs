namespace Vessel.Application.Interfaces.Auth;

public interface IPasswordHasherService
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string password);
}