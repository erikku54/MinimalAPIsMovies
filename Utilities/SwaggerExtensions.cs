using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace MinimalAPIsMovies.Utilities;

public static class SwaggerExtensions
{
    public static TBuilder AddMoviesFilterParameters<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.WithOpenApi(operation =>
        {
            AddPaginationParameters(operation);

            operation.Parameters.Add(
                new OpenApiParameter()
                {
                    Name = "Title",
                    In = ParameterLocation.Query,
                    Required = false,
                    Description = "Movie title (optional)",
                    Schema = new OpenApiSchema() { Type = "string", Nullable = true },
                }
            );
            operation.Parameters.Add(
                new OpenApiParameter()
                {
                    Name = "GenreId",
                    In = ParameterLocation.Query,
                    Required = false,
                    Description = "Genre ID (optional)",
                    Schema = new OpenApiSchema() { Type = "integer", Nullable = true },
                }
            );
            operation.Parameters.Add(
                new OpenApiParameter()
                {
                    Name = "InTheaters",
                    In = ParameterLocation.Query,
                    Required = false,
                    Description = "In theaters (optional)",
                    Schema = new OpenApiSchema() { Type = "boolean", Nullable = true },
                }
            );
            operation.Parameters.Add(
                new OpenApiParameter()
                {
                    Name = "FutureReleases",
                    In = ParameterLocation.Query,
                    Required = false,
                    Description = "Future releases (optional)",
                    Schema = new OpenApiSchema() { Type = "boolean", Nullable = true },
                }
            );
            operation.Parameters.Add(
                new OpenApiParameter()
                {
                    Name = "OrderByField",
                    In = ParameterLocation.Query,
                    Required = false,
                    Description = "Order by field (optional)",
                    Schema = new OpenApiSchema()
                    {
                        Type = "string",
                        Enum = new List<IOpenApiAny>()
                        {
                            new OpenApiString("Title"),
                            new OpenApiString("ReleaseDate"),
                        },
                        Nullable = true,
                    },
                }
            );
            operation.Parameters.Add(
                new OpenApiParameter()
                {
                    Name = "OrderByAscending",
                    In = ParameterLocation.Query,
                    Required = false,
                    Description = "Order by ascending (optional)",
                    Schema = new OpenApiSchema()
                    {
                        Type = "boolean",
                        Nullable = true,
                        Default = new OpenApiBoolean(true),
                    },
                }
            );

            return operation;
        });
    }

    public static TBuilder AddPaginationParameters<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.WithOpenApi(operation =>
        {
            AddPaginationParameters(operation);
            return operation;
        });
    }

    private static void AddPaginationParameters(OpenApiOperation operation)
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
    }
}
