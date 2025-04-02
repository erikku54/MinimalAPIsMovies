using System;
using AutoMapper;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;

namespace MinimalAPIsMovies.Utilities;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Genre, GenreDTO>();
        CreateMap<CreateGenreDTO, Genre>();

        CreateMap<Actor, ActorDTO>();
        CreateMap<CreateActorDTO, Actor>()
            .ForMember(dest => dest.Picture, opt => opt.Ignore()); // Ignore Picture property during mapping

        CreateMap<Movie, MovieDTO>();
        CreateMap<CreateMovieDTO, Movie>()
            .ForMember(dest => dest.Poster, opt => opt.Ignore()); // Ignore Poster property during mapping;

        CreateMap<Comment, CommentDTO>();
        CreateMap<CreateCommentDTO, Comment>();
    }
}
