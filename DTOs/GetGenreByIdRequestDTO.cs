using AutoMapper;
using MinimalAPIsMovies.Repositories;

namespace MinimalAPIsMovies.DTOs;

public class GetGenreByIdRequestDTO
{
    public int Id { get; set; }
    public IGenresRepository Repository { get; set; } = null!;
    public IMapper Mapper { get; set; } = null!;
}
