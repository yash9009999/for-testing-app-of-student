using AuthServer.Infrastructure;
using AuthServer.Models;

namespace AuthServer.Services;

public class UserService : IUserService
{
    private readonly AuthDbContext _context;

    public UserService(AuthDbContext context)
    {
        _context = context;
    }

    public AuthUser? GetByUsername(string username)
    {
        return _context.Users.FirstOrDefault(u => u.Username == username);
    }

    /// <summary>SSD: used for duplicate checks at registration — never log raw email values.</summary>
    public AuthUser? GetByEmail(string email)
    {
        return _context.Users.FirstOrDefault(u => u.Email == email);
    }

    public AuthUser? GetById(string id)
    {
        return _context.Users.Find(id);
    }

    public AuthUser Create(string username, string email, string fullName, string password)
    {
        var user = new AuthUser
        {
            Id = Guid.NewGuid().ToString(),
            Username = username,
            Email = email,
            FullName = fullName,
            PasswordHash = PasswordHasher.Hash(password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return user;
    }

    public bool VerifyPassword(AuthUser user, string password)
    {
        return PasswordHasher.Verify(password, user.PasswordHash);
    }
}
