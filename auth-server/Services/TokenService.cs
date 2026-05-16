using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private static readonly RSA Rsa;
    private static readonly RsaSecurityKey SigningKey;
    
    static TokenService()
    {
        var keyPath = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "signing-key.xml"
        );
        Rsa = TokenKeyStore.LoadOrCreate(keyPath);
        SigningKey = new RsaSecurityKey(Rsa)
        {
            KeyId = "okra-auth-key"
        };
    }
    
    // In-memory storage for authorization codes (in production, use Redis or database)
    private static readonly Dictionary<string, AuthCodeData> AuthorizationCodes = new();

    private class AuthCodeData
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJwtToken(string userId, string username, string email)
    {
        var credentials = new SigningCredentials(SigningKey, SecurityAlgorithms.RsaSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["Jwt:ExpirationMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RsaSecurityKey GetPublicKey()
    {
        var publicParams = Rsa.ExportParameters(false);
        var rsa = RSA.Create();
        rsa.ImportParameters(publicParams);
        return new RsaSecurityKey(rsa)
        {
            KeyId = SigningKey.KeyId
        };
    }

    public string GenerateAuthorizationCode(string userId)
    {
        var code = Guid.NewGuid().ToString("N");
        
        AuthorizationCodes[code] = new AuthCodeData
        {
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5) // Codes expire in 5 minutes
        };

        // Clean up expired codes
        var expiredCodes = AuthorizationCodes
            .Where(kvp => kvp.Value.ExpiresAt < DateTime.UtcNow)
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var expiredCode in expiredCodes)
        {
            AuthorizationCodes.Remove(expiredCode);
        }

        return code;
    }

    public string? ValidateAuthorizationCode(string code)
    {
        if (!AuthorizationCodes.TryGetValue(code, out var data))
        {
            return null;
        }

        if (data.ExpiresAt < DateTime.UtcNow)
        {
            AuthorizationCodes.Remove(code);
            return null;
        }

        // Remove code after use (one-time use)
        AuthorizationCodes.Remove(code);

        return data.UserId;
    }
}
