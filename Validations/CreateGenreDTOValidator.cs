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
            .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
            .MinimumLength(3).WithMessage("Name must be at least 3 characters long")
            .MaximumLength(150).WithMessage(ValidationUtilities.MaximumLengthMessage)
            .Must(ValidationUtilities.FirstLetterIsUpper).WithMessage(ValidationUtilities.FirstLetterIsUpperMessage)
            .MustAsync(async (name, _) =>
            {
                var exists = await genresRepository.Exists(id: 0, name);
                return !exists;
            }).WithMessage(createGenreDTO => $"The genre name '{createGenreDTO.Name}' already exists.");
    }

}