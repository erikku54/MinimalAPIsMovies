using MinimalAPIsMovies.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

string authorName = builder.Configuration.GetValue<string>("AuthorName")!;

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
});



// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast =  Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast");

app.Run();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
