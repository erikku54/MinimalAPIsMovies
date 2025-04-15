using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using MinimalAPIsMovies.Entities;

namespace MinimalAPIsMovies.Repositories;

public class CommentsRepository : ICommentsRepository
{
    private readonly string _connectionString;

    public CommentsRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<int> Create(Comment comment)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var query = "INSERT INTO Comments(Body, MovieId) VALUES (@body, @movieId);"
            var id = await connection.ExecuteAsync("Comments_Create", new { comment.Body, comment.MovieId, comment.UserId }, commandType: CommandType.StoredProcedure);

            comment.Id = id;
            return id;
        }
    }

    public async Task Delete(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.ExecuteAsync("Comments_Delete", new { Id = id },
                commandType: CommandType.StoredProcedure);
        }
    }

    public async Task<bool> Exists(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var exists = await connection.QuerySingleAsync<bool>("Comments_Exists", new { Id = id },
                commandType: CommandType.StoredProcedure);

            return exists;
        }
    }

    public async Task<List<Comment>> GetAll(int movieId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var comments = await connection.QueryAsync<Comment>("Comments_GetAllByMovieId", new { MovieId = movieId },
                commandType: CommandType.StoredProcedure);

            return comments.ToList();
        }
    }

    public async Task<Comment?> GetById(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var comment = await connection.QuerySingleOrDefaultAsync<Comment>("Comments_GetById", new { Id = id },
                commandType: CommandType.StoredProcedure);

            return comment;
        }
    }

    public async Task Update(Comment comment)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.ExecuteAsync("Comments_Update", new
            {
                comment.Id,
                comment.Body,
                comment.MovieId
            },
                commandType: CommandType.StoredProcedure);
        }
    }

}
