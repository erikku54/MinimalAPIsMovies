using AutoMapper;
using MinimalAPIsMovies.Repositories;

namespace MinimalAPIsMovies.Filters;

public class TestFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // This code will execute before the endpoint handler
        var param1 = context.Arguments.OfType<int>().FirstOrDefault(); // int type argument of endpoint handler
        var param2 = context.Arguments.OfType<IGenresRepository>().FirstOrDefault(); // IGenresRepository type argument of endpoint handler
        var param3 = context.Arguments.OfType<IMapper>().FirstOrDefault(); // IMapper type argument of endpoint handler


        var result = await next(context);

        // This code will execute after the endpoint handler
        return result;
    }
}
