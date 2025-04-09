using FluentValidation;
using MinimalAPIsMovies.DTOs;

namespace MinimalAPIsMovies.Validations;

public class CreateActorDTOValidator : AbstractValidator<CreateActorDTO>
{
    public CreateActorDTOValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("The field '{PropertyName}' is required")
            .MaximumLength(150).WithMessage("The field '{PropertyName}' must be less than {MaxLength} characters");

        var minimunDate = new DateTime(1900, 1, 1);
        RuleFor(x => x.DateOfBirth)
            .GreaterThanOrEqualTo(minimunDate).WithMessage($"The field '{nameof(CreateActorDTO.DateOfBirth)}' must be greater than {minimunDate.ToShortDateString()}");
    }
}
