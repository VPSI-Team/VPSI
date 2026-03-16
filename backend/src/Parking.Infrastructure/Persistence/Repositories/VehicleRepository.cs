using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Domain.Entities;
using Parking.Domain.ValueObjects;

namespace Parking.Infrastructure.Persistence.Repositories;

internal sealed class VehicleRepository(AppDbContext db) : IVehicleRepository
{
    public Task<Vehicle?> GetByPlateAsync(PlateNumber plate, CancellationToken ct = default) =>
        db.Vehicles.FirstOrDefaultAsync(v => v.PlateNumber.Value == plate.Value, ct);

    public async Task AddAsync(Vehicle vehicle, CancellationToken ct = default) =>
        await db.Vehicles.AddAsync(vehicle, ct);
}
