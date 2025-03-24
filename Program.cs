using MinimalAPIsMovies.Entities;

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

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("free");
app.UseOutputCache();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => authorName);

app.MapGet("/genres", () =>
{
    var genres = new List<Genre>() {
        new Genre { Id = 1, Name = "Drama" },
        new Genre { Id = 2, Name = "Comedy" },
        new Genre { Id = 3, Name = "Action" },
        new Genre { Id = 4, Name = "Fantasy" },
        new Genre { Id = 5, Name = "Horror" },
        new Genre { Id = 6, Name = "Mystery" },
        new Genre { Id = 7, Name = "Romance" },
        new Genre { Id = 8, Name = "Thriller" },
        new Genre { Id = 9, Name = "Western" }
    };

    return genres;
}).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(15)));

app.Run();

