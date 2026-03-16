using Parking.Domain.Entities;
using Parking.Domain.Enums;

namespace Parking.Application.Abstractions;

public interface IParkingSessionRepository
{
    Task<ParkingSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ParkingSession?> GetActiveByVehicleIdAsync(Guid vehicleId, CancellationToken ct = default);
    Task<int> CountActiveInLotAsync(Guid parkingLotId, CancellationToken ct = default);
    Task AddAsync(ParkingSession session, CancellationToken ct = default);
    void Update(ParkingSession session);
}
