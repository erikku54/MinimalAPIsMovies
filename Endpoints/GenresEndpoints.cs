using System;
using System.Linq;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Repositories;

namespace MinimalAPIsMovies.Endpoints;

public static class GenresEndpoints
{
    public static RouteGroupBuilder MapGenres(this RouteGroupBuilder builder)
    {
        builder.MapPost("/", Create);
        builder.MapGet("/", GetGenres).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("genres-get"));
        builder.MapGet("/{id}", GetById);
        builder.MapPut("/{id:int}", Update);
        builder.MapDelete("/{id:int}", Delete);
        builder.MapPost("/seed", SeedGenres);

        return builder;
    }

    static async Task<Results<Created<GenreDTO>, ValidationProblem>> Create(CreateGenreDTO createGenreDTO, IGenresRepository genresRepository, IOutputCacheStore outputCacheStore, IMapper mapper, IValidator<CreateGenreDTO> validator)
    {
        var validationResult = await validator.ValidateAsync(createGenreDTO);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var genre = mapper.Map<Genre>(createGenreDTO);

        var id = await genresRepository.Create(genre);

        await outputCacheStore.EvictByTagAsync("genres-get", default);

        var genreDTO = mapper.Map<GenreDTO>(genre);
        return TypedResults.Created($"/genres/{id}", genreDTO);
    }

    static async Task<Ok<List<GenreDTO>>> GetGenres(IGenresRepository genresRepository, IMapper mapper)
    {
        var genres = await genresRepository.GetAll();

        var genreDTOs = mapper.Map<List<GenreDTO>>(genres);
        return TypedResults.Ok(genreDTOs);
    }

    static async Task<Results<Ok<GenreDTO>, NotFound>> GetById(int id, IGenresRepository genresRepository, IMapper mapper)
    {
        var genre = await genresRepository.GetById(id);
        if (genre is null)
        {
            // return Results.NotFound();
            return TypedResults.NotFound();
        }

        var genreDTO = mapper.Map<GenreDTO>(genre);
        return TypedResults.Ok(genreDTO);
    }

    static async Task<Results<Ok<GenreDTO>, NotFound, ValidationProblem>> Update(int id, CreateGenreDTO createGenreDTO, IGenresRepository genresRepository, IOutputCacheStore outputCacheStore, IMapper mapper, IValidator<CreateGenreDTO> validator)
    {
        var validationResult = await validator.ValidateAsync(createGenreDTO);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        // Check if the genre exists
        if (!await genresRepository.Exists(id))
        {
            return TypedResults.NotFound();
        }

        // Check if the genre name already exists
        var exists = await genresRepository.Exists(id: 0, createGenreDTO.Name);
        if (exists)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                { nameof(createGenreDTO.Name), new[] { $"The genre name '{createGenreDTO.Name}' already exists." } }
            });
        }



        if (!await genresRepository.Exists(id))
        {
            return TypedResults.NotFound();
        }

        // var genre = new Genre { Id = id, Name = createGenreDTO.Name };
        var genre = mapper.Map<Genre>(createGenreDTO);
        genre.Id = id;

        await genresRepository.Update(genre);
        await outputCacheStore.EvictByTagAsync("genres-get", default);

        var genreDTO = mapper.Map<GenreDTO>(genre);
        return TypedResults.Ok(genreDTO);
    }

    static async Task<Results<NoContent, NotFound>> Delete(int id, IGenresRepository genresRepository, IOutputCacheStore outputCacheStore)
    {
        if (!await genresRepository.Exists(id))
        {
            return TypedResults.NotFound();
        }
        await genresRepository.Delete(id);
        await outputCacheStore.EvictByTagAsync("genres-get", default);
        return TypedResults.NoContent();
    }

    static async Task<Created<List<GenreDTO>>> SeedGenres(IGenresRepository genresRepository, IOutputCacheStore outputCacheStore, IMapper mapper)
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

        var genreDTOs = mapper.Map<List<GenreDTO>>(genres);
        return TypedResults.Created("/genres", genreDTOs);
    }
}
