using System.ComponentModel.DataAnnotations;
using InvoiceTrackerAPI2.Models.Enums;

namespace InvoiceTrackerAPI2.DTOs;

public record InvoiceFilterDto
{
    [EnumDataType(typeof(InvoiceStatus), ErrorMessage = "Invalid invoice status.")]
    public InvoiceStatus? Status { get; init; }

    [StringLength(200, ErrorMessage = "Client name filter must not exceed 200 characters.")]
    public string? ClientName { get; init; }

    public DateTime? From { get; init; }

    public DateTime? To { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
    public int Page { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; init; } = 20;
}
