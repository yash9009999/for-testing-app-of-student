namespace AuthServer.Models;

/// <summary>
/// Represents a user account in the Auth Server.
/// This is separate from the domain User model in the main API.
/// </summary>
public class AuthUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
