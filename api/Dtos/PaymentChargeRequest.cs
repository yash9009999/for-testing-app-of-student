namespace api.Dtos;

/// <summary>
/// SSD: minimal payment boundary — only stable order identity and money fields cross into the payment gateway adapter;
/// never pass full <see cref="OrderDTO"/> (PII / basket surface) to a gateway implementation.
/// </summary>
public sealed record PaymentChargeRequest(
    long OrderId,
    decimal Amount,
    string Currency
);
