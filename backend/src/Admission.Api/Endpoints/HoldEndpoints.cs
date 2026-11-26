using Admission.Api.Middleware;
using Admission.Application.Holds;
using Microsoft.AspNetCore.Mvc;

namespace Admission.Api.Endpoints;

public static class HoldEndpoints
{
    public const string StudentHeader = "X-Student-Id";

    public static void MapHoldEndpoints(this IEndpointRouteBuilder app)
    {
        var holds = app.MapGroup(string.Empty).WithTags("Applications (holds)");

        holds.MapPost("/classes/{classId}/holds", async (
            string classId,
            [FromHeader(Name = StudentHeader)] string? studentId,
            HoldService service,
            CancellationToken ct) =>
        {
            var hold = await service.PlaceAsync(RouteIds.Parse(classId, "class id"), studentId, ct);
            return Results.Created($"/holds/{hold.HoldId}", hold);
        })
        .WithSummary("Apply for a seat (hold it for HOLD_DURATION_SECONDS)")
        .Produces<HoldResponse>(StatusCodes.Status201Created)
        .Produces<ErrorBody>(StatusCodes.Status400BadRequest)
        .Produces<ErrorBody>(StatusCodes.Status404NotFound)
        .Produces<ErrorBody>(StatusCodes.Status409Conflict);

        holds.MapGet("/classes/{classId}/holds", async (string classId, HoldService service, CancellationToken ct) =>
            Results.Ok(await service.ListForClassAsync(RouteIds.Parse(classId, "class id"), ct)))
        .WithSummary("Admin: all applications of a class")
        .Produces<IReadOnlyList<HoldResponse>>()
        .Produces<ErrorBody>(StatusCodes.Status404NotFound);

        holds.MapGet("/holds/{holdId}", async (
            string holdId,
            [FromHeader(Name = StudentHeader)] string? studentId,
            HoldService service,
            CancellationToken ct) =>
            Results.Ok(await service.GetAsync(RouteIds.Parse(holdId, "hold id"), studentId, ct)))
        .WithSummary("Current status of an application (X-Student-Id optional)")
        .Produces<HoldResponse>()
        .Produces<ErrorBody>(StatusCodes.Status403Forbidden)
        .Produces<ErrorBody>(StatusCodes.Status404NotFound);

        holds.MapPost("/holds/{holdId}/confirm", async (
            string holdId,
            [FromHeader(Name = StudentHeader)] string? studentId,
            HoldService service,
            CancellationToken ct) =>
            Results.Ok(await service.ConfirmAsync(RouteIds.Parse(holdId, "hold id"), studentId, ct)))
        .WithSummary("Pay admission fee and confirm (idempotent)")
        .Produces<HoldResponse>()
        .Produces<ErrorBody>(StatusCodes.Status403Forbidden)
        .Produces<ErrorBody>(StatusCodes.Status409Conflict)
        .Produces<ErrorBody>(StatusCodes.Status410Gone);

        holds.MapDelete("/holds/{holdId}", async (
            string holdId,
            [FromHeader(Name = StudentHeader)] string? studentId,
            HoldService service,
            CancellationToken ct) =>
        {
            await service.CancelAsync(RouteIds.Parse(holdId, "hold id"), studentId, ct);
            return Results.NoContent();
        })
        .WithSummary("Cancel an application")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ErrorBody>(StatusCodes.Status403Forbidden)
        .Produces<ErrorBody>(StatusCodes.Status409Conflict)
        .Produces<ErrorBody>(StatusCodes.Status410Gone);

        holds.MapGet("/me/holds", async (
            [FromHeader(Name = StudentHeader)] string? studentId,
            HoldService service,
            CancellationToken ct) =>
            Results.Ok(await service.ListMineAsync(studentId, ct)))
        .WithSummary("All applications of this student")
        .Produces<IReadOnlyList<HoldResponse>>()
        .Produces<ErrorBody>(StatusCodes.Status400BadRequest);
    }
}
