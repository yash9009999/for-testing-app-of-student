namespace api.Models;

public class User
{
    public long Id { get; set; }
    public string ExternalUserId { get; set; } = string.Empty; // Stable id from the identity provider (claim); not exposed in API error text
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string FavouriteFlavour { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Order> Orders { get; set; } = new();

    public bool IsDeleted { get; set; }
}
