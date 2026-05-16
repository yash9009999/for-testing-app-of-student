using System.ComponentModel.DataAnnotations;
using api.Models;

namespace api.Dtos;

/// <summary>
/// SSD: write-only shape for POST /api/order — totals and ids are server-derived; never trust client-supplied money fields.
/// </summary>
public record OrderCreateRequest(
    [MaxLength(64)]
    string? Promotion,
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    List<TreatBasketRequest> BasketItems
);

/// <summary>
/// SSD: write-only shape for PUT /api/order — <see cref="OrderId"/> scopes the mutation; totals remain computed server-side.
/// </summary>
public record OrderUpdateRequest(
    [Range(1, long.MaxValue)]
    long OrderId,
    [MaxLength(64)]
    string? Promotion,
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    List<TreatBasketRequest> BasketItems
);

/// <summary>
/// SSD: basket lines carry only product identity — prices/names are loaded from the database to prevent client-side tampering.
/// </summary>
public record TreatBasketRequest(
    [Required]
    [MinLength(1)]
    [MaxLength(50)]
    List<ProductIdRequest> Products
);

/// <summary>
/// SSD: constrain to positive catalog ids — avoids zero/negative ids and caps JSON payload size via parent list limits in services.
/// </summary>
public record ProductIdRequest(
    [Range(1, long.MaxValue)]
    long ProductId
);
