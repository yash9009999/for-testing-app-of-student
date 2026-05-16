using AuthServer.Dtos;
using AuthServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Controllers;

/// <summary>
/// Identity endpoints for Scoops2Go. SSD: public summaries avoid disclosing internal token technology; see code docs on DTOs.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RegistrationConflictMessage = "Unable to register with the provided details.";

    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public AuthController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    /// <summary>Direct login for trusted first-party clients (returns bearer credential on success).</summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResponse(false, "Invalid request."));

        var user = _userService.GetByUsername(request.Username);
        if (user == null || !_userService.VerifyPassword(user, request.Password))
        {
            // SSD: identical failure text for unknown user vs bad password — reduces username enumeration.
            return Unauthorized(new AuthResponse(false, "Invalid username or password."));
        }

        var token = _tokenService.GenerateJwtToken(user.Id, user.Username, user.Email);

        // SSD: TokenType disambiguates bearer material for API consumers; never log Token or UserId.
        return Ok(new AuthResponse(true, "Login successful.", token, user.Id, "Bearer"));
    }

    /// <summary>Registers a user and returns a bearer credential on success.</summary>
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResponse(false, "Invalid request."));

        // SSD: generic conflict message for username OR email collisions — avoids harvesting valid addresses.
        if (_userService.GetByUsername(request.Username) != null || _userService.GetByEmail(request.Email) != null)
        {
            return Conflict(new AuthResponse(false, RegistrationConflictMessage));
        }

        var user = _userService.Create(request.Username, request.Email, request.FullName, request.Password);
        var token = _tokenService.GenerateJwtToken(user.Id, user.Username, user.Email);

        return Ok(new AuthResponse(true, "Registration successful.", token, user.Id, "Bearer"));
    }

    /// <summary>
    /// OAuth 2.0 Authorization endpoint.
    /// Authenticates user and returns authorization code.
    /// </summary>
    [HttpPost("authorize")]
    public IActionResult Authorize([FromBody] LoginRequest request, [FromQuery] string? redirect_uri)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResponse(false, "Invalid request."));

        var user = _userService.GetByUsername(request.Username);
        if (user == null || !_userService.VerifyPassword(user, request.Password))
        {
            return Unauthorized(new AuthResponse(false, "Invalid username or password."));
        }

        // Generate authorization code
        var code = _tokenService.GenerateAuthorizationCode(user.Id);

        // In a real OAuth flow, redirect to redirect_uri with code
        // For now, return the code in the response
        return Ok(new { code, redirect_uri });
    }

    /// <summary>
    /// OAuth 2.0 Token endpoint.
    /// Exchanges authorization code for access token.
    /// </summary>
    [HttpPost("token")]
    public IActionResult Token([FromBody] TokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResponse(false, "Invalid request."));

        var userId = _tokenService.ValidateAuthorizationCode(request.Code);
        if (userId == null)
        {
            return Unauthorized(new AuthResponse(false, "Invalid or expired authorization code."));
        }

        var user = _userService.GetById(userId);
        if (user == null)
        {
            return NotFound(new AuthResponse(false, "User not found."));
        }

        var token = _tokenService.GenerateJwtToken(user.Id, user.Username, user.Email);

        return Ok(new AuthResponse(true, "Token generated successfully.", token, user.Id, "Bearer"));
    }
}
