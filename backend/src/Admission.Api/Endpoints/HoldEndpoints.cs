using Admission.Application.Holds;
using Microsoft.AspNetCore.Mvc;

namespace Admission.Api.Endpoints;

public static class HoldEndpoints
{
    /// <summary>Header carrying the student's birth registration number (no auth system).</summary>
    public const string StudentHeader = "X-Student-Id";

    public static void MapHoldEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/classes/{classId}/holds", async (
            string classId,
            [FromHeader(Name = StudentHeader)] string? studentId,
            HoldService service,
            CancellationToken ct) =>
        {
            var hold = await service.PlaceAsync(RouteIds.Parse(classId, "class id"), studentId, ct);
            return Results.Created($"/holds/{hold.Id}", hold);
        });

        app.MapGet("/classes/{classId}/holds", async (string classId, HoldService service, CancellationToken ct) =>
            Results.Ok(await service.ListForClassAsync(RouteIds.Parse(classId, "class id"), ct)));

        app.MapGet("/holds/{holdId}", async (
            string holdId,
            [FromHeader(Name = StudentHeader)] string? studentId,
            HoldService service,
            CancellationToken ct) =>
            Results.Ok(await service.GetAsync(RouteIds.Parse(holdId, "hold id"), studentId, ct)));

        app.MapGet("/me/holds", async (
            [FromHeader(Name = StudentHeader)] string? studentId,
            HoldService service,
            CancellationToken ct) =>
            Results.Ok(await service.ListMineAsync(studentId, ct)));
    }
}
