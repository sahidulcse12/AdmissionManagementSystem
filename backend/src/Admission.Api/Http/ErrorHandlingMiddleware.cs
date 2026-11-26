namespace Admission.Api.Http;

/// <summary>Converts exceptions into the uniform JSON error format.</summary>
public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApiException ex)
        {
            await WriteAsync(context, ex.StatusCode, ex.Code, ex.Message);
        }
        catch (BadHttpRequestException ex)
        {
            logger.LogDebug(ex, "Bad request");
            await WriteAsync(context, StatusCodes.Status400BadRequest, "INVALID_REQUEST",
                "The request body is missing or is not valid JSON.");
        }
        catch (Exception ex) when (!context.RequestAborted.IsCancellationRequested)
        {
            logger.LogError(ex, "Unhandled error for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "INTERNAL_ERROR",
                "An unexpected error occurred.");
        }
    }

    private static Task WriteAsync(HttpContext context, int status, string code, string message)
    {
        if (context.Response.HasStarted) return Task.CompletedTask;
        context.Response.Clear();
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(new ErrorBody(new ErrorDetail(code, message)));
    }
}
