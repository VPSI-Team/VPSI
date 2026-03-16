using MediatR;
using Parking.Contracts.Enums;

namespace Parking.Application.Payments.Commands.ConfirmPayment;

public sealed record ConfirmPaymentCommand(
    Guid PaymentIntentId,
    string ProviderRef,
    ExternalPaymentCallbackStatus CallbackStatus
) : IRequest;
