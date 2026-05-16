using api.Models;
using api.Services.Required;
using Microsoft.EntityFrameworkCore;

namespace api.Infrastructure.EntityFramework;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>SSD: negative / zero ids are invalid keys — reject before touching the database.</summary>
    public Product? GetById(long id)
    {
        if (id <= 0)
            return null;
        return _context.Products.Find(id);
    }

    public List<Product> GetAll(int skip, int take)
    {
        return _context.Products
            .OrderBy(p => p.Id)
            .Skip(skip)
            .Take(take)
            .ToList();
    }
}
