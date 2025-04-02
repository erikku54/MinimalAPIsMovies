using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Repositories;

namespace MinimalAPIsMovies.Endpoints;

public static class CommentsEndpoints
{
    public static RouteGroupBuilder MapComments(this RouteGroupBuilder builder)
    {
        builder.MapPost("/", Create);
        return builder;
    }

    static async Task<Results<Created<CommentDTO>, NotFound>> Create(int movieId, CreateCommentDTO createCommentDTO, ICommentsRepository commentsRepository, IMoviesRepository moviesRepository, IOutputCacheStore outputCacheStore, IMapper mapper)
    {
        var exists = await moviesRepository.Exists(movieId);
        if (!exists) return TypedResults.NotFound();

        var comment = mapper.Map<Comment>(createCommentDTO);
        comment.MovieId = movieId;

        var id = await commentsRepository.Create(comment);
        await outputCacheStore.EvictByTagAsync("comments-get", default);

        var commentDTO2 = mapper.Map<CommentDTO>(comment);
        return TypedResults.Created($"/comments/{id}", commentDTO2);
    }

}
