namespace api.Services.Required;

/// <summary>SSD: token validation returns structured outcomes — see <see cref="PrincipalValidationResult"/>.</summary>
public interface IAuthenticationService
{
    Task<PrincipalValidationResult> ValidateTokenAsync(string token);
}
