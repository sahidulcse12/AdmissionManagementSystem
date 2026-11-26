using Admission.Application.Common;

namespace Admission.Api.Endpoints;

public static class RouteIds
{
    public static Guid Parse(string raw, string name) =>
        Guid.TryParse(raw, out var id)
            ? id
            : throw new ValidationException("INVALID_ID", $"'{raw}' is not a valid {name}.");
}
