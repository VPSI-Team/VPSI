using Parking.Application.Abstractions;

namespace Parking.Infrastructure.Services;

/// <summary>Stub implementation — replace with real payment provider integration.</summary>
internal sealed class PaymentGatewayStub : IPaymentGateway
{
    public Task<PaymentGatewayResult> ChargeAsync(PaymentGatewayRequest request, CancellationToken ct = default) =>
        Task.FromResult(new PaymentGatewayResult(true, $"stub-{request.PaymentIntentId}", null));
}
