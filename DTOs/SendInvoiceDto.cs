using System.ComponentModel.DataAnnotations;

namespace InvoiceTrackerAPI2.DTOs;

public record SendInvoiceDto
{
    [Required]
    [EmailAddress]
    public string ToEmail { get; init; } = string.Empty;
}
