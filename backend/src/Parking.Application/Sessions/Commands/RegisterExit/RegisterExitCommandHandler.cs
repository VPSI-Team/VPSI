using MediatR;
using Parking.Application.Abstractions;
using Parking.Domain.Exceptions;

namespace Parking.Application.Sessions.Commands.RegisterExit;

public sealed class RegisterExitCommandHandler(
    IParkingSessionRepository sessionRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RegisterExitCommand>
{
    public async Task Handle(RegisterExitCommand request, CancellationToken ct)
    {
        var session = await sessionRepository.GetByIdAsync(request.SessionId, ct)
            ?? throw new KeyNotFoundException($"Session {request.SessionId} not found.");

        session.Close(request.DeviceId);
        sessionRepository.Update(session);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
