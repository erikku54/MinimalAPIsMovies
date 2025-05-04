using System;
using MinimalAPIsMovies.Utilities;

namespace MinimalAPIsMovies.DTOs;

public class PaginationDTO
{
    private const int PageDefault = 1;
    private const int RecordsPerPageDefault = 10;
    private const int RecordsPerPageMax = 50;

    private int _recordsPerPage = 10;

    public int Page { get; set; } = 1;
    public int RecordsPerPage
    {
        get => _recordsPerPage;
        set => _recordsPerPage = value > RecordsPerPageMax ? RecordsPerPageMax : value;
    }

    // 這裡必須使用BindAsync作模型繫結，而不能使用anotation
    // 因為預設的[FromQuery]會將查詢字串中的值轉換成字串，但我們需要的是int型別的值
    // 中間還有TryParse的過程
    public static ValueTask<PaginationDTO> BindAsync(HttpContext context)
    {
        var page = context.ExtractValueOrDefault(nameof(Page), PageDefault);
        var recordsPerPage = context.ExtractValueOrDefault(
            nameof(RecordsPerPage),
            RecordsPerPageDefault
        );

        var pagination = new PaginationDTO { Page = page, RecordsPerPage = recordsPerPage };

        return ValueTask.FromResult(pagination);
    }
}
