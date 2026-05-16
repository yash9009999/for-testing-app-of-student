using api.Dtos;
using api.Models;
using api.Services.Required;
using api.Services.Provided;

namespace api.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public ProductDTO? GetProductById(long id)
    {
        if (id <= 0)
            return null;

        var product = _productRepository.GetById(id);
        return product == null ? null : MapToDto(product);
    }

    public List<ProductDTO> GetProducts(int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var skip = (page - 1) * pageSize;
        var products = _productRepository.GetAll(skip, pageSize);
        return products.Select(MapToDto).ToList();
    }

    private static ProductDTO MapToDto(Product product)
    {
        return new ProductDTO(
            product.Id,
            product.Name,
            product.Price,
            product.Description ?? string.Empty,
            product.Ingredients,
            product.ProductType
        );
    }
}
