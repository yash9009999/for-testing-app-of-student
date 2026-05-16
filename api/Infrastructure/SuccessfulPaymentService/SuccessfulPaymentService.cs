using api.Dtos;
using api.Services.Required;
using Microsoft.Extensions.Logging;

namespace api.Infrastructure.SuccessfulPaymentService;

/// <summary>
/// SSD: demo gateway — still cannot detect real-world fraud without a PSP integration; this class logs a correlation id for reconciliation/audit only (no PAN data exists in this sample).
/// </summary>
public class SuccessfulPaymentService : IPaymentGateway
{
    private readonly ILogger<SuccessfulPaymentService> _logger;

    public SuccessfulPaymentService(ILogger<SuccessfulPaymentService> logger)
    {
        _logger = logger;
    }

    public async Task<PaymentDTO> ProcessPaymentAsync(PaymentChargeRequest charge, CancellationToken cancellationToken = default)
    {
        // SSD: yield so callers do not block synchronously on network-shaped work; real gateways would await HTTP here.
        await Task.Yield();

        if (charge.Amount < 0m)
        {
            _logger.LogWarning("Payment rejected: negative amount for OrderId {OrderId}", charge.OrderId);
            return new PaymentDTO(false, "none", "Invalid payment amount.");
        }

        var transactionId = Guid.NewGuid().ToString();

        // SSD: structured audit trail — replace with PSP settlement id + merchant reference in production.
        _logger.LogInformation(
            "Payment captured (demo). OrderId={OrderId} Amount={Amount} Currency={Currency} TransactionId={TransactionId}",
            charge.OrderId,
            charge.Amount,
            charge.Currency,
            transactionId);

        return new PaymentDTO(true, transactionId, "Payment successful.");
    }
}
