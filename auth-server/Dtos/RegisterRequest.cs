using System.ComponentModel.DataAnnotations;

namespace AuthServer.Dtos;

/// <summary>
/// SSD: inbound-only DTO — do not pass to <c>LogInformation</c> with object expansion (password leakage risk).
/// </summary>
public record RegisterRequest(
    [Required]
    [StringLength(50, MinimumLength = 2)]
    string Username,
    [Required]
    [EmailAddress]
    [StringLength(254, MinimumLength = 5)]
    string Email,
    [Required]
    [StringLength(100, MinimumLength = 1)]
    string FullName,
    [Required]
    [StringLength(128, MinimumLength = 8)]
    string Password
);
