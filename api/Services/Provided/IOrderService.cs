using api.Dtos;

namespace api.Services.Provided;

/// <summary>
/// SSD: every operation that depends on ownership takes an explicit <c>callerExternalUserId</c> (JWT subject) so services can enforce BOLA even if a controller regresses.
/// Guest carts additionally require <paramref name="callerGuestAccessToken"/> matching the secret issued at create time.
/// </summary>
public interface IOrderService
{
    /// <summary>Returns null when the order is missing or the caller is not allowed to view it.</summary>
    OrderDTO? GetOrderById(long id, string? callerExternalUserId, string? callerGuestAccessToken);

    /// <summary>Results filtered by caller: anonymous callers only see paid guest orders; authenticated callers only their own.</summary>
    List<OrderDTO> FindOrder(string memorableName, string? callerExternalUserId, string? callerGuestAccessToken);

    OrderDTO CreateOrder(OrderCreateRequest incomingOrder, string? ownerExternalUserId);

    OrderDTO? UpdateOrder(OrderUpdateRequest incomingOrder, string? callerExternalUserId, string? callerGuestAccessToken);

    Task<CheckoutDTO> CheckoutOrderAsync(long orderId, string? callerExternalUserId, string? callerGuestAccessToken, CancellationToken cancellationToken = default);

    /// <summary>Returns false when the order does not exist, caller is unauthorised, or delete fails.</summary>
    bool DeleteOrder(long orderId, string? callerExternalUserId, string? callerGuestAccessToken);
}
