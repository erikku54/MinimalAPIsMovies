using System;

namespace MinimalAPIsMovies.DTOs;

public class PaginationDTO
{
    private const int RecordsPerPageMax = 50;
    private int _recordsPerPage = 10;

    public int Page { get; set; } = 1;
    public int RecordsPerPage
    {
        get => _recordsPerPage;
        set => _recordsPerPage = value > RecordsPerPageMax ? RecordsPerPageMax : value;
    }
}
