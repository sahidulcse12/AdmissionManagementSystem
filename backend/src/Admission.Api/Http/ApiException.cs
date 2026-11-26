namespace Admission.Api.Http;

/// <summary>An expected failure that maps directly to an HTTP error response.</summary>
public sealed class ApiException(int statusCode, string code, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Code { get; } = code;

    public static ApiException BadRequest(string code, string message) => new(StatusCodes.Status400BadRequest, code, message);
    public static ApiException NotFound(string code, string message) => new(StatusCodes.Status404NotFound, code, message);
    public static ApiException Forbidden(string code, string message) => new(StatusCodes.Status403Forbidden, code, message);
    public static ApiException Conflict(string code, string message) => new(StatusCodes.Status409Conflict, code, message);
    public static ApiException Gone(string code, string message) => new(StatusCodes.Status410Gone, code, message);
}

/// <summary>Uniform error body: <c>{ "error": { "code": "...", "message": "..." } }</c>.</summary>
public sealed record ErrorBody(ErrorDetail Error);

public sealed record ErrorDetail(string Code, string Message);
