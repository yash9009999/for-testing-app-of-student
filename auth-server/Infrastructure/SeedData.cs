using AuthServer.Models;

namespace AuthServer.Infrastructure;

public static class SeedData
{
    public static void Initialize(AuthDbContext context)
    {
        if (context.Users.Any())
        {
            return; // DB has been seeded
        }

        // SSD: passwords are stored as PBKDF2 hashes (see PasswordHasher) — never persist plaintext.
        var users = new List<AuthUser>
        {
            new AuthUser
            {
                Id = "user-001",
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = PasswordHasher.Hash("password123"),
                CreatedAt = DateTime.UtcNow
            },
            new AuthUser
            {
                Id = "user-002",
                Username = "alice",
                Email = "alice@example.com",
                FullName = "Alice Smith",
                PasswordHash = PasswordHasher.Hash("alice123"),
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Users.AddRange(users);
        context.SaveChanges();
    }
}
