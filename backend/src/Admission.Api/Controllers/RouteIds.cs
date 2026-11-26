using Admission.Application.Common;

namespace Admission.Api.Controllers;

public static class RouteIds
{
    // Malformed ids return 400 instead of a silent 404.
    public static Guid Parse(string raw, string name) =>
        Guid.TryParse(raw, out var id)
            ? id
            : throw new ValidationException("INVALID_ID", $"'{raw}' is not a valid {name}.");
}
