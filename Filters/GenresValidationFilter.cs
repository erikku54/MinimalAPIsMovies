
// 可被泛型ValidationFilter<T>所取代

using FluentValidation;
using MinimalAPIsMovies.DTOs;

namespace MinimalAPIsMovies.Filters;

public class GenresValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // 取得IValidator<CreateGenreDTO>型別的驗證器
        var validator = context.HttpContext.RequestServices.GetService<IValidator<CreateGenreDTO>>();
        if (validator is null)
        {
            return await next(context);
        }

        // 取得型別為CreateGenreDTO的端點參數(要驗證的物件)
        var obj = context.Arguments.OfType<CreateGenreDTO>().FirstOrDefault();
        if (obj is null)
        {
            return Results.Problem("The object to validate could not be found.");
        }

        var validationResult = await validator.ValidateAsync(obj);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        return await next(context);
    }
}
