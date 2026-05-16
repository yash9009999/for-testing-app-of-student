using api.Dtos;

namespace api.Services.Provided;

public interface IProductService
{
    /// <summary>SSD: paginated catalogue — caps page size to reduce accidental over-fetch / scraping of the full menu in one shot.</summary>
    List<ProductDTO> GetProducts(int page = 1, int pageSize = 50);

    /// <summary>Returns null when no product exists for the id (avoid throwing into the HTTP layer).</summary>
    ProductDTO? GetProductById(long id);
}
