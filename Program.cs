using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.Endpoints;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Repositories;
using MinimalAPIsMovies.Services;

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

builder.Services.AddTransient<IFileStorage, LocalFileStorage>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.MapGet("/", () => authorName);

app.MapGroup("/genres").MapGenres();
app.MapGroup("/actors").MapActors();

app.Run();

