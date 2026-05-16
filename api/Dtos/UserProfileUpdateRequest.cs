using System.ComponentModel.DataAnnotations;

namespace api.Dtos;

/// <summary>
/// SSD: mass-assignment guard — profile updates cannot change immutable identity fields (internal id, external subject, CreatedAt).
/// </summary>
public sealed record UserProfileUpdateRequest(
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
    string FavouriteFlavour
);
