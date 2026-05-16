using api.Models;
using api.Services.Required;
using Microsoft.EntityFrameworkCore;

namespace api.Infrastructure.EntityFramework;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Order? GetById(long id)
    {
        return _context.Orders
            .Include(o => o.User)
            .Include(o => o.Treats)
                .ThenInclude(t => t.Products)
            .FirstOrDefault(o => o.Id == id);
    }

    // FIX 1: SQL Injection — replaced raw string interpolation with LINQ
    public List<Order> FindByMemorableName(string memorableName, int maxResults = 50)
    {
        maxResults = Math.Clamp(maxResults, 1, 200);
        return _context.Orders
            .Include(o => o.User)
            .Include(o => o.Treats)
                .ThenInclude(t => t.Products)
            .Where(o => o.MemorableName == memorableName)
            .Take(maxResults)
            .ToList();
    }

    public Order Add(Order order)
    {
        _context.Orders.Add(order);
        return order;
    }

    // FIX 2: Concurrency — attach RowVersion token and catch DbUpdateConcurrencyException
    public Order Update(Order order)
    {
        var tracked = _context.Orders
            .FirstOrDefault(o => o.Id == order.Id);

        if (tracked == null)
            throw new InvalidOperationException($"Order {order.Id} not found.");

        // Ensure EF checks the RowVersion on UPDATE; throws if another writer changed the row
        _context.Entry(tracked).OriginalValues["Version"] = order.Version;

        _context.Entry(tracked).CurrentValues.SetValues(order);

        try
        {
            _context.SaveChanges();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.Single();
            var dbValues = entry.GetDatabaseValues();

            if (dbValues == null)
                throw new InvalidOperationException("The order was deleted by another user.");

            throw new DbUpdateConcurrencyException(
                "The order was modified by another user. Please reload and retry.", ex);
        }

        return tracked;
    }

    // FIX 2 (continued): Concurrency — same RowVersion guard on Delete
    public void Delete(Order order)
    {
        var tracked = _context.Orders
            .FirstOrDefault(o => o.Id == order.Id);

        if (tracked == null)
            throw new InvalidOperationException($"Order {order.Id} not found.");

        _context.Entry(tracked).OriginalValues["Version"] = order.Version;

        try
        {
            _context.Orders.Remove(tracked);
            _context.SaveChanges();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "The order was modified or deleted by another user. Please reload and retry.");
        }
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}
