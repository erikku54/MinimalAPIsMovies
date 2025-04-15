using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Filters;
using MinimalAPIsMovies.Utilities;

namespace MinimalAPIsMovies.Endpoints;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsers(this RouteGroupBuilder group)
    {
        group.MapPost("/", Register).AddEndpointFilter<ValidationFilter<UserCredentialsDTO>>();
        group.MapPost("/login", Login).AddEndpointFilter<ValidationFilter<UserCredentialsDTO>>();

        group.MapPost("/makeadmin", MakeAdmin)
            .AddEndpointFilter<ValidationFilter<EditClaimDTO>>()
            .RequireAuthorization("isadmin");
        group.MapPost("/removeadmin", RemoveAdmin)
            .AddEndpointFilter<ValidationFilter<EditClaimDTO>>()
            .RequireAuthorization("isadmin");

        return group;
    }

    static async Task<Results<Ok<AuthenticationResponseDTO>, BadRequest<IEnumerable<IdentityError>>>> Register(UserCredentialsDTO userCredentialsDTO, [FromServices] UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        var user = new IdentityUser
        {
            UserName = userCredentialsDTO.Email,
            Email = userCredentialsDTO.Email,
        };

        var result = await userManager.CreateAsync(user, userCredentialsDTO.Password);
        if (result.Succeeded)
        {
            var authenticationResponse = await BuildToken(userCredentialsDTO, userManager, configuration);
            return TypedResults.Ok(authenticationResponse);
        }

        return TypedResults.BadRequest(result.Errors);
    }

    static async Task<Results<Ok<AuthenticationResponseDTO>, BadRequest<string>>> Login(UserCredentialsDTO userCredentialsDTO, [FromServices] SignInManager<IdentityUser> signInManager, [FromServices] UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        var user = await userManager.FindByEmailAsync(userCredentialsDTO.Email);
        if (user is null)
        {
            return TypedResults.BadRequest("There was a problem with the email or password");
        }

        var results = await signInManager.CheckPasswordSignInAsync(user, userCredentialsDTO.Password, false);
        if (results.Succeeded)
        {
            var authenticationResponse = await BuildToken(userCredentialsDTO, userManager, configuration);
            return TypedResults.Ok(authenticationResponse);
        }

        return TypedResults.BadRequest("There was a problem with the email or password");
    }

    static async Task<Results<NoContent, NotFound>> MakeAdmin(EditClaimDTO editClaimDTO, [FromServices] UserManager<IdentityUser> userManager)
    {
        var user = await userManager.FindByEmailAsync(editClaimDTO.Email);
        if (user is null) return TypedResults.NotFound();

        await userManager.AddClaimAsync(user, new Claim("isadmin", "true"));

        return TypedResults.NoContent();
    }

    static async Task<Results<NoContent, NotFound>> RemoveAdmin(EditClaimDTO editClaimDTO, [FromServices] UserManager<IdentityUser> userManager)
    {
        var user = await userManager.FindByEmailAsync(editClaimDTO.Email);
        if (user is null) return TypedResults.NotFound();

        await userManager.RemoveClaimAsync(user, new Claim("isadmin", "true"));

        return TypedResults.NoContent();
    }

    private async static Task<AuthenticationResponseDTO> BuildToken(UserCredentialsDTO userCredentialsDTO, UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        // 建立一個 claims 清單，這些資訊會寫入 JWT Token 中
        var claims = new List<Claim>
        {
            new Claim("email", userCredentialsDTO.Email),
            new Claim("Whatever I want", "this is a value")
        };

        var user = await userManager.FindByEmailAsync(userCredentialsDTO.Email);
        var claimsFromDb = await userManager.GetClaimsAsync(user!);

        claims.AddRange(claimsFromDb);

        var key = KeysHandler.GetKeys(configuration).First(); // 取得簽章金鑰
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // 用 HmacSha256 產生簽章憑證

        var expiration = DateTime.UtcNow.AddMinutes(30);

        var securityToken = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        // 產出實際的 JWT Token 字串
        var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

        return new AuthenticationResponseDTO
        {
            Token = token,
            Expiration = expiration,
        };
    }
}
