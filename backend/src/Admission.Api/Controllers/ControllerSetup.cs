using System.Text.Json.Serialization;
using Admission.Api.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Admission.Api.Controllers;

public static class ControllerSetup
{
    public static IServiceCollection AddAdmissionControllers(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .ConfigureApiBehaviorOptions(o =>
            {
                // [ApiController] rejects unreadable bodies (bad JSON, wrong types, empty body) before the
                // action runs. Answer with the same error shape as every other failure instead of ProblemDetails.
                o.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(new ErrorBody(new ErrorDetail(
                    "INVALID_REQUEST", "The request is malformed or the body is not valid JSON.")));
            });
        return services;
    }
}
