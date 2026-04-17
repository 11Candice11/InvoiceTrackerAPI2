using System.ComponentModel.DataAnnotations;
using InvoiceTrackerAPI2.Models.Enums;

namespace InvoiceTrackerAPI2.DTOs;
// all fields are nullable (only send what you want 2 update)
// partial update pattern
// TODO controller maps through CreateInvoiceDto - fix this to use UpdateInvoiceDto

public record UpdateInvoiceDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Client name must be between 2 and 200 characters.")]
    public string? ClientName { get; init; }

    [EmailAddress(ErrorMessage = "Client email must be a valid email address.")]
    [StringLength(200, ErrorMessage = "Client email must not exceed 200 characters.")]
    public string? ClientEmail { get; init; }

    public DateTime? IssueDate { get; init; }

    public DateTime? DueDate { get; init; }

    [EnumDataType(typeof(InvoiceStatus), ErrorMessage = "Invalid invoice status.")]
    public InvoiceStatus? Status { get; init; }

    [StringLength(1000, ErrorMessage = "Notes must not exceed 1000 characters.")]
    public string? Notes { get; init; }

    [MinLength(1, ErrorMessage = "At least one line item is required.")]
    public List<CreateLineItemDto>? LineItems { get; init; }
}
