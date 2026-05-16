using System.ComponentModel.DataAnnotations;
using api.Models;

namespace api.Dtos;

/// <summary>
/// SSD: catalog response — <see cref="Type"/> is a closed enum (not free text) so clients cannot smuggle unknown category strings;
/// prices are sourced from persistence when building this DTO, not from untrusted write payloads.
/// </summary>
public record ProductDTO(
    long ProductId,
    [property: MaxLength(200)] string ProductName,
    decimal Price,
    [property: MaxLength(2000)] string Description,
    IReadOnlyList<string> Ingredients,
    ProductType Type
);
