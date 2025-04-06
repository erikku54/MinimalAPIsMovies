using System;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Repositories;
using MinimalAPIsMovies.Services;

namespace MinimalAPIsMovies.Endpoints;

public static class MoviesEndpoints
{
    private readonly static string _container = "movies";

    public static RouteGroupBuilder MapMovies(this RouteGroupBuilder builder)
    {
        builder.MapPost("/{id:int}/assignGenres", AssignGenres);
        builder.MapPost("/", Create).DisableAntiforgery();
        builder.MapDelete("/{id:int}", Delete);
        builder.MapGet("/", GetAll).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("movies-get"));
        builder.MapGet("/{id:int}", GetById);
        builder.MapPut("/{id:int}", Update).DisableAntiforgery();

        return builder;
    }

    static async Task<Created<MovieDTO>> Create([FromForm] CreateMovieDTO createMovieDTO, IFileStorage fileStorage, IMoviesRepository moviesRepository, IOutputCacheStore outputCacheStore, IMapper mapper)
    {
        var movie = mapper.Map<Movie>(createMovieDTO);

        if (createMovieDTO.Poster is not null)
        {
            var url = await fileStorage.Store(_container, createMovieDTO.Poster);
            movie.Poster = url;
        }

        var id = await moviesRepository.Create(movie);
        movie.Id = id;
        await outputCacheStore.EvictByTagAsync("movies-get", default);
        var movieDTO = mapper.Map<MovieDTO>(movie);

        return TypedResults.Created($"/movies/{id}", movieDTO);
    }

    static async Task<Results<NoContent, NotFound>> Delete(int id, IMoviesRepository moviesRepository, IFileStorage fileStorage, IOutputCacheStore outputCacheStore)
    {
        var movieDB = await moviesRepository.GetById(id);
        if (movieDB is null) return TypedResults.NotFound();

        await fileStorage.Delete(movieDB.Poster, _container);
        await moviesRepository.Delete(id);
        await outputCacheStore.EvictByTagAsync("movies-get", default);
        return TypedResults.NoContent();
    }

    static async Task<Ok<List<MovieDTO>>> GetAll(IMoviesRepository moviesRepository, IMapper mapper, int page = 1, int recordsPerPage = 10)
    {
        var pagination = new PaginationDTO { Page = page, RecordsPerPage = recordsPerPage };
        var movies = await moviesRepository.GetAll(pagination);
        var moviesDTO = mapper.Map<List<MovieDTO>>(movies);
        return TypedResults.Ok(moviesDTO);
    }

    static async Task<Results<Ok<MovieDTO>, NotFound>> GetById(int id, IMoviesRepository moviesRepository, IMapper mapper)
    {
        var movie = await moviesRepository.GetById(id);
        if (movie is null) return TypedResults.NotFound();
        var movieDTO = mapper.Map<MovieDTO>(movie);
        return TypedResults.Ok(movieDTO);
    }

    static async Task<Results<NoContent, NotFound>> Update(int id, [FromForm] CreateMovieDTO createMovieDTO, IFileStorage fileStorage, IMoviesRepository moviesRepository, IOutputCacheStore outputCacheStore, IMapper mapper)
    {
        var movieDB = await moviesRepository.GetById(id);
        if (movieDB is null) return TypedResults.NotFound();

        var movieForUpdate = mapper.Map<Movie>(createMovieDTO);
        movieForUpdate.Id = id;
        movieForUpdate.Poster = movieDB.Poster; // Keep the existing poster if not updated

        if (createMovieDTO.Poster is not null)
        {
            var url = await fileStorage.Edit(movieForUpdate.Poster, _container, createMovieDTO.Poster);
            movieForUpdate.Poster = url;
        }

        await moviesRepository.Update(movieForUpdate);
        await outputCacheStore.EvictByTagAsync("movies-get", default);
        return TypedResults.NoContent();
    }

    static async Task<Results<NoContent, NotFound, BadRequest<string>>> AssignGenres(int id, List<int> genresIds, IMoviesRepository moviesRepository, IGenresRepository genresRepository)
    {
        var movie = await moviesRepository.GetById(id);
        if (movie is null) return TypedResults.NotFound();

        if (genresIds.Count == 0) return TypedResults.NoContent();

        var genresThatExists = await genresRepository.Exists(genresIds);
        if (genresThatExists.Count != genresIds.Count)
        {
            var nonExistingIds = genresIds.Except(genresThatExists).ToList();
            var nonExistingIdsCSV = string.Join(", ", nonExistingIds);
            return TypedResults.BadRequest($"The following genres do not exist: {nonExistingIdsCSV}");
        }

        await moviesRepository.Assign(id, genresIds);
        // await outputCacheStore.EvictByTagAsync("movies-get", default);
        return TypedResults.NoContent();
    }
}
