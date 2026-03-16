using FluentAssertions;
using Parking.Domain.ValueObjects;

namespace Parking.UnitTests.Domain;

public sealed class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_Succeeds()
    {
        var money = Money.Create(100m, "CZK");
        money.Amount.Should().Be(100m);
        money.Currency.Should().Be("CZK");
    }

    [Fact]
    public void Create_WithNegativeAmount_Throws()
    {
        var act = () => Money.Create(-1m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        var a = Money.Create(30m);
        var b = Money.Create(70m);
        a.Add(b).Amount.Should().Be(100m);
    }

    [Fact]
    public void Add_DifferentCurrency_Throws()
    {
        var czk = Money.Create(100m, "CZK");
        var eur = Money.Create(10m, "EUR");
        var act = () => czk.Add(eur);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Zero_ReturnsZeroAmount()
    {
        Money.Zero().Amount.Should().Be(0m);
    }
}
