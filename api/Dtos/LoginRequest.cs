using System.ComponentModel.DataAnnotations;

namespace api.Dtos;

/// <summary>
/// SSD: reserved for future API-local auth flows — passwords are plain strings at the HTTP boundary (JSON cannot bind <see cref="System.Security.SecureString"/>);
/// cap length to mitigate CPU-exhaustion attacks against password hashing endpoints.
/// </summary>
public record LoginRequest(
    [property: Required]
    [property: StringLength(50, MinimumLength = 1)]
    string Username,
    [property: Required]
    [property: StringLength(128, MinimumLength = 8)]
    string Password
);
