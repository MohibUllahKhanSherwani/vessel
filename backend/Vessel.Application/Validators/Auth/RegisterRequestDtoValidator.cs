using FluentValidation;
using Vessel.Application.DTOs.Auth;

namespace Vessel.Application.Validators.Auth;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email is required!");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required!").MinimumLength(5).WithMessage("Password must be at least 6 characters long!");
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Full name is required!").MaximumLength(100).WithMessage("Full name must be at most 100 characters long!");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone number is required!");
    // No rules for Role, bcz it will be set to Consumer by default in the AuthService

    }
}