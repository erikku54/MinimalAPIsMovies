using System;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;

namespace MinimalAPIsMovies.Repositories;

public class MoviesRepository : IMoviesRepository
{
    private readonly string _connectionString;
    private readonly HttpContext _httpContext;

    public MoviesRepository(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _httpContext = httpContextAccessor.HttpContext!;
    }

    public async Task Assign(int id, List<int> genresIds)
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));

        foreach (var genreId in genresIds)
        {
            dt.Rows.Add(genreId);
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.ExecuteAsync("Movies_AssignGenres", new { movieId = id, genresIds = dt }, commandType: CommandType.StoredProcedure);
        }
    }

    public async Task Assign(int id, List<ActorMovie> actors)
    {
        for (int i = 0; i < actors.Count; i++)
        {
            actors[i].Order = i + 1;
        }

        var dt = new DataTable();
        dt.Columns.Add("ActorId", typeof(int));
        dt.Columns.Add("Order", typeof(int));
        dt.Columns.Add("Character", typeof(string));

        foreach (var actorMovie in actors)
        {
            dt.Rows.Add(actorMovie.ActorId, actorMovie.Order, actorMovie.Character);
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.ExecuteAsync("Movies_AssignActors", new { movieId = id, actorsMovies = dt }, commandType: CommandType.StoredProcedure);
        }
    }

    public async Task<int> Create(Movie movie)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var id = await connection.QuerySingleAsync<int>("Movies_Create", new { movie.Title, movie.Poster, movie.InTheaters, movie.ReleaseDate }, commandType: CommandType.StoredProcedure);
            movie.Id = id;
            return id;
        }
    }

    public async Task Delete(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.ExecuteAsync("Movies_Delete", new { id }, commandType: CommandType.StoredProcedure);
            return;
        }
    }

    public async Task<bool> Exists(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var exists = await connection.QuerySingleAsync<bool>("Movies_Exists", new { id }, commandType: CommandType.StoredProcedure);
            return exists;
        }
    }

    public async Task<List<Movie>> GetAll(PaginationDTO paginationDTO)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var movies = await connection.QueryAsync<Movie>("Movies_GetAll", new { paginationDTO.Page, paginationDTO.RecordsPerPage }, commandType: CommandType.StoredProcedure);

            var movieCount = await connection.ExecuteScalarAsync<int>("Movies_Count", commandType: CommandType.StoredProcedure);

            _httpContext.Response.Headers.Append("totalAmountOfRecords", movieCount.ToString());
            return movies.ToList();
        }
    }

    public async Task<Movie> GetById(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            using (var multip = await connection.QueryMultipleAsync("Movies_GetById", new { id }, commandType: CommandType.StoredProcedure))
            {
                var movie = await multip.ReadSingleAsync<Movie>();
                var comments = await multip.ReadAsync<Comment>();
                var genres = await multip.ReadAsync<Genre>();
                var actors = await multip.ReadAsync<ActorMovieDTO>();

                movie.Comments = comments.ToList();
                movie.GenresMovies = genres.Select(x => new GenreMovie { GenreId = x.Id, Genre = x }).ToList();
                movie.ActorsMovies = actors.Select(x => new ActorMovie { ActorId = x.Id, Character = x.Character }).ToList();

                return movie;
            }
        }
    }

    public async Task Update(Movie movie)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.ExecuteAsync("Movies_Update", new { movie.Id, movie.Title, movie.Poster, movie.InTheaters, movie.ReleaseDate }, commandType: CommandType.StoredProcedure);
            return;
        }
    }

}
