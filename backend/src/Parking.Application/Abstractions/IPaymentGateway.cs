namespace Parking.Application.Abstractions;

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ChargeAsync(PaymentGatewayRequest request, CancellationToken ct = default);
}

public sealed record PaymentGatewayRequest(
    Guid PaymentIntentId,
    decimal Amount,
    string Currency,
    string Method
);

public sealed record PaymentGatewayResult(
    bool Success,
    string? ProviderRef,
    string? ErrorMessage
);
