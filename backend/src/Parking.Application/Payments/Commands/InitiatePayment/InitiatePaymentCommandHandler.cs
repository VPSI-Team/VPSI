using MediatR;
using Parking.Application.Abstractions;
using Parking.Contracts.V1.Payments;
using Parking.Domain.Enums;
using Parking.Domain.ValueObjects;

namespace Parking.Application.Payments.Commands.InitiatePayment;

public sealed class InitiatePaymentCommandHandler(
    IParkingSessionRepository sessionRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<InitiatePaymentCommand, InitiatePaymentResponse>
{
    public async Task<InitiatePaymentResponse> Handle(InitiatePaymentCommand request, CancellationToken ct)
    {
        var session = await sessionRepository.GetByIdAsync(request.SessionId, ct)
            ?? throw new KeyNotFoundException($"Session {request.SessionId} not found.");

        var amount = Money.Create(request.Amount, request.Currency);
        var method = MapMethod(request.Method);
        var intent = session.AddPaymentIntent(amount, method);

        sessionRepository.Update(session);
        await unitOfWork.SaveChangesAsync(ct);

        return new InitiatePaymentResponse(intent.Id, intent.Status.ToString(), null);
    }

    private static PaymentMethod MapMethod(Contracts.Enums.ExternalPaymentMethod method) => method switch
    {
        Contracts.Enums.ExternalPaymentMethod.Card => PaymentMethod.Card,
        Contracts.Enums.ExternalPaymentMethod.MobileApp => PaymentMethod.MobileApp,
        Contracts.Enums.ExternalPaymentMethod.Qr => PaymentMethod.Qr,
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
    };
}
