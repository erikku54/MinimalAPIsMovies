using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Filters;
using MinimalAPIsMovies.Repositories;
using MinimalAPIsMovies.Services;

namespace MinimalAPIsMovies.Endpoints;

public static class CommentsEndpoints
{
    public static RouteGroupBuilder MapComments(this RouteGroupBuilder builder)
    {
        builder.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateCommentDTO>>()
            .RequireAuthorization();
        builder.MapGet("/", GetAll).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("comments-get"));
        builder.MapGet("/{id:int}", GetById).WithName("GetCommentById");
        builder.MapPut("/{id:int}", Update)
            .AddEndpointFilter<ValidationFilter<CreateCommentDTO>>()
            .RequireAuthorization();
        builder.MapDelete("/{id:int}", Delete).RequireAuthorization();
        return builder;
    }

    static async Task<Results<CreatedAtRoute<CommentDTO>, NotFound, BadRequest<string>>> Create(int movieId, CreateCommentDTO createCommentDTO, ICommentsRepository commentsRepository, IMoviesRepository moviesRepository, IUsersService usersService, IOutputCacheStore outputCacheStore, IMapper mapper)
    {
        var exists = await moviesRepository.Exists(movieId);
        if (!exists) return TypedResults.NotFound();

        var user = await usersService.GetUser();
        if (user is null) return TypedResults.BadRequest("User not found");

        var comment = mapper.Map<Comment>(createCommentDTO);
        comment.MovieId = movieId;
        comment.UserId = user.Id;

        var id = await commentsRepository.Create(comment);
        await outputCacheStore.EvictByTagAsync("comments-get", default);

        var commentDTO = mapper.Map<CommentDTO>(comment);
        return TypedResults.CreatedAtRoute(commentDTO, "GetCommentById", new { id, movieId });
    }

    static async Task<Results<NoContent, NotFound, ForbidHttpResult>> Delete(int movieId, int id, ICommentsRepository commentsRepository, IUsersService usersService, IMoviesRepository moviesRepository, IOutputCacheStore outputCacheStore)
    {
        var exists = await moviesRepository.Exists(movieId);
        if (!exists) return TypedResults.NotFound();

        var commentFromDB = await commentsRepository.GetById(id);
        if (commentFromDB is null) return TypedResults.NotFound();

        var user = await usersService.GetUser();
        if (user is null) return TypedResults.NotFound();

        if (commentFromDB.UserId != user.Id) return TypedResults.Forbid(); // Forbidden

        await commentsRepository.Delete(id);
        await outputCacheStore.EvictByTagAsync("comments-get", default);

        return TypedResults.NoContent();
    }

    static async Task<Results<Ok<List<CommentDTO>>, NotFound>> GetAll(int movieId, ICommentsRepository commentsRepository, IMoviesRepository moviesRepository, IMapper mapper)
    {
        var exists = await moviesRepository.Exists(movieId);
        if (!exists) return TypedResults.NotFound();

        var comments = await commentsRepository.GetAll(movieId);

        var commentsDTO = mapper.Map<List<CommentDTO>>(comments);
        return TypedResults.Ok(commentsDTO);
    }

    static async Task<Results<Ok<CommentDTO>, NotFound>> GetById(int movieId, int id, ICommentsRepository commentsRepository, IMoviesRepository moviesRepository, IMapper mapper)
    {
        var exists = await moviesRepository.Exists(movieId);
        if (!exists) return TypedResults.NotFound();

        var comment = await commentsRepository.GetById(id);
        if (comment is null) return TypedResults.NotFound();

        var commentDTO = mapper.Map<CommentDTO>(comment);
        return TypedResults.Ok(commentDTO);
    }

    static async Task<Results<NoContent, NotFound, ForbidHttpResult>> Update(int movieId, int id, CreateCommentDTO createCommentDTO, ICommentsRepository commentsRepository, IMoviesRepository moviesRepository, IUsersService usersService, IOutputCacheStore outputCacheStore, IMapper mapper)
    {
        var exists = await moviesRepository.Exists(movieId);
        if (!exists) return TypedResults.NotFound();

        var commentFromDB = await commentsRepository.GetById(id);
        if (commentFromDB is null) return TypedResults.NotFound();

        var user = await usersService.GetUser();
        if (user is null) return TypedResults.NotFound();

        if (commentFromDB.UserId != user.Id) return TypedResults.Forbid(); // Forbidden

        commentFromDB.Body = createCommentDTO.Body;

        await commentsRepository.Update(commentFromDB);
        await outputCacheStore.EvictByTagAsync("comments-get", default);

        return TypedResults.NoContent();
    }
}
