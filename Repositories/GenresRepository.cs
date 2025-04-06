using System;
using System.Data;
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
            /* var query = @"INSERT INTO Genres (Name) VALUES (@Name);
                             SELECT CAST(SCOPE_IDENTITY() as int);";*/

            var id = await connection.QuerySingleAsync<int>("Genres_Create", new { genre.Name }, commandType: CommandType.StoredProcedure);
            genre.Id = id;
            return id;
        }
    }

    public async Task Delete(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var query = "DELETE FROM Genres WHERE Id = @Id";
            await connection.ExecuteAsync("Genres_Delete", new { Id = id }, commandType: CommandType.StoredProcedure);
            return;
        }
    }

    public async Task<bool> Exists(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var query = "SELECT COUNT(1) FROM Genres WHERE Id = @Id";
            var count = await connection.QuerySingleAsync<int>("Genres_Exists", new { Id = id }, commandType: CommandType.StoredProcedure);
            return (count > 0);
        }
    }

    public async Task<List<int>> Exists(List<int> ids)
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));

        foreach (var id in ids)
        {
            dt.Rows.Add(id);
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            // var query = "SELECT Id FROM Genres WHERE Id IN @Ids";
            var idsOfGenresThatExists = await connection.QueryAsync<int>("Genres_GetBySeveralIds", new { genresIds = dt }, commandType: CommandType.StoredProcedure);
            return idsOfGenresThatExists.ToList();
        }
    }

    public async Task<List<Genre>> GetAll()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var query = "SELECT Id, Name FROM Genres ORDER BY Name";
            var genres = (await connection.QueryAsync<Genre>(@"Genres_GetAll", commandType: CommandType.StoredProcedure));
            return genres.ToList();
        }

    }

    public async Task<Genre?> GetById(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var query = "SELECT Id, Name FROM Genres WHERE Id = @Id";
            var genre = await connection.QueryFirstOrDefaultAsync<Genre>("Genres_GetById", new { Id = id }, commandType: CommandType.StoredProcedure);
            return genre;
        }
    }

    public async Task Update(Genre genre)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var query = "UPDATE Genres SET Name = @Name WHERE Id = @Id";
            await connection.ExecuteAsync("Genres_Update", new { genre.Id, genre.Name }, commandType: CommandType.StoredProcedure);

            return;
        }
    }


}
