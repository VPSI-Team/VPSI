using FluentValidation;

namespace Parking.Application.Sessions.Commands.RegisterEntry;

public sealed class RegisterEntryCommandValidator : AbstractValidator<RegisterEntryCommand>
{
    public RegisterEntryCommandValidator()
    {
        RuleFor(x => x.PlateNumber)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.ParkingLotId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.CountryCode)
            .MaximumLength(3)
            .When(x => x.CountryCode is not null);
    }
}
