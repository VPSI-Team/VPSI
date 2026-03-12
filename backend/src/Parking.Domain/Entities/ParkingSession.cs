using Parking.Domain.Enums;
using Parking.Domain.Exceptions;
using Parking.Domain.ValueObjects;

namespace Parking.Domain.Entities;

public sealed class ParkingSession
{
    public Guid Id { get; private set; }
    public Guid ParkingLotId { get; private set; }
    public Guid VehicleId { get; private set; }
    public ParkingSessionStatus Status { get; private set; }
    public TimeRange TimeRange { get; private set; } = null!;
    public Money? TotalAmount { get; private set; }
    public Guid? EntryDeviceId { get; private set; }
    public Guid? ExitDeviceId { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }

    private readonly List<PaymentIntent> _paymentIntents = [];
    public IReadOnlyCollection<PaymentIntent> PaymentIntents => _paymentIntents.AsReadOnly();

    private ParkingSession() { }

    public static ParkingSession Start(Guid parkingLotId, Guid vehicleId, Guid? entryDeviceId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            ParkingLotId = parkingLotId,
            VehicleId = vehicleId,
            Status = ParkingSessionStatus.Active,
            TimeRange = TimeRange.StartAt(DateTimeOffset.UtcNow),
            EntryDeviceId = entryDeviceId
        };

    /// <summary>Marks session as paid. Transition: Active → Paid.</summary>
    public void MarkAsPaid(Money totalAmount)
    {
        if (Status != ParkingSessionStatus.Active)
            throw new InvalidSessionStateException(Status, ParkingSessionStatus.Paid);

        TotalAmount = totalAmount;
        PaidAt = DateTimeOffset.UtcNow;
        Status = ParkingSessionStatus.Paid;
    }

    /// <summary>Closes the session on vehicle exit. Transition: Active|Paid → Closed.</summary>
    public void Close(Guid? exitDeviceId = null)
    {
        if (Status is not (ParkingSessionStatus.Active or ParkingSessionStatus.Paid))
            throw new InvalidSessionStateException(Status, ParkingSessionStatus.Closed);

        TimeRange = TimeRange.Close(DateTimeOffset.UtcNow);
        ExitDeviceId = exitDeviceId;
        Status = ParkingSessionStatus.Closed;
    }

    /// <summary>Marks session as disputed (any state).</summary>
    public void MarkAsDisputed()
    {
        Status = ParkingSessionStatus.Disputed;
    }

    /// <summary>Creates a new payment intent for this session.</summary>
    public PaymentIntent AddPaymentIntent(Money amount, PaymentMethod method)
    {
        if (Status != ParkingSessionStatus.Active)
            throw new InvalidSessionStateException(Status, ParkingSessionStatus.Paid);

        var intent = PaymentIntent.Create(Id, amount, method);
        _paymentIntents.Add(intent);
        return intent;
    }
}
