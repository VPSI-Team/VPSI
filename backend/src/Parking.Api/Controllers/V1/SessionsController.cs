using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Parking.Application.Sessions.Commands.RegisterEntry;
using Parking.Application.Sessions.Commands.RegisterExit;
using Parking.Contracts.V1.Sessions;

namespace Parking.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sessions")]
public sealed class SessionsController(IMediator mediator) : ControllerBase
{
    [HttpPost("entry")]
    [ProducesResponseType(typeof(RegisterEntryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterEntry(
        [FromBody] RegisterEntryRequest request,
        CancellationToken ct)
    {
        var command = new RegisterEntryCommand(
            request.PlateNumber,
            request.CountryCode,
            request.ParkingLotId,
            request.DeviceId);

        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(RegisterEntry), new { id = result.SessionId }, result);
    }

    [HttpPost("{sessionId:guid}/exit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegisterExit(
        Guid sessionId,
        [FromQuery] Guid? deviceId,
        CancellationToken ct)
    {
        await mediator.Send(new RegisterExitCommand(sessionId, deviceId), ct);
        return NoContent();
    }
}
