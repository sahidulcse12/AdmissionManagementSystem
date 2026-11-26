namespace Admission.Api.Http;

public static class RouteIds
{
    /// <summary>Parses a route identifier, returning 400 instead of a silent 404 for malformed ids.</summary>
    public static Guid Parse(string raw, string name) =>
        Guid.TryParse(raw, out var id)
            ? id
            : throw ApiException.BadRequest("INVALID_ID", $"'{raw}' is not a valid {name}.");
}
