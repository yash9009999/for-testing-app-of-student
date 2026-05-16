using System.ComponentModel.DataAnnotations;

namespace api.Dtos;

/// <summary>
/// SSD: payment outcomes — keep <see cref="TransactionId"/> non-guessable (GUID/ULID), cap lengths to reduce log-poisoning, and never forward raw processor diagnostics in <see cref="Message"/>.
/// </summary>
public record PaymentDTO(
    bool Success,
    [property: Required]
    [property: MaxLength(128)]
    string TransactionId,
    [property: MaxLength(256)]
    string Message
);
