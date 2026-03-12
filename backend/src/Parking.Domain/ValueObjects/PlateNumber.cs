namespace Parking.Domain.ValueObjects;

public sealed record PlateNumber
{
    public string Value { get; }

    private PlateNumber(string value) => Value = value;

    public static PlateNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Plate number cannot be empty.", nameof(value));

        var normalized = value.Trim().ToUpperInvariant().Replace(" ", "");

        if (normalized.Length > 10)
            throw new ArgumentException("Plate number is too long.", nameof(value));

        return new PlateNumber(normalized);
    }

    public override string ToString() => Value;
}
