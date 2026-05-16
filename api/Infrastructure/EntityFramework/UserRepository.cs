using api.Models;
using api.Services.Required;
using Microsoft.EntityFrameworkCore;

namespace api.Infrastructure.EntityFramework;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public User? GetById(long id)
    {
        if (id <= 0)
            return null;
        return _context.Users.Find(id);
    }

    /// <summary>SSD: reject null/whitespace external ids at the repository edge — avoids pointless table scans and inconsistent “not found” behaviour.</summary>
    public User? GetByExternalUserId(string externalUserId)
    {
        if (string.IsNullOrWhiteSpace(externalUserId))
            return null;

        return _context.Users.FirstOrDefault(u => u.ExternalUserId == externalUserId);
    }

    public User Create(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }

    public bool Update(User user)
    {
        _context.Users.Update(user);
        return _context.SaveChanges() > 0;
    }
}
