using System.Security.Cryptography;
using System.Text;
using api.Dtos;
using Microsoft.EntityFrameworkCore;
using api.Models;
using api.Services.Provided;
using api.Services.Required;

namespace api.Services;

/// <summary>
/// SSD / BOLA: order reads and mutations require explicit caller identity on every entry point — controllers must forward JWT subject (or null for guests).
/// Guest carts require the secret token issued at creation (<see cref="Order.GuestAccessToken"/>).
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPaymentGateway _paymentGateway;

    /// <summary>SSD: single source of truth for standard delivery fee (avoids divergent literals across create/update paths).</summary>
    private static readonly decimal StandardDelivery = 2.50m;

    private const int MemorableNameSearchCap = 50;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        IUserService userService,
        IDateTimeProvider dateTimeProvider,
        IPaymentGateway paymentGateway)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _userService = userService;
        _dateTimeProvider = dateTimeProvider;
        _paymentGateway = paymentGateway;
    }

    public OrderDTO? GetOrderById(long id, string? callerExternalUserId, string? callerGuestAccessToken)
    {
        if (id <= 0)
            return null;

        var order = _orderRepository.GetById(id);
        if (order == null)
            return null;

        try
        {
            EnsureCallerMayViewOrder(order, callerExternalUserId, callerGuestAccessToken);
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }

        return MapToDto(order);
    }

    public List<OrderDTO> FindOrder(string memorableName, string? callerExternalUserId, string? callerGuestAccessToken)
    {
        var orders = _orderRepository.FindByMemorableName(memorableName, MemorableNameSearchCap);
        IEnumerable<Order> query = orders;

        if (string.IsNullOrEmpty(callerExternalUserId))
        {
            // SSD: only completed guest orders are discoverable by memorable name; unpaid carts stay secret.
            query = query.Where(o => o.UserId == null && o.MemorableName != null);
        }
        else
        {
            query = query.Where(o => o.User != null && string.Equals(o.User.ExternalUserId, callerExternalUserId, StringComparison.Ordinal));
        }

        return query
            .Where(o =>
            {
                try
                {
                    EnsureCallerMayViewOrder(o, callerExternalUserId, callerGuestAccessToken);
                    return true;
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
            })
            .Select(MapToDto)
            .ToList();
    }

    public OrderDTO CreateOrder(OrderCreateRequest incomingOrder, string? ownerExternalUserId)
    {
        var order = new Order(
            _dateTimeProvider.UtcNow.UtcDateTime,
            0, 0, null,
            incomingOrder.Promotion,
            null);

        foreach (var t in BuildTreats(incomingOrder.BasketItems))
            order.Treats.Add(t);

        ValidateOrder(order);
        CalculateCosts(order);

        if (!string.IsNullOrWhiteSpace(incomingOrder.Promotion))
            ApplyPromotion(order, incomingOrder.Promotion!);

        TryAssignOwner(order, ownerExternalUserId);

        if (order.UserId == null)
            order.GuestAccessToken = GenerateGuestAccessToken();

        _orderRepository.Add(order);
        _orderRepository.SaveChanges();

        return MapToDto(order, includeGuestAccessToken: order.UserId == null);
    }

    public OrderDTO? UpdateOrder(OrderUpdateRequest incomingOrder, string? callerExternalUserId, string? callerGuestAccessToken)
    {
        var order = _orderRepository.GetById(incomingOrder.OrderId);
        if (order == null)
            return null;

        if (order.PaidAt != null || !string.IsNullOrWhiteSpace(order.MemorableName))
            throw new InvalidOperationException("Cannot update an order that has already been checked out.");

        EnsureCallerMayMutateOrder(order, callerExternalUserId, callerGuestAccessToken);

        order.Treats.Clear();
        foreach (var t in BuildTreats(incomingOrder.BasketItems))
            order.Treats.Add(t);

        order.Promotion = incomingOrder.Promotion;

        ValidateOrder(order);
        CalculateCosts(order);

        if (!string.IsNullOrWhiteSpace(incomingOrder.Promotion))
            ApplyPromotion(order, incomingOrder.Promotion!);

        _orderRepository.SaveChanges();

        return MapToDto(order);
    }

    public async Task<CheckoutDTO> CheckoutOrderAsync(long orderId, string? callerExternalUserId, string? callerGuestAccessToken, CancellationToken cancellationToken = default)
    {
        if (orderId <= 0)
            throw new KeyNotFoundException();

        var order = _orderRepository.GetById(orderId)
            ?? throw new KeyNotFoundException();

        if (order.PaidAt != null || !string.IsNullOrWhiteSpace(order.MemorableName))
            throw new InvalidOperationException("Order has already been checked out.");

        EnsureCallerMayMutateOrder(order, callerExternalUserId, callerGuestAccessToken);

        var charge = new PaymentChargeRequest(order.Id, order.OrderTotal, "GBP");
        var paymentResponse = await _paymentGateway.ProcessPaymentAsync(charge, cancellationToken);
        if (!paymentResponse.Success)
            throw new InvalidOperationException("Payment was not accepted.");

        order.MemorableName = MemorableNameGenerator.Generate();
        order.PaidAt = _dateTimeProvider.UtcNow.UtcDateTime;
        order.GuestAccessToken = null;

        try
        {
            _orderRepository.SaveChanges();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Order has already been checked out.");
        }

        var estimatedUtc = CalcEstimatedDeliveryTime(order.OrderTime!.Value, order.EstDeliveryMinutes);
        var estimatedOffset = new DateTimeOffset(DateTime.SpecifyKind(estimatedUtc, DateTimeKind.Utc));

        return new CheckoutDTO(true, paymentResponse.TransactionId, MapToDto(order), estimatedOffset);
    }

    public bool DeleteOrder(long orderId, string? callerExternalUserId, string? callerGuestAccessToken)
    {
        try
        {
            if (orderId <= 0)
                return false;

            var order = _orderRepository.GetById(orderId);
            if (order == null)
                return false;

            EnsureCallerMayMutateOrder(order, callerExternalUserId, callerGuestAccessToken);
            _orderRepository.Delete(order);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>SSD: read-side BOLA — mirrors HTTP policy so a forgotten controller check cannot leak another user’s basket.</summary>
    private void EnsureCallerMayViewOrder(Order order, string? callerExternalUserId, string? callerGuestAccessToken)
    {
        var ownerId = ResolveOwnerExternalId(order);

        if (!string.IsNullOrEmpty(ownerId))
        {
            if (string.IsNullOrEmpty(callerExternalUserId) ||
                !string.Equals(ownerId, callerExternalUserId, StringComparison.Ordinal))
                throw new UnauthorizedAccessException();
            return;
        }

        // SSD: after checkout the guest secret is cleared; tracking uses the memorable name only.
        if (order.PaidAt != null && !string.IsNullOrEmpty(order.MemorableName))
            return;

        EnsureGuestTokenMatches(order, callerGuestAccessToken);
    }

    private void TryAssignOwner(Order order, string? ownerExternalUserId)
    {
        if (string.IsNullOrWhiteSpace(ownerExternalUserId) || order.UserId != null)
            return;

        var dto = _userService.GetUserById(ownerExternalUserId);
        order.UserId = dto.Id;
        order.User = _userRepository.GetById(dto.Id);
        order.GuestAccessToken = null;
    }

    private void EnsureCallerMayMutateOrder(Order order, string? callerExternalUserId, string? callerGuestAccessToken)
    {
        var ownerId = ResolveOwnerExternalId(order);

        if (!string.IsNullOrEmpty(ownerId))
        {
            if (string.IsNullOrEmpty(callerExternalUserId) ||
                !string.Equals(ownerId, callerExternalUserId, StringComparison.Ordinal))
                throw new UnauthorizedAccessException();
            return;
        }

        EnsureGuestTokenMatches(order, callerGuestAccessToken);
    }

    private static void EnsureGuestTokenMatches(Order order, string? callerGuestAccessToken)
    {
        if (string.IsNullOrEmpty(order.GuestAccessToken))
            throw new UnauthorizedAccessException();

        var expected = Encoding.UTF8.GetBytes(order.GuestAccessToken);
        var provided = Encoding.UTF8.GetBytes(callerGuestAccessToken ?? string.Empty);

        if (expected.Length != provided.Length ||
            !CryptographicOperations.FixedTimeEquals(expected, provided))
            throw new UnauthorizedAccessException();
    }

    private string? ResolveOwnerExternalId(Order order) =>
        order.User?.ExternalUserId
        ?? (order.UserId is long uid ? _userRepository.GetById(uid)?.ExternalUserId : null);

    private static string GenerateGuestAccessToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    private List<Treat> BuildTreats(IEnumerable<TreatBasketRequest>? basketItems)
    {
        var treats = new List<Treat>();
        if (basketItems == null)
            return treats;

        foreach (var treatDto in basketItems)
        {
            var productIds = treatDto.Products?.Select(p => p.ProductId).ToList() ?? [];
            var products = productIds.Select(id =>
                _productRepository.GetById(id)
                ?? throw new KeyNotFoundException()
            ).ToList();

            treats.Add(new Treat { Products = products });
        }

        return treats;
    }

    private static void ValidateOrder(Order order)
    {
        if (!ValidateBasketSize(order.Treats.Count))
            throw new InvalidOperationException($"Invalid basket size: {order.Treats.Count}");

        foreach (var treat in order.Treats)
            ValidateTreatProducts(treat.Products);
    }

    private static bool ValidateBasketSize(int count) => count is >= 1 and <= 100;

    private static void ValidateTreatProducts(List<Product> products)
    {
        if (products.Count == 0)
            throw new InvalidOperationException("Each treat must include at least one product.");

        var cones = products.Count(p => p.ProductType == ProductType.CONE);
        var flavours = products.Count(p => p.ProductType == ProductType.FLAVOR);

        if (cones != 1)
            throw new InvalidOperationException("Each treat must include exactly one cone.");

        if (flavours < 1)
            throw new InvalidOperationException("Each treat must include at least one flavour.");
    }

    private static void CalculateCosts(Order order)
    {
        decimal subTotal = RoundMoney(Math.Max(0m, CalcSubtotal(order.Treats)));
        decimal seasonalSurcharge = RoundMoney(Math.Max(0m, CalcSurcharge(order.OrderTime!.Value)));
        decimal totalDelivery = RoundMoney(Math.Max(0m, StandardDelivery + seasonalSurcharge));

        int basketSize = order.Treats.Count;
        int productCount = order.Treats.Sum(t => t.Products.Count);

        order.DeliveryCost = totalDelivery;
        order.OrderTotal = RoundMoney(Math.Max(0m, subTotal + totalDelivery));
        order.EstDeliveryMinutes = CalcEstDeliveryMinutes(basketSize, productCount);

        if (order.DeliveryCost < 0m || order.OrderTotal < 0m)
            throw new InvalidOperationException("Computed totals must be non-negative.");
    }

    private static decimal CalcSubtotal(IEnumerable<Treat> treats) =>
        treats.Sum(t => t.Products.Sum(p => p.Price));

    private static decimal RoundMoney(decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    private static decimal CalcSurcharge(DateTime orderTimeUtc)
    {
        var month = orderTimeUtc.Month;
        return month is 6 or 7 or 8 ? 0.50m : 0m;
    }

    private static int CalcEstDeliveryMinutes(int basketSize, int productCount) =>
        Math.Min(120, 15 + basketSize * 4 + Math.Max(0, productCount - basketSize * 2));

    private static DateTime CalcEstimatedDeliveryTime(DateTime orderTimeUtc, int? estDeliveryMinutes) =>
        orderTimeUtc.AddMinutes(estDeliveryMinutes ?? 30);

    private static void ApplyPromotion(Order order, string promotion)
    {
        var code = promotion.Trim();
        if (string.IsNullOrEmpty(code))
            return;

        var subtotal = RoundMoney(Math.Max(0m, order.OrderTotal - order.DeliveryCost));
        var baseTotal = order.OrderTotal;
        var treatCount = order.Treats.Count;

        static bool Eq(string a, string b) =>
            string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        decimal newTotal;
        if (Eq(code, "LuckyForSome") && baseTotal >= 13m)
            newTotal = baseTotal * 0.87m;
        else if (Eq(code, "MegaMelt100") && baseTotal >= 100m)
            newTotal = baseTotal - 20m;
        else if (Eq(code, "Frozen40") && subtotal >= 40m && treatCount >= 4)
            newTotal = baseTotal * 0.60m;
        else if (Eq(code, "TripleTreat3") && treatCount >= 3)
            newTotal = baseTotal - 3m;
        else if (Eq(code, "ScoopThereItIs!"))
            newTotal = baseTotal - 1m;
        else
            throw new InvalidOperationException("Unknown or ineligible promotion code.");

        order.Promotion = code;
        order.OrderTotal = RoundMoney(Math.Max(0m, newTotal));
    }

    private static OrderDTO MapToDto(Order order, bool includeGuestAccessToken = false)
    {
        var basket = order.Treats.Select(t =>
            new TreatDTO(t.Products.Select(MapProductToDto).ToList())).ToList();

        return new OrderDTO(
            order.Id,
            order.OrderTime,
            order.OrderTotal,
            order.DeliveryCost,
            order.EstDeliveryMinutes,
            order.Promotion,
            order.MemorableName,
            basket,
            includeGuestAccessToken ? order.GuestAccessToken : null);
    }

    private static ProductDTO MapProductToDto(Product p) =>
        new(
            p.Id,
            p.Name,
            p.Price,
            p.Description ?? string.Empty,
            p.Ingredients,
            p.ProductType);
}
