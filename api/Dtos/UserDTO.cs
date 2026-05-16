using System.ComponentModel.DataAnnotations;

namespace api.Dtos;

/// <summary>
/// SSD: profile projection — annotate inbound updates so oversized strings are rejected before they hit EF.
/// </summary>
public record UserDTO(
    long Id,
    [property: Required]
    [property: StringLength(50, MinimumLength = 1)]
    string Username,
    [property: Required]
    [property: EmailAddress]
    [property: StringLength(254, MinimumLength = 5)]
    string Email,
    [property: Required]
    [property: StringLength(100, MinimumLength = 1)]
    string FullName,
    [property: StringLength(200)]
    string FavouriteFlavour,
    DateTime CreatedAt
);
