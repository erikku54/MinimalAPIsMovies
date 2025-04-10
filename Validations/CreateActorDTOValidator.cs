using FluentValidation;
using MinimalAPIsMovies.DTOs;

namespace MinimalAPIsMovies.Validations;

public class CreateActorDTOValidator : AbstractValidator<CreateActorDTO>
{
    public CreateActorDTOValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
            .MaximumLength(150).WithMessage(ValidationUtilities.MaximumLengthMessage);

        var minimunDate = new DateTime(1900, 1, 1);
        RuleFor(x => x.DateOfBirth)
            .GreaterThanOrEqualTo(minimunDate).WithMessage(ValidationUtilities.GreaterThanDateMessage(minimunDate));
    }
}
