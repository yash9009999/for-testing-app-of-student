using System.ComponentModel.DataAnnotations;

namespace AuthServer.Dtos;

/// <summary>
/// Request for exchanging authorization code for access token.
/// This is part of the OAuth 2.0 Authorization Code Flow.
/// </summary>
public record TokenRequest(
    [Required]
    [StringLength(512, MinimumLength = 1)]
    string Code,
    [StringLength(2048)]
    string? RedirectUri
);
