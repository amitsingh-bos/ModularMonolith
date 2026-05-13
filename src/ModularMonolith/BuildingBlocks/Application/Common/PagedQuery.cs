namespace ModularMonolith.BuildingBlocks.Application.Common;

public class PagedQuery
{
    private int _pageSize = 20;
    private int _pageNumber = 1;

    public const int MaxPageSize = 100;

    public int PageNumber
    {
        get => _pageNumber;
        init => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    }

    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public string SortDirection { get; init; } = "asc";

    public int Skip => (PageNumber - 1) * PageSize;
}
