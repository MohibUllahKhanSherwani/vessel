using FluentValidation;
using Vessel.Application.DTOs.Alerts;

namespace Vessel.Application.Validators.Alerts;

public class UpdatePriceAlertDtoValidator : AbstractValidator<UpdatePriceAlertDto>
{
    public UpdatePriceAlertDtoValidator()
    {
        RuleFor(x => x.ThresholdTotalPrice)
            .GreaterThan(0)
            .WithMessage("Threshold total price must be greater than 0.");

        RuleFor(x => x.TargetVolumeInGallons)
            .GreaterThan(0)
            .WithMessage("Target volume must be greater than 0.");

        RuleFor(x => x.Direction)
            .IsInEnum()
            .WithMessage("Invalid alert direction.");
    }
}
