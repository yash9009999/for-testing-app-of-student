using AuthServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Controllers;

[ApiController]
[Route(".well-known")]
public class JwksController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public JwksController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpGet("jwks.json")]
    public IActionResult GetJwks()
    {
        var key = _tokenService.GetPublicKey();
        var parameters = key.Rsa!.ExportParameters(false);

        var jwks = new
        {
            keys = new[]
            {
                new
                {
                    kty = "RSA",
                    use = "sig",
                    kid = key.KeyId,
                    n = Base64UrlEncoder.Encode(parameters.Modulus!),
                    e = Base64UrlEncoder.Encode(parameters.Exponent!)
                }
            }
        };

        return Ok(jwks);
    }
}
