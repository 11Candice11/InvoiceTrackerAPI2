using System.ComponentModel.DataAnnotations;

namespace InvoiceTrackerAPI2.DTOs.Auth;

public record ResetPasswordDto
{
    [Required(ErrorMessage = "Reset token is required.")]
    public string Token { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Must be a valid email address.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [MaxLength(100, ErrorMessage = "Password must not exceed 100 characters.")]
    public string NewPassword { get; init; } = string.Empty;
}
