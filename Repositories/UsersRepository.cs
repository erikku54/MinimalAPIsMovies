using System.Data;
using System.Security.Claims;
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

    public async Task<IList<Claim>> GetClaims(IdentityUser user)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // var sql = "SELECT ClaimType as [Type], ClaimValue as [Value] ROM UsersClaims WHERE UserId = @id;";
            var claims = await connection.QueryAsync<Claim>("Users_GetClaims", new { user.Id }, commandType: CommandType.StoredProcedure);

            return claims.ToList();
        }
    }

    public async Task AssignClaims(IdentityUser user, IEnumerable<Claim> claims)
    {
        var sql = @"INSERT INTO UserClaims(UserId, ClaimType, ClaimValue) VALUES (@Id, @Type, @Value);";

        var parameters = claims.Select(c => new { user.Id, Type = c.Type, Value = c.Value });

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
    }

    public async Task RemoveClaims(IdentityUser user, IEnumerable<Claim> claims)
    {
        var sql = @"DELETE FROM UserClaims WHERE UserId = @Id AND ClaimType = @Type;";

        var parameters = claims.Select(c => new { user.Id, c.Type });

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.ExecuteAsync(sql, parameters);
        }
    }
}
