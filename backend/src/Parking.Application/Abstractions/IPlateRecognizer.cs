namespace Parking.Application.Abstractions;

public interface IPlateRecognizer
{
    Task<PlateReadResult?> ReadAsync(byte[] imageData, CancellationToken ct = default);
}

public sealed record PlateReadResult(string Plate, double Confidence, string? CountryCode);
