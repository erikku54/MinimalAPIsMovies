using System;
using MinimalAPIsMovies.Entities;

namespace MinimalAPIsMovies.Repositories;

public interface IGenresRepository
{
    Task<int> Create(Genre genre);
    Task<List<Genre>> GetAll();
    Task<Genre?> GetById(int id);
    Task<bool> Exists(int id);
    Task<bool> Exists(int id, string name);
    Task<List<int>> Exists(List<int> ids);
    Task Update(Genre genre);
    Task Delete(int id);
}
