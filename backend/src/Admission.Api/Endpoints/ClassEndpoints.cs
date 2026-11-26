using Admission.Api.Middleware;
using Admission.Application.Classes;
using Admission.Application.Common;

namespace Admission.Api.Endpoints;

public static class ClassEndpoints
{
    public static void MapClassEndpoints(this IEndpointRouteBuilder app)
    {
        var classes = app.MapGroup("/classes").WithTags("Classes");

        classes.MapPost(string.Empty, async (CreateClassRequest? request, ClassService service, CancellationToken ct) =>
        {
            if (request is null) throw new ValidationException("INVALID_REQUEST", "Request body is required.");
            var created = await service.CreateAsync(request, ct);
            return Results.Created($"/classes/{created.Id}", created);
        })
        .WithSummary("Create a class (each shift is its own class)")
        .Produces<ClassResponse>(StatusCodes.Status201Created)
        .Produces<ErrorBody>(StatusCodes.Status400BadRequest)
        .Produces<ErrorBody>(StatusCodes.Status409Conflict);

        classes.MapGet(string.Empty, async (ClassService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(ct)))
        .WithSummary("List all classes with seat counts")
        .Produces<IReadOnlyList<ClassResponse>>();

        classes.MapGet("/{classId}", async (string classId, ClassService service, CancellationToken ct) =>
            Results.Ok(await service.GetAsync(RouteIds.Parse(classId, "class id"), ct)))
        .WithSummary("Seat status of one class")
        .Produces<ClassResponse>()
        .Produces<ErrorBody>(StatusCodes.Status400BadRequest)
        .Produces<ErrorBody>(StatusCodes.Status404NotFound);
    }
}
