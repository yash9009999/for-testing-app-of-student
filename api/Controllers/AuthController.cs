using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

/// <summary>
/// SSD / secure systems fixes applied here: (1) health-style responses stay generic — no hints about
/// separate identity components or token technology; (2) disallowed HTTP verbs on <c>/api/auth/status</c>
/// return 405 with an <c>Allow</c> header — OPTIONS is not listed so CORS middleware can answer preflight first.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult GetAuthStatus()
    {
        return Ok(new { message = "Service is operational." });
    }

    /// <summary>
    /// SSD fix: explicit 405 for verbs other than GET on this path (POST, DELETE, PATCH, etc.).
    /// OPTIONS is intentionally omitted so the CORS pipeline can answer preflight before routing.
    /// </summary>
    [Route("status")]
    [AcceptVerbs("POST", "PUT", "DELETE", "PATCH", "HEAD", "TRACE")]
    public IActionResult RejectDisallowedMethodsOnStatus()
    {
        Response.Headers.Allow = "GET";
        return StatusCode(StatusCodes.Status405MethodNotAllowed);
    }
}
