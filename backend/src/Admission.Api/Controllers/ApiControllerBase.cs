using Admission.Api.Middleware;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Admission.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Produces("application/json")]
[ProducesResponseType<ErrorBody>(StatusCodes.Status400BadRequest)]
public abstract class ApiControllerBase : ControllerBase
{
    public const string StudentHeader = "X-Student-Id";
}
