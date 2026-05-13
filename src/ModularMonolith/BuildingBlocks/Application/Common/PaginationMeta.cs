namespace ModularMonolith.BuildingBlocks.Application.Common;

public sealed class PaginationMeta
{
    public int TotalRecords { get; init; }
    public int TotalPages { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
}
