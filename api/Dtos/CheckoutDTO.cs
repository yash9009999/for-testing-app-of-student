namespace api.Dtos;

/// <summary>
/// SSD: <c>TransactionId</c> is sensitive — do not log or ship to analytics without redaction (enumeration risk).
/// <c>EstimatedDeliveryTime</c> is UTC <see cref="DateTimeOffset"/> to avoid ambiguous <see cref="DateTime"/> kinds.
/// Nested <c>Order</c> matches this API contract; trim fields if clients need a thinner payload.
/// There is no payment amount here by design — clients must not infer correctness from <c>Paid</c> alone.
/// </summary>
public record CheckoutDTO(
    bool Paid,
    string TransactionId,
    OrderDTO Order,
    DateTimeOffset EstimatedDeliveryTime
);
