using MediatR;
using Parking.Contracts.Enums;
using Parking.Contracts.V1.Payments;

namespace Parking.Application.Payments.Commands.InitiatePayment;

public sealed record InitiatePaymentCommand(
    Guid SessionId,
    decimal Amount,
    string Currency,
    ExternalPaymentMethod Method
) : IRequest<InitiatePaymentResponse>;
