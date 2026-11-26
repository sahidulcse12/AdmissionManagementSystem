using Admission.Api.Middleware;
using Admission.Application.Classes;
using Admission.Application.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Admission.Api.Controllers;

[Route("classes")]
[Tags("Classes")]
public sealed class ClassesController(ClassService classes) : ApiControllerBase
{
    [HttpPost]
    [EndpointSummary("Create a class (each shift is its own class)")]
    [ProducesResponseType<ClassResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorBody>(StatusCodes.Status409Conflict)]
    public async Task<Created<ClassResponse>> Create([FromBody] CreateClassRequest? request, CancellationToken ct)
    {
        if (request is null) throw new ValidationException("INVALID_REQUEST", "Request body is required.");
        var created = await classes.CreateAsync(request, ct);
        return TypedResults.Created($"/classes/{created.Id}", created);
    }

    [HttpGet]
    [EndpointSummary("List all classes with seat counts")]
    public async Task<IReadOnlyList<ClassResponse>> List(CancellationToken ct) =>
        await classes.ListAsync(ct);

    [HttpGet("{classId}")]
    [EndpointSummary("Seat status of one class")]
    [ProducesResponseType<ErrorBody>(StatusCodes.Status404NotFound)]
    public async Task<ClassResponse> Get(string classId, CancellationToken ct) =>
        await classes.GetAsync(RouteIds.Parse(classId, "class id"), ct);
}
