using MediatR;
using Parking.Application.Abstractions;
using Parking.Contracts.V1.Sessions;
using Parking.Domain.Entities;
using Parking.Domain.ValueObjects;

namespace Parking.Application.Sessions.Commands.RegisterEntry;

public sealed class RegisterEntryCommandHandler(
    IVehicleRepository vehicleRepository,
    IParkingSessionRepository sessionRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RegisterEntryCommand, RegisterEntryResponse>
{
    public async Task<RegisterEntryResponse> Handle(RegisterEntryCommand request, CancellationToken ct)
    {
        var plate = PlateNumber.Create(request.PlateNumber);

        var vehicle = await vehicleRepository.GetByPlateAsync(plate, ct)
            ?? await CreateVehicleAsync(plate, request.CountryCode, ct);

        var session = ParkingSession.Start(request.ParkingLotId, vehicle.Id, request.DeviceId);
        await sessionRepository.AddAsync(session, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new RegisterEntryResponse(session.Id, plate.Value, session.TimeRange.Start);
    }

    private async Task<Vehicle> CreateVehicleAsync(PlateNumber plate, string? countryCode, CancellationToken ct)
    {
        var vehicle = Vehicle.Create(plate, countryCode);
        await vehicleRepository.AddAsync(vehicle, ct);
        return vehicle;
    }
}
