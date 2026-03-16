using Parking.Domain.Entities;
using Parking.Domain.ValueObjects;

namespace Parking.Application.Abstractions;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByPlateAsync(PlateNumber plate, CancellationToken ct = default);
    Task AddAsync(Vehicle vehicle, CancellationToken ct = default);
}
