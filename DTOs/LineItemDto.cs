namespace InvoiceTrackerAPI2.DTOs;

public record LineItemDto
{
    public int Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public int Qty { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Total { get; init; }
}
