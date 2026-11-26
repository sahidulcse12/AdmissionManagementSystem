using Admission.Api.Middleware;
using Admission.Application.Holds;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Admission.Api.Controllers;

[Route("classes/{classId}/holds")]
[Tags("Applications (holds)")]
public sealed class ClassHoldsController(HoldService holds) : ApiControllerBase
{
    [HttpPost]
    [EndpointSummary("Apply for a seat (hold it for HOLD_DURATION_SECONDS)")]
    [ProducesResponseType<HoldResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorBody>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorBody>(StatusCodes.Status409Conflict)]
    public async Task<Created<HoldResponse>> Apply(
        string classId,
        [FromHeader(Name = StudentHeader)] string? studentId,
        CancellationToken ct)
    {
        var hold = await holds.PlaceAsync(RouteIds.Parse(classId, "class id"), studentId, ct);
        return TypedResults.Created($"/holds/{hold.HoldId}", hold);
    }

    [HttpGet]
    [EndpointSummary("Admin: all applications of a class")]
    [ProducesResponseType<ErrorBody>(StatusCodes.Status404NotFound)]
    public async Task<IReadOnlyList<HoldResponse>> List(string classId, CancellationToken ct) =>
        await holds.ListForClassAsync(RouteIds.Parse(classId, "class id"), ct);
}
