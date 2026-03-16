using Parking.Domain.ValueObjects;

namespace Parking.Domain.Entities;

public sealed class Vehicle
{
    public Guid Id { get; private set; }
    public PlateNumber PlateNumber { get; private set; } = null!;
    public string? CountryCode { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Vehicle() { }

    public static Vehicle Create(PlateNumber plateNumber, string? countryCode = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            PlateNumber = plateNumber,
            CountryCode = countryCode,
            CreatedAt = DateTimeOffset.UtcNow
        };
}
