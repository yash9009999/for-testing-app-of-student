using System.ComponentModel.DataAnnotations;
using api.Models;

namespace api.Dtos;

/// <summary>
/// SSD: write-only shape for POST /api/order — totals and ids are server-derived; never trust client-supplied money fields.
/// </summary>
public record OrderCreateRequest(
    [property: MaxLength(64)]
    string? Promotion,
    [property: Required]
    [property: MinLength(1)]
    [property: MaxLength(100)]
    List<TreatBasketRequest> BasketItems
);

/// <summary>
/// SSD: write-only shape for PUT /api/order — <see cref="OrderId"/> scopes the mutation; totals remain computed server-side.
/// </summary>
public record OrderUpdateRequest(
    [property: Range(1, long.MaxValue)]
    long OrderId,
    [property: MaxLength(64)]
    string? Promotion,
    [property: Required]
    [property: MinLength(1)]
    [property: MaxLength(100)]
    List<TreatBasketRequest> BasketItems
);

/// <summary>
/// SSD: basket lines carry only product identity — prices/names are loaded from the database to prevent client-side tampering.
/// </summary>
public record TreatBasketRequest(
    [property: Required]
    [property: MinLength(1)]
    [property: MaxLength(50)]
    List<ProductIdRequest> Products
);

/// <summary>
/// SSD: constrain to positive catalog ids — avoids zero/negative ids and caps JSON payload size via parent list limits in services.
/// </summary>
public record ProductIdRequest(
    [property: Range(1, long.MaxValue)]
    long ProductId
);
