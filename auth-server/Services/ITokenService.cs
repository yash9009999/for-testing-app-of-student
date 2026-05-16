namespace AuthServer.Services;

public interface ITokenService
{
    string GenerateJwtToken(string userId, string username, string email);
    string GenerateAuthorizationCode(string userId);
    string? ValidateAuthorizationCode(string code);
    Microsoft.IdentityModel.Tokens.RsaSecurityKey GetPublicKey();
}
