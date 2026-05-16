namespace api.Dtos;

/// <summary>
/// SSD: transport shape for auth-adjacent responses in this solution — keep success payloads minimal; see remarks.
/// </summary>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="Message">Human-readable, non-diagnostic text for clients.</param>
/// <param name="UserId">Optional stable id — avoid logging; prefer deriving identity from verified token claims when possible.</param>
/// <param name="Token">Bearer secret — never log, never place in query strings, clear on sign-out.</param>
/// <param name="TokenType">When <paramref name="Token"/> is set, use <c>Bearer</c> so handlers do not mis-classify the value.</param>
public record AuthResponse(
    bool Success,
    string Message,
    string? UserId = null,
    string? Token = null,
    string? TokenType = null
);
