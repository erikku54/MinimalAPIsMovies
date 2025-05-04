using System;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;

namespace MinimalAPIsMovies.Repositories;

public class ActorsRepository : IActorsRepository
{
    private readonly string _connectionString;
    private readonly HttpContext _httpContext;

    public ActorsRepository(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _httpContext = httpContextAccessor.HttpContext!;
    }

    public async Task<int> Create(Actor actor)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var query = "INSERT INTO Actors (Name, DateOfBirth, Picture) VALUES (@Name, @DateOfBirth, @Picture); SELECT CAST(SCOPE_IDENTITY() as int)";
            var id = await connection.QuerySingleAsync<int>(
                "Actors_Create",
                new
                {
                    actor.Name,
                    actor.DateOfBirth,
                    actor.Picture,
                },
                commandType: CommandType.StoredProcedure
            );

            actor.Id = id;
            return id;
        }
    }

    public async Task Delete(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var query = "DELETE FROM Actors WHERE Id = @Id";
            await connection.ExecuteAsync(
                "Actors_Delete",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
            return;
        }
    }

    public async Task<bool> Exists(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var query = "SELECT COUNT(1) FROM Actors WHERE Id = @Id";
            var exists = await connection.ExecuteScalarAsync<bool>(
                "Actors_Exists",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
            return exists;
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
            // var query = "SELECT Id FROM Actors a INNER JOIN @actorsIds ai ON a.Id = ai.Id";
            var idsOfExistingActors = await connection.QueryAsync<int>(
                "Actors_GetBySeveralIds",
                new { actorsIds = dt },
                commandType: CommandType.StoredProcedure
            );
            return idsOfExistingActors.ToList();
        }
    }

    public async Task<List<Actor>> GetAll(PaginationDTO pagination)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var query = "SELECT Id, Name, DateOfBirth, Picture FROM Actors ORDER BY Name
            //              OFFSET ((@page -1) * @recordsPerPage) ROWS FETCH NEXT @recordsPerPage ROWS ONLY;";
            var actors = await connection.QueryAsync<Actor>(
                "Actors_GetAll",
                new { pagination.Page, pagination.RecordsPerPage },
                commandType: CommandType.StoredProcedure
            );

            var actorsCount = await connection.QuerySingleAsync<int>(
                "Actors_Count",
                commandType: CommandType.StoredProcedure
            );

            _httpContext.Response.Headers.Append("totalAmountOfRecords", actorsCount.ToString());

            return actors.ToList();
        }
    }

    public async Task<Actor?> GetById(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var query = "SELECT Id, Name, DateOfBirth, Picture FROM Actors WHERE Id = @Id";
            var actor = await connection.QuerySingleOrDefaultAsync<Actor>(
                "Actors_GetById",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
            return actor;
        }
    }

    public async Task<List<Actor>> GetByName(string name)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var query = "SELECT Id, Name, DateOfBirth, Picture FROM Actors WHERE Name LIKE '%'+@Name+'%'";
            var actors = await connection.QueryAsync<Actor>(
                "Actors_GetByName",
                new { Name = name },
                commandType: CommandType.StoredProcedure
            );
            return actors.ToList();
        }
    }

    public async Task Update(Actor actor)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var query = "UPDATE Actors SET Name = @Name, DateOfBirth = @DateOfBirth, Picture = @Picture WHERE Id = @Id";
            await connection.ExecuteAsync(
                "Actors_Update",
                new
                {
                    actor.Id,
                    actor.Name,
                    actor.DateOfBirth,
                    actor.Picture,
                },
                commandType: CommandType.StoredProcedure
            );
            return;
        }
    }
}
