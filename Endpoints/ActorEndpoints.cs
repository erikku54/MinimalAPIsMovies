using System;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Repositories;

namespace MinimalAPIsMovies.Endpoints;

public static class ActorEndpoints
{
    public static RouteGroupBuilder MapActors(this RouteGroupBuilder builder)
    {
        builder.MapPost("/create", Create).DisableAntiforgery();


        return builder;
    }

    static async Task<Created<ActorDTO>> Create([FromForm] CreateActorDTO createActorDTO, IOutputCacheStore outputCacheStore, IActorsRepository actorsRepository, IMapper mapper)
    {
        var actor = mapper.Map<Actor>(createActorDTO);
        var id = await actorsRepository.Create(actor);

        await outputCacheStore.EvictByTagAsync("actors-get", default);
        var actorDTO = mapper.Map<ActorDTO>(actor);
        return TypedResults.Created($"/actors/{id}", actorDTO);
    }

}
