using api.Dtos;
using api.Services.Provided;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

/// <summary>
/// SSD: <see cref="T:api.Infrastructure.Http.ProductCatalogHttpMethodMiddleware"/> enforces GET-only. Catalogue is intentionally
/// <see cref="AllowAnonymous"/> so the SPA can load the menu without logging in (documented trade-off vs API1:
/// add auth + app login gate, or partner API keys / edge rate limits, for higher assurance).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class ProductController : ControllerBase
{
    private const string NotFoundMessage = "The requested resource was not found.";

    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public ActionResult<List<ProductDTO>> GetAllProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Invalid pagination parameters." });

        var products = _productService.GetProducts(page, pageSize);
        if (products.Count == 0)
            return NotFound(new { message = NotFoundMessage });

        return Ok(products);
    }

    [HttpGet("{id}")]
    public ActionResult<ProductDTO> GetProductById(long id)
    {
        var product = _productService.GetProductById(id);
        if (product == null)
            return NotFound(new { message = NotFoundMessage });

        return Ok(product);
    }
}
