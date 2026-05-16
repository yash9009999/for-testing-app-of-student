namespace api.Models;

public class Treat
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public Order? Order { get; set; }

    public List<Product> Products { get; set; } = new();

    public bool IsDeleted { get; set; }

    public Treat()
    {
        // For deserialization and EF
    }

    public Treat(long orderId)
    {
        OrderId = orderId;
    }
}
