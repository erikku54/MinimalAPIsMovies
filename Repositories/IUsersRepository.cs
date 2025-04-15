using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace MinimalAPIsMovies.Repositories;

public interface IUsersRepository
{
    Task AssignClaims(IdentityUser user, IEnumerable<Claim> claims);
    Task<string> Create(IdentityUser user);
    Task<IdentityUser?> GetByEmail(string normalizedEmail);
    Task<IList<Claim>> GetClaims(IdentityUser user);
    Task RemoveClaims(IdentityUser user, IEnumerable<Claim> claims);
}