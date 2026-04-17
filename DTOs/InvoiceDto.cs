using InvoiceTrackerAPI2.Models.Enums;

namespace InvoiceTrackerAPI2.DTOs;

public record InvoiceDto
{
    public int Id { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public string ClientName { get; init; } = string.Empty;
    public string ClientEmail { get; init; } = string.Empty;
    public InvoiceStatus Status { get; init; }
    public DateTime IssueDate { get; init; }
    public DateTime DueDate { get; init; }
    public decimal VatRate { get; init; }
    public string? Notes { get; init; }
    public List<LineItemDto> LineItems { get; init; } = [];
    // Computed fields that dont exist on the model
    // calculated by AutoMapper 
    public decimal Subtotal { get; init; }
    public decimal VatAmount { get; init; }
    public decimal Total { get; init; }
}
