using Parking.Contracts.Enums;

namespace Parking.Contracts.V1.Payments;

public sealed record InitiatePaymentRequest(
    decimal Amount,
    string Currency,
    ExternalPaymentMethod Method
);
