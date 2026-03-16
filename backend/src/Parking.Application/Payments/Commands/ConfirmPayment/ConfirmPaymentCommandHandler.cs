using MediatR;

namespace Parking.Application.Payments.Commands.ConfirmPayment;

public sealed class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand>
{
    public Task Handle(ConfirmPaymentCommand request, CancellationToken ct)
    {
        // Full implementation requires a dedicated PaymentIntentRepository — follow-up issue.
        throw new NotImplementedException("Requires PaymentIntentRepository — to be wired in a follow-up.");
    }
}
