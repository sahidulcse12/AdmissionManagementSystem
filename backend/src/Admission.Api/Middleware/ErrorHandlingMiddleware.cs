using Admission.Application.Common;
using Admission.Domain.Common;

namespace Admission.Api.Middleware;

public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (!context.RequestAborted.IsCancellationRequested)
        {
            var (status, code, message) = Map(ex);
            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(ex, "Unhandled error for {Method} {Path}", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted) throw;
            context.Response.Clear();
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ErrorBody(new ErrorDetail(code, message)));
        }
    }

    private static (int Status, string Code, string Message) Map(Exception ex) => ex switch
    {
        DomainException d => (d.Kind switch
        {
            DomainErrorKind.Conflict => StatusCodes.Status409Conflict,
            DomainErrorKind.Gone => StatusCodes.Status410Gone,
            _ => StatusCodes.Status400BadRequest,
        }, d.Code, d.Message),
        ValidationException v => (StatusCodes.Status400BadRequest, v.Code, v.Message),
        NotFoundException n => (StatusCodes.Status404NotFound, n.Code, n.Message),
        ForbiddenException f => (StatusCodes.Status403Forbidden, f.Code, f.Message),
        ConflictException c => (StatusCodes.Status409Conflict, c.Code, c.Message),
        BadHttpRequestException => (StatusCodes.Status400BadRequest, "INVALID_REQUEST",
            "The request is malformed or the body is not valid JSON."),
        _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred."),
    };
}

public sealed record ErrorBody(ErrorDetail Error);

public sealed record ErrorDetail(string Code, string Message);
