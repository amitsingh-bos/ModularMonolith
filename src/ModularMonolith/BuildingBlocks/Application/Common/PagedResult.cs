namespace ModularMonolith.BuildingBlocks.Application.Common;

public sealed class PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, int totalRecords, int pageNumber, int pageSize)
    {
        Items = items;
        TotalRecords = totalRecords;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalRecords / pageSize) : 0;
        HasNextPage = pageNumber < TotalPages;
        HasPreviousPage = pageNumber > 1;
    }

    public IReadOnlyList<T> Items { get; }
    public int TotalRecords { get; }
    public int TotalPages { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public bool HasNextPage { get; }
    public bool HasPreviousPage { get; }

    public PaginationMeta ToPaginationMeta() => new()
    {
        TotalRecords = TotalRecords,
        TotalPages = TotalPages,
        CurrentPage = PageNumber,
        PageSize = PageSize,
        HasNextPage = HasNextPage,
        HasPreviousPage = HasPreviousPage
    };
}
