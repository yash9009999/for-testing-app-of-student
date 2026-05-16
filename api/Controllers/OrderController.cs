using api.Dtos;
using api.Services.Provided;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

/// <summary>
/// SSD: BOLA mitigations on reads/mutations; errors mapped to generic payloads (no exception.Message to clients).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private const string NotFoundMessage = "The requested resource was not found.";
    private const string ConflictMessage = "The request could not be completed.";
    private const string GuestTokenHeader = "X-Guest-Token";

    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private string? CallerExternalId => HttpContext.Items["UserId"] as string;

    private string? CallerGuestAccessToken =>
        Request.Headers.TryGetValue(GuestTokenHeader, out var values)
            ? values.FirstOrDefault()
            : null;

    [HttpGet("{orderId}")]
    public ActionResult<OrderDTO> GetOrder(long orderId)
    {
        var order = _orderService.GetOrderById(orderId, CallerExternalId, CallerGuestAccessToken);
        if (order == null)
            return NotFound(new { message = NotFoundMessage });

        return Ok(order);
    }

    [HttpGet("search")]
    public ActionResult<List<OrderDTO>> FindOrders([FromQuery] string memorableName)
    {
        var orders = _orderService.FindOrder(memorableName, CallerExternalId, CallerGuestAccessToken);
        if (orders.Count == 0)
            return NotFound(new { message = NotFoundMessage });

        return Ok(orders);
    }

    [HttpPost]
    public ActionResult<OrderDTO> CreateOrder([FromBody] OrderCreateRequest incomingOrder)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var outgoingOrder = _orderService.CreateOrder(incomingOrder, CallerExternalId);
            return CreatedAtAction(nameof(GetOrder), new { orderId = outgoingOrder.OrderId }, outgoingOrder);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = NotFoundMessage });
        }
        catch (InvalidOperationException)
        {
            return Conflict(new { message = ConflictMessage });
        }
    }

    [HttpPut]
    public ActionResult<OrderDTO> UpdateOrder([FromBody] OrderUpdateRequest incomingOrder)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var outgoingOrder = _orderService.UpdateOrder(incomingOrder, CallerExternalId, CallerGuestAccessToken);
            if (outgoingOrder == null)
                return NotFound(new { message = NotFoundMessage });

            return Ok(outgoingOrder);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = NotFoundMessage });
        }
        catch (UnauthorizedAccessException)
        {
            return NotFound(new { message = NotFoundMessage });
        }
        catch (InvalidOperationException)
        {
            return Conflict(new { message = ConflictMessage });
        }
    }

    [HttpPost("{orderId}/checkout")]
    public async Task<ActionResult<CheckoutDTO>> CheckoutOrder(long orderId, CancellationToken cancellationToken)
    {
        try
        {
            var checkout = await _orderService.CheckoutOrderAsync(orderId, CallerExternalId, CallerGuestAccessToken, cancellationToken);
            return Ok(checkout);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = NotFoundMessage });
        }
        catch (UnauthorizedAccessException)
        {
            return NotFound(new { message = NotFoundMessage });
        }
        catch (InvalidOperationException)
        {
            return Conflict(new { message = ConflictMessage });
        }
    }

    [HttpDelete("{orderId}")]
    public IActionResult DeleteOrder(long orderId)
    {
        if (!_orderService.DeleteOrder(orderId, CallerExternalId, CallerGuestAccessToken))
            return NotFound(new { message = NotFoundMessage });

        return NoContent();
    }
}
