using FluentValidation;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Repositories;

namespace MinimalAPIsMovies.Validations;

public class CreateGenreDTOValidator : AbstractValidator<CreateGenreDTO>
{
    public CreateGenreDTOValidator(IGenresRepository genresRepository, IHttpContextAccessor httpContextAccessor)
    {
        var routeValueId = httpContextAccessor.HttpContext?.Request.RouteValues["id"];
        var id = 0;
        if (routeValueId is string routeValueIdString)
        {
            int.TryParse(routeValueIdString, out id);
        }

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("The field '{PropertyName}' is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters long")
            .MaximumLength(150).WithMessage("The field '{PropertyName}' must be less than {MaxLength} characters")
            .Must(FirstLetterIsUpper).WithMessage("The first letter of the field '{PropertyName}' must be uppercase")
            .MustAsync(async (name, _) =>
            {
                var exists = await genresRepository.Exists(id: 0, name);
                return !exists;
            }).WithMessage(createGenreDTO => $"The genre name '{createGenreDTO.Name}' already exists.");
    }

    private bool FirstLetterIsUpper(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return true;

        var firstLetter = name[0];
        return firstLetter == char.ToUpper(firstLetter);
    }
}