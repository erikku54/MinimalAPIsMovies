using System;
using Dapper;
using Microsoft.Data.SqlClient;
using MinimalAPIsMovies.Entities;

namespace MinimalAPIsMovies.Repositories;

public class GenresRepository : IGenresRepository
{
    private readonly string _connectionString;

    public GenresRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }


    public async Task<int> Create(Genre genre)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var query = connection.Query("SELECT 1").FirstOrDefault();
            var query = @"INSERT INTO Genres (Name)
                          VALUES (@Name);

                          SELECT CAST(SCOPE_IDENTITY() as int);";

            var id = await connection.QuerySingleAsync<int>(query, genre);
            genre.Id = id;
            return id;
        }
    }

    public async Task Delete(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = "DELETE FROM Genres WHERE Id = @Id";
            await connection.ExecuteAsync(query, new { Id = id });
            return;
        }
    }

    public async Task<bool> Exists(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = "SELECT COUNT(1) FROM Genres WHERE Id = @Id";
            var count = await connection.QuerySingleAsync<int>(query, new { Id = id });
            return (count > 0);
        }
    }

    public async Task<List<Genre>> GetAll()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = "SELECT Id, Name FROM Genres ORDER BY Name";
            var genres = (await connection.QueryAsync<Genre>(query));
            return genres.ToList();
        }

    }

    public async Task<Genre?> GetById(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = "SELECT Id, Name FROM Genres WHERE Id = @Id";
            var genre = await connection.QueryFirstOrDefaultAsync<Genre>(query, new { Id = id });
            return genre;
        }
    }

    public async Task Update(Genre genre)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = "UPDATE Genres SET Name = @Name WHERE Id = @Id";
            await connection.ExecuteAsync(query, genre);

            return;
        }
    }


}
