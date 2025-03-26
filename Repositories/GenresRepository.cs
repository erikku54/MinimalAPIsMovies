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
}
