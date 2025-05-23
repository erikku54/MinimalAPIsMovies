using System;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;

namespace MinimalAPIsMovies.Repositories;

public interface IMoviesRepository
{
    Task Assign(int id, List<int> genresIds);
    Task Assign(int id, List<ActorMovie> actors);
    Task<int> Create(Movie movie);
    Task Delete(int id);
    Task<bool> Exists(int id);
    Task<List<Movie>> GetAll(PaginationDTO paginationDTO);
    Task<Movie> GetById(int id);
    Task Update(Movie movie);
    Task<List<Movie>> GetByFilter(MoviesFilterDTO filterDTO);
}
