namespace InvoiceTrackerAPI2.DTOs.Auth;

public record AuthResponseDto
{
    public string Token { get; init; } = string.Empty;
    public UserDto User { get; init; } = null!;
}


