namespace AuthServer.Dtos;

/// <summary>
/// SSD: success payloads carry a bearer credential — treat <see cref="Token"/> as confidential (no logging / querystrings).
/// </summary>
/// <param name="Success">Unambiguous outcome for UI clients.</param>
/// <param name="Message">Non-diagnostic message safe for end users.</param>
/// <param name="Token">Bearer access token — never log or persist in cleartext outside protected storage.</param>
/// <param name="UserId">Correlates to the identity store; omit in future revisions if clients can rely solely on token claims.</param>
/// <param name="TokenType">When a token is returned, this is <c>Bearer</c> to prevent misuse as a non-OAuth secret.</param>
public record AuthResponse(
    bool Success,
    string Message,
    string? Token = null,
    string? UserId = null,
    string? TokenType = null
);
