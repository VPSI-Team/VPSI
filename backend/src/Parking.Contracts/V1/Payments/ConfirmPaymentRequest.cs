using Parking.Contracts.Enums;

namespace Parking.Contracts.V1.Payments;

public sealed record ConfirmPaymentRequest(
    string ProviderRef,
    ExternalPaymentCallbackStatus Status,
    DateTimeOffset ProcessedAt
);
