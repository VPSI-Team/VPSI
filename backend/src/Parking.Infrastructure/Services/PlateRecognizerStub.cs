using Parking.Application.Abstractions;

namespace Parking.Infrastructure.Services;

/// <summary>Stub implementation — replace with real LPR integration.</summary>
internal sealed class PlateRecognizerStub : IPlateRecognizer
{
    public Task<PlateReadResult?> ReadAsync(byte[] imageData, CancellationToken ct = default) =>
        Task.FromResult<PlateReadResult?>(null);
}
