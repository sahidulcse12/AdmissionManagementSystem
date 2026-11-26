using Admission.Api.Middleware;
using Admission.Application.Holds;
using Microsoft.AspNetCore.Mvc;

namespace Admission.Api.Controllers;

[Route("holds")]
[Tags("Applications (holds)")]
public sealed class HoldsController(HoldService holds) : ApiControllerBase
{
    [HttpGet("{holdId}")]
    [EndpointSummary("Current status of an application (X-Student-Id optional)")]
    [ProducesResponseType<ErrorBody>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorBody>(StatusCodes.Status404NotFound)]
    public async Task<HoldResponse> Get(
        string holdId,
        [FromHeader(Name = StudentHeader)] string? studentId,
        CancellationToken ct) =>
        await holds.GetAsync(RouteIds.Parse(holdId, "hold id"), studentId, ct);

    [HttpPost("{holdId}/confirm")]
    [EndpointSummary("Pay admission fee and confirm (idempotent)")]
    [ProducesResponseType<ErrorBody>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorBody>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ErrorBody>(StatusCodes.Status410Gone)]
    public async Task<HoldResponse> Confirm(
        string holdId,
        [FromHeader(Name = StudentHeader)] string? studentId,
        CancellationToken ct) =>
        await holds.ConfirmAsync(RouteIds.Parse(holdId, "hold id"), studentId, ct);

    [HttpDelete("{holdId}")]
    [EndpointSummary("Cancel an application")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorBody>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorBody>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ErrorBody>(StatusCodes.Status410Gone)]
    public async Task<NoContentResult> Cancel(
        string holdId,
        [FromHeader(Name = StudentHeader)] string? studentId,
        CancellationToken ct)
    {
        await holds.CancelAsync(RouteIds.Parse(holdId, "hold id"), studentId, ct);
        return NoContent();
    }
}
