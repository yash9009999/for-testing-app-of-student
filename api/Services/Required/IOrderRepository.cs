using api.Models;

namespace api.Services.Required;

public interface IOrderRepository
{
    Order? GetById(long id);

    /// <summary>SSD: bounded result set — prevents memory blow-up on short/common memorable names.</summary>
    List<Order> FindByMemorableName(string query, int maxResults = 50);

    Order Add(Order order);
    Order Update(Order order);
    void Delete(Order order);

    /// <summary>SSD: unit-of-work escape hatch — prefer atomic repository methods where possible to avoid forgotten saves.</summary>
    void SaveChanges();
}
