using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Domain.Entities;
using Parking.Domain.Enums;

namespace Parking.Infrastructure.Persistence.Repositories;

internal sealed class ParkingSessionRepository(AppDbContext db) : IParkingSessionRepository
{
    public Task<ParkingSession?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.ParkingSessions
            .Include(s => s.PaymentIntents)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<ParkingSession?> GetActiveByVehicleIdAsync(Guid vehicleId, CancellationToken ct = default) =>
        db.ParkingSessions
            .FirstOrDefaultAsync(s => s.VehicleId == vehicleId && s.Status == ParkingSessionStatus.Active, ct);

    public Task<int> CountActiveInLotAsync(Guid parkingLotId, CancellationToken ct = default) =>
        db.ParkingSessions
            .CountAsync(s => s.ParkingLotId == parkingLotId && s.Status == ParkingSessionStatus.Active, ct);

    public async Task AddAsync(ParkingSession session, CancellationToken ct = default) =>
        await db.ParkingSessions.AddAsync(session, ct);

    public void Update(ParkingSession session) =>
        db.ParkingSessions.Update(session);
}
