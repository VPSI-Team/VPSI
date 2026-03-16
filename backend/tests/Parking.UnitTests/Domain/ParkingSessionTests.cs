using FluentAssertions;
using Parking.Domain.Entities;
using Parking.Domain.Enums;
using Parking.Domain.Exceptions;
using Parking.Domain.ValueObjects;

namespace Parking.UnitTests.Domain;

public sealed class ParkingSessionTests
{
    private static ParkingSession CreateSession() =>
        ParkingSession.Start(Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Start_CreatesActiveSession()
    {
        var session = CreateSession();
        session.Status.Should().Be(ParkingSessionStatus.Active);
        session.TimeRange.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void MarkAsPaid_FromActive_TransitionsToPaid()
    {
        var session = CreateSession();
        session.MarkAsPaid(Money.Create(80m));
        session.Status.Should().Be(ParkingSessionStatus.Paid);
        session.PaidAt.Should().NotBeNull();
        session.TotalAmount!.Amount.Should().Be(80m);
    }

    [Fact]
    public void MarkAsPaid_FromClosed_Throws()
    {
        var session = CreateSession();
        session.MarkAsPaid(Money.Create(50m));
        session.Close();

        var act = () => session.MarkAsPaid(Money.Create(50m));
        act.Should().Throw<InvalidSessionStateException>();
    }

    [Fact]
    public void Close_FromPaid_TransitionsToClosed()
    {
        var session = CreateSession();
        session.MarkAsPaid(Money.Create(50m));
        session.Close();

        session.Status.Should().Be(ParkingSessionStatus.Closed);
        session.TimeRange.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void Close_FromActive_TransitionsToClosed()
    {
        var session = CreateSession();
        session.Close(); // emergency / free exit
        session.Status.Should().Be(ParkingSessionStatus.Closed);
    }

    [Fact]
    public void AddPaymentIntent_FromActive_AddsIntent()
    {
        var session = CreateSession();
        var intent = session.AddPaymentIntent(Money.Create(100m), PaymentMethod.Card);
        session.PaymentIntents.Should().ContainSingle();
        intent.Status.Should().Be(PaymentStatus.Initiated);
    }

    [Fact]
    public void AddPaymentIntent_WhenPaid_Throws()
    {
        var session = CreateSession();
        session.MarkAsPaid(Money.Create(100m));

        var act = () => session.AddPaymentIntent(Money.Create(100m), PaymentMethod.Card);
        act.Should().Throw<InvalidSessionStateException>();
    }
}
