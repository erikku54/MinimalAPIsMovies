using System;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Filters;
using MinimalAPIsMovies.Repositories;
using MinimalAPIsMovies.Services;

namespace MinimalAPIsMovies.Endpoints;

public static class ActorEndpoints
{
    private static readonly string _container = "actors";

    public static RouteGroupBuilder MapActors(this RouteGroupBuilder builder)
    {
        builder
            .MapPost("/", Create)
            .DisableAntiforgery()
            .AddEndpointFilter<ValidationFilter<CreateActorDTO>>()
            .RequireAuthorization("isadmin");
        builder.MapDelete("/{id:int}", Delete).RequireAuthorization("isadmin");
        builder
            .MapGet("/", GetAll)
            .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("actors-get"));
        builder.MapGet("/{id:int}", GetById);
        builder.MapGet("/getByName/{name}", GetByName);
        builder
            .MapPut("/{id:int}", Update)
            .DisableAntiforgery()
            .AddEndpointFilter<ValidationFilter<CreateActorDTO>>()
            .RequireAuthorization("isadmin");

        return builder;
    }

    static async Task<Created<ActorDTO>> Create(
        [FromForm] CreateActorDTO createActorDTO,
        IOutputCacheStore outputCacheStore,
        IActorsRepository actorsRepository,
        IMapper mapper,
        IFileStorage fileStorage
    )
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

    static async Task<Ok<List<ActorDTO>>> GetAll(
        IActorsRepository actorsRepository,
        IMapper mapper,
        PaginationDTO pagination
    )
    {
        var actors = await actorsRepository.GetAll(pagination);
        var actorsDTO = mapper.Map<List<ActorDTO>>(actors);
        return TypedResults.Ok(actorsDTO);
    }

    static async Task<Results<Ok<ActorDTO>, NotFound>> GetById(
        int id,
        IActorsRepository actorsRepository,
        IMapper mapper
    )
    {
        var actor = await actorsRepository.GetById(id);
        if (actor is null)
        {
            return TypedResults.NotFound();
        }

        var actorDTO = mapper.Map<ActorDTO>(actor);
        return TypedResults.Ok(actorDTO);
    }

    static async Task<Ok<List<ActorDTO>>> GetByName(
        string name,
        IActorsRepository actorsRepository,
        IMapper mapper
    )
    {
        var actors = await actorsRepository.GetByName(name);
        var actorsDTO = mapper.Map<List<ActorDTO>>(actors);
        return TypedResults.Ok(actorsDTO);
    }

    static async Task<Results<NoContent, NotFound>> Update(
        int id,
        [FromForm] CreateActorDTO createActorDTO,
        IActorsRepository actorsRepository,
        IFileStorage fileStorage,
        IOutputCacheStore outputCacheStore,
        IMapper mapper
    )
    {
        var actor = await actorsRepository.GetById(id);
        if (actor is null)
        {
            return TypedResults.NotFound();
        }

        var actorForUpdate = mapper.Map<Actor>(createActorDTO);
        actorForUpdate.Id = id;
        // (Keep the old picture)因為格式不同，Map在處理此轉換時忽略Picture，要另外處理
        actorForUpdate.Picture = actor.Picture;

        if (createActorDTO.Picture is not null)
        {
            var url = await fileStorage.Edit(
                actorForUpdate.Picture,
                _container,
                createActorDTO.Picture
            );
            actorForUpdate.Picture = url;
        }

        await actorsRepository.Update(actorForUpdate);
        await outputCacheStore.EvictByTagAsync("actors-get", default);
        return TypedResults.NoContent();
    }

    static async Task<Results<NoContent, NotFound>> Delete(
        int id,
        IActorsRepository actorsRepository,
        IFileStorage fileStorage,
        IOutputCacheStore outputCacheStore
    )
    {
        var actor = await actorsRepository.GetById(id);
        if (actor is null)
        {
            return TypedResults.NotFound();
        }

        await actorsRepository.Delete(id);
        await fileStorage.Delete(actor.Picture, _container);
        await outputCacheStore.EvictByTagAsync("actors-get", default);
        return TypedResults.NoContent();
    }
}
