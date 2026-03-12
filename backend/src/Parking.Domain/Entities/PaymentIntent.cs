using Parking.Domain.Enums;
using Parking.Domain.ValueObjects;

namespace Parking.Domain.Entities;

public sealed class PaymentIntent
{
    public Guid Id { get; private set; }
    public Guid ParkingSessionId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? ProviderRef { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private PaymentIntent() { }

    internal static PaymentIntent Create(Guid sessionId, Money amount, PaymentMethod method) =>
        new()
        {
            Id = Guid.NewGuid(),
            ParkingSessionId = sessionId,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Initiated,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Authorize(string? providerRef = null)
    {
        Status = PaymentStatus.Authorized;
        ProviderRef = providerRef;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Capture()
    {
        if (Status != PaymentStatus.Authorized)
            throw new InvalidOperationException("Payment must be authorized before it can be captured.");
        Status = PaymentStatus.Captured;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Fail()
    {
        Status = PaymentStatus.Failed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
