using AuthServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Infrastructure;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// SSD: identity aggregate root — keep mutations behind <see cref="AuthServer.Services.IUserService"/> rather than exposing raw sets to controllers.
    /// </summary>
    public DbSet<AuthUser> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuthUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}
