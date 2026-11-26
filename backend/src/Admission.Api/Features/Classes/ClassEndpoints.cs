using Admission.Api.Http;

namespace Admission.Api.Features.Classes;

public static class ClassEndpoints
{
    public static void MapClassEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/classes", async (CreateClassRequest? request, ClassService service, CancellationToken ct) =>
        {
            if (request is null) throw ApiException.BadRequest("INVALID_REQUEST", "Request body is required.");
            var created = await service.CreateAsync(request, ct);
            return Results.Created($"/classes/{created.Id}", created);
        });

        app.MapGet("/classes", async (ClassService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(ct)));

        app.MapGet("/classes/{classId}", async (string classId, ClassService service, CancellationToken ct) =>
            Results.Ok(await service.GetAsync(RouteIds.Parse(classId, "class id"), ct)));
    }
}
