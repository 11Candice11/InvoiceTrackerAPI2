namespace InvoiceTrackerAPI2.DTOs;

public record PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
