using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string authorName = builder.Configuration.GetValue<string>("AuthorName")!;


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(corsPolicyBuilder =>
    {
        corsPolicyBuilder.WithOrigins(builder.Configuration["AllowedOrigins"]!.Split(","))
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
    options.AddPolicy("free", corsPo =>
    {
        corsPo.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddOutputCache();

builder.Services.AddScoped<IGenresRepository, GenresRepository>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("free");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseOutputCache();


var genresEndpoints = app.MapGroup("/genres");

app.MapGet("/", () => authorName);

genresEndpoints.MapGet("/", async (IGenresRepository genresRepository) =>
{
    var genres = await genresRepository.GetAll();
    return TypedResults.Ok(genres);
}).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("genres-get"));

genresEndpoints.MapGet("/{id}", async (int id, IGenresRepository genresRepository) =>
{
    var genre = await genresRepository.GetById(id);
    if (genre is null)
    {
        return Results.NotFound();
        // return TypedResults.NotFound();
    }
    return TypedResults.Ok(genre);
});

genresEndpoints.MapPost("/", async (Genre genre, IGenresRepository genresRepository, IOutputCacheStore outputCacheStore) =>
{
    var id = await genresRepository.Create(genre);
    await outputCacheStore.EvictByTagAsync("genres-get", default);
    return TypedResults.Created($"/genres/{id}", genre);
});

genresEndpoints.MapPut("/{id:int}", async (int id, Genre genre, IGenresRepository genresRepository, IOutputCacheStore outputCacheStore) =>
{
    if (!await genresRepository.Exists(id))
    {
        return Results.NotFound();
    }
    genre.Id = id;
    await genresRepository.Update(genre);
    await outputCacheStore.EvictByTagAsync("genres-get", default);
    return TypedResults.Ok(genre);
});

genresEndpoints.MapDelete("/{id:int}", async (int id, IGenresRepository genresRepository, IOutputCacheStore outputCacheStore) =>
{
    if (!await genresRepository.Exists(id))
    {
        return Results.NotFound();
    }
    await genresRepository.Delete(id);
    await outputCacheStore.EvictByTagAsync("genres-get", default);
    return Results.NoContent();
});


genresEndpoints.MapPost("/seed", async (IGenresRepository genresRepository, IOutputCacheStore outputCacheStore) =>
{
    var genres = new List<Genre>() {
        new Genre { Id = 1, Name = "Action" },
        new Genre { Id = 2, Name = "Drama" },
        new Genre { Id = 3, Name = "Comedy" },
        new Genre { Id = 4, Name = "Fantasy" },
        new Genre { Id = 5, Name = "Horror" },
        new Genre { Id = 6, Name = "Mystery" },
        new Genre { Id = 7, Name = "Romance" },
        new Genre { Id = 8, Name = "Thriller" },
        new Genre { Id = 9, Name = "Western" }
    };

    foreach (var genre in genres)
    {
        await genresRepository.Create(genre);
    }
    // Task.WaitAll(genres.Select(genre => genresRepository.Create(genre)).ToArray());

    await outputCacheStore.EvictByTagAsync("genres-get", default);
    return TypedResults.Created("/genres", genres);
});

app.Run();
