using System;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Repositories;
using MinimalAPIsMovies.Services;

namespace MinimalAPIsMovies.Endpoints;

public static class ActorEndpoints
{
    private readonly static string _container = "actors";
    public static RouteGroupBuilder MapActors(this RouteGroupBuilder builder)
    {
        builder.MapPost("/", Create).DisableAntiforgery();
        builder.MapGet("/", GetAll).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("actors-get"));
        builder.MapGet("/{id:int}", GetById);


        return builder;
    }

    static async Task<Created<ActorDTO>> Create([FromForm] CreateActorDTO createActorDTO, IOutputCacheStore outputCacheStore, IActorsRepository actorsRepository, IMapper mapper, IFileStorage fileStorage)
    {
        var actor = mapper.Map<Actor>(createActorDTO);

        if (createActorDTO.Picture is not null)
        {
            var url = await fileStorage.Store(_container, createActorDTO.Picture);
            actor.Picture = url;
        }

        var id = await actorsRepository.Create(actor);
        await outputCacheStore.EvictByTagAsync("actors-get", default);
        var actorDTO = mapper.Map<ActorDTO>(actor);
        return TypedResults.Created($"/actors/{id}", actorDTO);
    }

    static async Task<Ok<List<ActorDTO>>> GetAll(IActorsRepository actorsRepository, IMapper mapper)
    {
        var actors = await actorsRepository.GetAll();
        var actorsDTO = mapper.Map<List<ActorDTO>>(actors);
        return TypedResults.Ok(actorsDTO);
    }

    static async Task<Results<Ok<ActorDTO>, NotFound>> GetById(int id, IActorsRepository actorsRepository, IMapper mapper)
    {
        var actor = await actorsRepository.GetById(id);
        if (actor is null)
        {
            return TypedResults.NotFound();
        }

        var actorDTO = mapper.Map<ActorDTO>(actor);
        return TypedResults.Ok(actorDTO);
    }
}
