using System.ComponentModel.DataAnnotations;

namespace AuthServer.Dtos;

/// <summary>
/// SSD: passwords arrive as strings for JSON binding — C# strings cannot be deterministically zeroed; cap length to reduce bcrypt/PBKDF2-style CPU abuse.
/// Prefer never logging this DTO verbatim (structured logging with explicit fields only).
/// </summary>
public record LoginRequest(
    [property: Required]
    [property: StringLength(50, MinimumLength = 1)]
    string Username,
    [property: Required]
    [property: StringLength(128, MinimumLength = 8)]
    string Password
);
