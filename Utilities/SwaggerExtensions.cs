using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace MinimalAPIsMovies.Utilities;

public static class SwaggerExtensions
{
    public static TBuilder AddPaginationParameters<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.WithOpenApi(operation =>
        {
            operation.Parameters.Add(
                new OpenApiParameter()
                {
                    Name = "Page",
                    In = ParameterLocation.Query,
                    Required = false,
                    Description = "Page number (optional)",
                    Schema = new OpenApiSchema()
                    {
                        Type = "integer",
                        Default = new OpenApiInteger(1),
                        Nullable = true,
                    },
                }
            );
            operation.Parameters.Add(
                new OpenApiParameter()
                {
                    Name = "RecordsPerPage",
                    In = ParameterLocation.Query,
                    Required = false,
                    Description = "Items per page (optional)",
                    Schema = new OpenApiSchema()
                    {
                        Type = "integer",
                        Default = new OpenApiInteger(10),
                        Nullable = true,
                    },
                }
            );

            return operation;
        });
    }
}
