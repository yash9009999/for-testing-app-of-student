using api.Attributes;
using api.Dtos;
using api.Services.Provided;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

/// <summary>
/// SSD: BOLA on reads — callers may only load their own profile; errors never echo <see cref="System.Exception.Message"/>.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private const string NotFoundMessage = "The requested resource was not found.";

    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    [RequireAuth]
    public ActionResult<UserDTO> GetUserById(string id)
    {
        var currentUserId = (string?)HttpContext.Items["UserId"];

        if (string.IsNullOrEmpty(currentUserId) || !string.Equals(currentUserId, id, StringComparison.Ordinal))
            return NotFound(new { message = NotFoundMessage });

        try
        {
            var user = _userService.GetUserById(id);
            return Ok(user);
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = NotFoundMessage });
        }
    }

    [HttpPut("{id}")]
    [RequireAuth]
    public ActionResult<UserDTO> UpdateUser(string id, [FromBody] UserProfileUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var currentUserId = (string?)HttpContext.Items["UserId"];

            if (string.IsNullOrEmpty(currentUserId) || !string.Equals(currentUserId, id, StringComparison.Ordinal))
                return Forbid();

            var updatedUser = _userService.UpdateUser(id, request);
            return Ok(updatedUser);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = NotFoundMessage });
        }
        catch (InvalidOperationException)
        {
            return Conflict(new { message = "The request could not be completed." });
        }
    }
}
