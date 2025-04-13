using System.Data;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace MinimalAPIsMovies.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly string _connectionString;

    public UsersRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<IdentityUser?> GetByEmail(string normalizedEmail)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var sql = "SELECT * FROM Users WHERE NormalizedEmail = @normalizedEmail;";
            return await connection.QuerySingleOrDefaultAsync<IdentityUser>("GetByEmail", new { normalizedEmail }, commandType: CommandType.StoredProcedure);
        }
    }

    public async Task<string> Create(IdentityUser user)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            user.Id = Guid.NewGuid().ToString();
            // var sql = "INSERT INTO Users(id, email, NormalizedEmail, UserName, NormalizedUserName, PasswordHash) VALUES (@Id, @email, @normalizedEmail, @userName, @normalizedUserName, @passwordHash);";
            await connection.ExecuteAsync("Users_Create",
                new
                {
                    user.Id,
                    user.Email,
                    user.NormalizedEmail,
                    user.UserName,
                    user.NormalizedUserName,
                    user.PasswordHash
                },
                commandType: CommandType.StoredProcedure);
            return user.Id;
        }
    }


}
