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
}
