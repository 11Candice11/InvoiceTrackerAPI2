using System.ComponentModel.DataAnnotations;

namespace InvoiceTrackerAPI2.DTOs;

public record CreateLineItemDto
{
    [Required(ErrorMessage = "Description is required.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 500 characters.")]
    public string Description { get; init; } = string.Empty;

    [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10,000.")]
    public int Qty { get; init; } = 1;

    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0.")]
    public decimal UnitPrice { get; init; }
}
