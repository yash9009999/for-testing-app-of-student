namespace api.Services.Required;

/// <summary>
/// SSD: explicit validation outcome — avoids silent null returns that hide expiry vs signature failures from logs and metrics.
/// Renamed away from <c>TokenValidationResult</c> to avoid clashing with Microsoft.IdentityModel.Tokens types.
/// </summary>
public enum PrincipalValidationFailureReason
{
    None = 0,
    MalformedOrEmpty,
    SigningKeyUnavailable,
    SignatureInvalid,
    TokenExpired,
    InvalidIssuerOrAudience,
    MissingSubjectClaim,
    Other
}

public readonly record struct PrincipalValidationResult(
    bool Succeeded,
    string? UserId,
    PrincipalValidationFailureReason FailureReason)
{
    public static PrincipalValidationResult Ok(string userId) => new(true, userId, PrincipalValidationFailureReason.None);

    public static PrincipalValidationResult Fail(PrincipalValidationFailureReason reason) => new(false, null, reason);
}
