using api.Dtos;

namespace api.Services.Required;

public interface IPaymentGateway
{
    /// <summary>
    /// SSD: async contract — real gateways are network-bound; demo implementation still yields the thread without blocking ASP.NET worker pools under load.
    /// </summary>
    Task<PaymentDTO> ProcessPaymentAsync(PaymentChargeRequest charge, CancellationToken cancellationToken = default);
}
