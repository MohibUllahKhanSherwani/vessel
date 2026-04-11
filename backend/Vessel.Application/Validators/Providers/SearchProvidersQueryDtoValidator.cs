using FluentValidation;
using Vessel.Application.DTOs.Providers;

namespace Vessel.Application.Validators.Providers;

public class SearchProvidersQueryDtoValidator : AbstractValidator<SearchProvidersQueryDto>
{
    public SearchProvidersQueryDtoValidator()
    {
        RuleFor(x => x.Lat)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Lon)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180.");

        RuleFor(x => x.RadiusKm)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Radius must be greater than 0 and less than or equal to 100 km.");
    }
}
