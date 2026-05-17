using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InvoiceTrackerAPI2.Data;
using InvoiceTrackerAPI2.DTOs.Auth;
using InvoiceTrackerAPI2.Models;
using InvoiceTrackerAPI2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace InvoiceTrackerAPI2.Services;

// bcrypt for password hashing
// yeah it's slow (like 300ms per hash) but makes brute force attacks expensive

public class AuthService(AppDbContext db, IConfiguration config) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("An account with that email already exists.");

        var user = new User
        {
            Name         = dto.Name,
            Email        = dto.Email,
            // default cost = 11
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return BuildResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

// used for login - re hashes input and then compares so never decrypting
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return BuildResponse(user);
    }

// returns even if email doesn't exist
// prevents user enumeration
    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null) return;

        var plaintext = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        user.PasswordResetToken       = HashToken(plaintext);
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await db.SaveChangesAsync();

        // TODO: send email with reset link containing the plaintext token
        // e.g. https://yourapp.com/reset-password?token={plaintext}&email={user.Email}
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var tokenHash = HashToken(dto.Token);

        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.Email == dto.Email &&
            u.PasswordResetToken == tokenHash &&
            u.PasswordResetTokenExpiry > DateTime.UtcNow)
            ?? throw new InvalidOperationException("Invalid or expired reset token.");

        user.PasswordHash             = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.PasswordResetToken       = null;
        user.PasswordResetTokenExpiry = null;
        await db.SaveChangesAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    // store SHA-256 hash of the token — plaintext is sent to user's email only
    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

// GenerateJwt creates token with:
//  - user id
// - email
// - name
// - jwt id (jti - unique token ID. useful for token revocation)


    private AuthResponseDto BuildResponse(User user) => new()
    {
        Token = GenerateJwt(user),
        User  = new UserDto { Id = user.Id, Name = user.Name, Email = user.Email }
    };

    private string GenerateJwt(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name",                        user.Name),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             config["Jwt:Issuer"],
            audience:           config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
