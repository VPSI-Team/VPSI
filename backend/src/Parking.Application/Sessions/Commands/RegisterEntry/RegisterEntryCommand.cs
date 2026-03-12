using MediatR;
using Parking.Contracts.V1.Sessions;

namespace Parking.Application.Sessions.Commands.RegisterEntry;

public sealed record RegisterEntryCommand(
    string PlateNumber,
    string? CountryCode,
    Guid ParkingLotId,
    Guid? DeviceId
) : IRequest<RegisterEntryResponse>;
