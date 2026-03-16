using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Parking.Application.Payments.Commands.InitiatePayment;
using Parking.Contracts.V1.Payments;

namespace Parking.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sessions/{sessionId:guid}/payments")]
public sealed class PaymentsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(InitiatePaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> InitiatePayment(
        Guid sessionId,
        [FromBody] InitiatePaymentRequest request,
        CancellationToken ct)
    {
        var command = new InitiatePaymentCommand(sessionId, request.Amount, request.Currency, request.Method);
        var result = await mediator.Send(command, ct);
        return Created(string.Empty, result);
    }
}
