using Admission.Application.Common;

namespace Admission.Api.Endpoints;

public static class RouteIds
{
    /// <summary>Parses a route identifier, returning 400 instead of a silent 404 for malformed ids.</summary>
    public static Guid Parse(string raw, string name) =>
        Guid.TryParse(raw, out var id)
            ? id
            : throw new ValidationException("INVALID_ID", $"'{raw}' is not a valid {name}.");
}
