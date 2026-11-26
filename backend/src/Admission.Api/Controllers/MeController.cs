using Admission.Application.Holds;
using Microsoft.AspNetCore.Mvc;

namespace Admission.Api.Controllers;

[Route("me")]
[Tags("Applications (holds)")]
public sealed class MeController(HoldService holds) : ApiControllerBase
{
    [HttpGet("holds")]
    [EndpointSummary("All applications of this student")]
    public async Task<IReadOnlyList<HoldResponse>> MyHolds(
        [FromHeader(Name = StudentHeader)] string? studentId,
        CancellationToken ct) =>
        await holds.ListMineAsync(studentId, ct);
}
