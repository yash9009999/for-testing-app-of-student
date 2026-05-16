using System.ComponentModel.DataAnnotations;

namespace AuthServer.Dtos;

/// <summary>
/// SSD: inbound-only DTO — do not pass to <c>LogInformation</c> with object expansion (password leakage risk).
/// </summary>
public record RegisterRequest(
    [property: Required]
    [property: StringLength(50, MinimumLength = 2)]
    string Username,
    [property: Required]
    [property: EmailAddress]
    [property: StringLength(254, MinimumLength = 5)]
    string Email,
    [property: Required]
    [property: StringLength(100, MinimumLength = 1)]
    string FullName,
    [property: Required]
    [property: StringLength(128, MinimumLength = 8)]
    string Password
);
