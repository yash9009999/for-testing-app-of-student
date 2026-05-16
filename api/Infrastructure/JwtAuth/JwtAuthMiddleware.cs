using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using api.Services.Required;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace api.Infrastructure.JwtAuth;

/// <summary>
/// SSD: validates an optional bearer credential when present and, if valid, stores a stable caller identifier on the context.
/// </summary>
public class JwtAuthMiddleware
{
    private readonly RequestDelegate _next;

    public JwtAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthenticationService authService)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        var token = authHeader?.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(token))
        {
            var result = await authService.ValidateTokenAsync(token);
            if (result.Succeeded && !string.IsNullOrWhiteSpace(result.UserId))
                context.Items["UserId"] = result.UserId;
        }

        await _next(context);
    }
}

/// <summary>
/// SSD: validates bearer credentials using issuer JWKS — maps failures to <see cref="PrincipalValidationFailureReason"/> for structured logging (no token material in logs).
/// </summary>
public class JwtAuthenticationService : IAuthenticationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtAuthenticationService> _logger;

    private static RsaSecurityKey? _cachedKey;
    private static DateTime _cacheExpiryUtc = DateTime.MinValue;

    public JwtAuthenticationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<JwtAuthenticationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PrincipalValidationResult> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogDebug("Token validation failed: {Reason}", PrincipalValidationFailureReason.MalformedOrEmpty);
            return PrincipalValidationResult.Fail(PrincipalValidationFailureReason.MalformedOrEmpty);
        }

        var signingKey = await GetPublicKeyAsync();
        if (signingKey == null)
        {
            _logger.LogWarning("Token validation failed: {Reason}", PrincipalValidationFailureReason.SigningKeyUnavailable);
            return PrincipalValidationResult.Fail(PrincipalValidationFailureReason.SigningKeyUnavailable);
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Auth:Issuer"] ?? "OkraAuthServer",
                ValidateAudience = true,
                ValidAudience = _configuration["Auth:Audience"] ?? "Scoops2GoAPI",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            }, out _);

            var sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(sub))
            {
                _logger.LogInformation("Token validation failed: {Reason}", PrincipalValidationFailureReason.MissingSubjectClaim);
                return PrincipalValidationResult.Fail(PrincipalValidationFailureReason.MissingSubjectClaim);
            }

            return PrincipalValidationResult.Ok(sub);
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogInformation("Token validation failed: {Reason}", PrincipalValidationFailureReason.TokenExpired);
            return PrincipalValidationResult.Fail(PrincipalValidationFailureReason.TokenExpired);
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            _logger.LogInformation("Token validation failed: {Reason}", PrincipalValidationFailureReason.SignatureInvalid);
            return PrincipalValidationResult.Fail(PrincipalValidationFailureReason.SignatureInvalid);
        }
        catch (SecurityTokenInvalidAudienceException)
        {
            _logger.LogInformation("Token validation failed: {Reason}", PrincipalValidationFailureReason.InvalidIssuerOrAudience);
            return PrincipalValidationResult.Fail(PrincipalValidationFailureReason.InvalidIssuerOrAudience);
        }
        catch (SecurityTokenInvalidIssuerException)
        {
            _logger.LogInformation("Token validation failed: {Reason}", PrincipalValidationFailureReason.InvalidIssuerOrAudience);
            return PrincipalValidationResult.Fail(PrincipalValidationFailureReason.InvalidIssuerOrAudience);
        }
        catch (SecurityTokenMalformedException)
        {
            _logger.LogDebug("Token validation failed: {Reason}", PrincipalValidationFailureReason.MalformedOrEmpty);
            return PrincipalValidationResult.Fail(PrincipalValidationFailureReason.MalformedOrEmpty);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Token validation failed: {Reason}", PrincipalValidationFailureReason.Other);
            return PrincipalValidationResult.Fail(PrincipalValidationFailureReason.Other);
        }
    }

    private async Task<RsaSecurityKey?> GetPublicKeyAsync()
    {
        if (_cachedKey != null && DateTime.UtcNow < _cacheExpiryUtc)
            return _cachedKey;

        try
        {
            var client = _httpClientFactory.CreateClient("IdentityIssuerJwks");
            var json = await client.GetStringAsync("/.well-known/jwks.json");
            var jwks = JsonSerializer.Deserialize<JwksResponse>(json);
            var key = jwks?.keys?.FirstOrDefault();
            if (key == null || string.IsNullOrWhiteSpace(key.n) || string.IsNullOrWhiteSpace(key.e))
                return null;

            var rsa = RSA.Create();
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = Base64UrlEncoder.DecodeBytes(key.n),
                Exponent = Base64UrlEncoder.DecodeBytes(key.e)
            });

            _cachedKey = new RsaSecurityKey(rsa) { KeyId = key.kid };
            _cacheExpiryUtc = DateTime.UtcNow.AddHours(1);
            return _cachedKey;
        }
        catch
        {
            return null;
        }
    }

    private class JwksResponse
    {
        public List<JwkKey>? keys { get; set; }
    }

    private class JwkKey
    {
        public string? kty { get; set; }
        public string? use { get; set; }
        public string? kid { get; set; }
        public string? n { get; set; }
        public string? e { get; set; }
    }
}
