using System.ComponentModel.DataAnnotations;

namespace AuthServer.Dtos;

/// <summary>
/// Request for exchanging authorization code for access token.
/// This is part of the OAuth 2.0 Authorization Code Flow.
/// </summary>
public record TokenRequest(
    [property: Required]
    [property: StringLength(512, MinimumLength = 1)]
    string Code,
    [property: StringLength(2048)]
    string? RedirectUri
);
