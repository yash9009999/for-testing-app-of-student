using api.Models;

namespace api.Services.Required;

public interface IProductRepository
{
    /// <summary>SSD: invalid keys (≤0) return null at the repository — callers should treat as not found.</summary>
    Product? GetById(long id);
    List<Product> GetAll(int skip, int take);
}
