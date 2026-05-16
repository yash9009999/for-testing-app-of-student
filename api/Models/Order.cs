namespace api.Models;

/// <summary>
/// SSD: <see cref="User"/> and <see cref="Treats"/> are public for EF graph materialisation — never return this entity from HTTP; always map through <see cref="Dtos.OrderDTO"/>.
/// </summary>
public class Order
{
    public long Id { get; set; }

    public DateTime? OrderTime { get; set; }

    public decimal OrderTotal { get; set; }

    public decimal DeliveryCost { get; set; }

    public int? EstDeliveryMinutes { get; set; }

    public string? Promotion { get; set; }

    public string? MemorableName { get; set; }

    /// <summary>SSD: secret required for guest-cart access when <see cref="UserId"/> is null — mitigates sequential order-id IDOR.</summary>
    public string? GuestAccessToken { get; set; }

    /// <summary>SSD: set when checkout completes successfully — prevents double payment if checkout is retried after a successful run.</summary>
    public DateTime? PaidAt { get; set; }

    public List<Treat> Treats { get; set; } = new();

    public long? UserId { get; set; }

    public User? User { get; set; }

    public long? Version { get; set; }

    public bool IsDeleted { get; set; }

    protected Order()
    {
        // For deserialization
    }

    public Order(DateTime orderTime, decimal orderTotal, decimal deliveryCost, int? estDeliveryMinutes, string? promotion, string? memorableName = null)
    {
        OrderTime = orderTime;
        OrderTotal = orderTotal;
        DeliveryCost = deliveryCost;
        EstDeliveryMinutes = estDeliveryMinutes;
        Promotion = promotion;
        MemorableName = memorableName;
    }
}
