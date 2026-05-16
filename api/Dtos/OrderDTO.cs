namespace api.Dtos;

/// <summary>
/// SSD: read model for orders after server-side pricing — do not reuse this type for POST/PUT bodies (see <see cref="OrderCreateRequest"/> / <see cref="OrderUpdateRequest"/>).
/// Sequential <see cref="OrderId"/> remains an IDOR risk for guest carts; authenticated orders return 404 to non-owners (see OrderController).
/// </summary>
public record OrderDTO(
    long OrderId,
    DateTime? OrderTime,
    decimal OrderTotal,
    decimal DeliveryCost,
    int? EstDeliveryMinutes,
    string? Promotion,
    string? MemorableName,
    List<TreatDTO> BasketItems,
    /// <summary>Returned once when a guest cart is created; store client-side and send as X-Guest-Token on later calls.</summary>
    string? GuestAccessToken = null);
