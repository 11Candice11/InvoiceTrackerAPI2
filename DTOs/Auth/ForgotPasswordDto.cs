using System.ComponentModel.DataAnnotations;

namespace InvoiceTrackerAPI2.DTOs.Auth;

public record ForgotPasswordDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Must be a valid email address.")]
    public string Email { get; init; } = string.Empty;
}
