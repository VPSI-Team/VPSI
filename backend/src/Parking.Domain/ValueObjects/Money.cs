namespace Parking.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "CZK")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        return new Money(Math.Round(amount, 2), currency.ToUpperInvariant());
    }

    public static Money Zero(string currency = "CZK") => new(0m, currency.ToUpperInvariant());

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {Currency} and {other.Currency}.");
        return new Money(Amount + other.Amount, Currency);
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
