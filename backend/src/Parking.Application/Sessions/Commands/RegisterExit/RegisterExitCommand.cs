using MediatR;

namespace Parking.Application.Sessions.Commands.RegisterExit;

public sealed record RegisterExitCommand(
    Guid SessionId,
    Guid? DeviceId
) : IRequest;
