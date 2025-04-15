using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.IdentityModel.Tokens;
using MinimalAPIsMovies.Endpoints;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Repositories;
using MinimalAPIsMovies.Services;
using MinimalAPIsMovies.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string authorName = builder.Configuration.GetValue<string>("AuthorName")!;


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(corsPolicyBuilder =>
    {
        corsPolicyBuilder.WithOrigins(builder.Configuration["AllowedOrigins"]!.Split(","))
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
    options.AddPolicy("free", corsPo =>
    {
        corsPo.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddOutputCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IGenresRepository, GenresRepository>();
builder.Services.AddScoped<IActorsRepository, ActorsRepository>();
builder.Services.AddScoped<IMoviesRepository, MoviesRepository>();
builder.Services.AddScoped<ICommentsRepository, CommentsRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IErrorsRepository, ErrorsRepository>();

builder.Services.AddTransient<IFileStorage, LocalFileStorage>();
builder.Services.AddTransient<IUsersService, UsersService>();


// Identity 服務相關
builder.Services.AddTransient<IUserStore<IdentityUser>, UserStore>();
builder.Services.AddTransient<SignInManager<IdentityUser>>();
builder.Services.AddIdentityCore<IdentityUser>();

// 啟用 API 端點探索:負責收集端點的元數據 (確保包括 Minimal API的所有端點都能被正確探索)
builder.Services.AddEndpointsApiExplorer();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// 啟用 OpenAPI/Swagger 支援: 依賴元數據探索的結果來生成 API 文件
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        // 保留 JWT Claims 的原始名稱，而不會自動映射為.NET ClaimTypes
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 這些參數是用來驗證 JWT 的有效性
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKeys = KeysHandler.GetAllKeys(builder.Configuration),
            // IssuerSigningKey = KeysHandler.GetKeys(builder.Configuration).First(),
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseExceptionHandler(ExceptionHandlerApp => ExceptionHandlerApp.Run(async context =>
{
    // 將錯誤訊息儲存在資料庫中
    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
    var exception = exceptionHandlerFeature?.Error;
    if (exceptionHandlerFeature is null) return;

    var error = new Error()
    {
        ErrorMessage = exception?.Message ?? "Unknown error",
        StackTrace = exception?.StackTrace,
        Date = DateTime.UtcNow,
    };

    // 自訂給使用者的錯誤訊息
    await Results.BadRequest(new
    {
        Type = "error",
        Message = "an expected exception has occurred",
        StatusCode = 500,
    }).ExecuteAsync(context);
}));
app.UseStatusCodePages();

app.UseCors("free");
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseOutputCache();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => authorName);

app.MapGroup("/genres").MapGenres();
app.MapGroup("/actors").MapActors();
app.MapGroup("/movies").MapMovies();
app.MapGroup("/movies/{movieId:int}/comments").MapComments();
app.MapGroup("/users").MapUsers();

app.Run();

