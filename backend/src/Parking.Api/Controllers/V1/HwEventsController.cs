using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Parking.Application.Abstractions;
using Parking.Contracts.V1.HwEvents;

namespace Parking.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/hw/events")]
public sealed class HwEventsController(IEventProcessor eventProcessor) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(HwEventResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IngestEvent(
        [FromBody] HwEventRequest request,
        CancellationToken ct)
    {
        await eventProcessor.ProcessAsync(request.DeviceId, request.EventType, request.Payload, ct);

        var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString() ?? Guid.NewGuid().ToString();

        return Accepted(new HwEventResponse("ACCEPTED", correlationId, "ASYNC"));
    }
}
