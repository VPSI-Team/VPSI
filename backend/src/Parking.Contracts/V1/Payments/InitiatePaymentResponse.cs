namespace Parking.Contracts.V1.Payments;

public sealed record InitiatePaymentResponse(
    Guid PaymentIntentId,
    string Status,
    string? ProviderRedirectUrl
);
