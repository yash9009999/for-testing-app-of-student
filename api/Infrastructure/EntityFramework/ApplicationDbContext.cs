using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Infrastructure.EntityFramework;

/// <summary>
/// SSD: <see cref="DbSet{TEntity}"/> accessors are <c>internal</c> so only infrastructure in this assembly performs bulk reads;
/// global query filters add a baseline isolation hook for soft-deleted rows (defense in depth — callers must still enforce auth).
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Restricted to internal/protected access to prevent unrestricted external querying
    internal DbSet<Product> Products { get; set; }
    internal DbSet<Order> Orders { get; set; }
    internal DbSet<Treat> Treats { get; set; }
    internal DbSet<User> Users { get; set; }

    // Scoped accessor methods enforce intentional, explicit data access
    internal IQueryable<Product> GetProducts() => Products.AsNoTracking();
    internal IQueryable<Order> GetOrders() => Orders.AsNoTracking();
    internal IQueryable<Treat> GetTreats() => Treats.AsNoTracking();

    // Users are particularly sensitive — no direct bulk read exposed
    internal IQueryable<User> GetUserById(int userId) =>
        Users.AsNoTracking().Where(u => u.Id == userId);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filters enforce soft data isolation at the persistence layer
        modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Treat>().HasQueryFilter(t => !t.IsDeleted);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new TreatConfiguration());
    }
} 


