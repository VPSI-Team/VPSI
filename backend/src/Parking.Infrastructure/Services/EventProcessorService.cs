using System.Text.Json;
using Parking.Application.Abstractions;
using Parking.Contracts.Enums;

namespace Parking.Infrastructure.Services;

internal sealed class EventProcessorService : IEventProcessor
{
    private readonly IParkingSessionRepository _sessionRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EventProcessorService(
        IParkingSessionRepository sessionRepository,
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ProcessAsync(Guid deviceId, HwEventType eventType, JsonElement payload, CancellationToken ct = default)
    {
        switch (eventType)
        {
            case HwEventType.LprPlateRead:
                await HandleLprReadAsync(deviceId, payload, ct);
                break;
            default:
                break;
        }
    }

    private Task HandleLprReadAsync(Guid deviceId, JsonElement payload, CancellationToken ct)
    {
        // Placeholder — full implementation in follow-up
        _ = (_sessionRepository, _vehicleRepository, _unitOfWork);
        return Task.CompletedTask;
    }
}
