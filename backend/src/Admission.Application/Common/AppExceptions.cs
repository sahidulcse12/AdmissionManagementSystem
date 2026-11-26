namespace Admission.Application.Common;

/// <summary>Base type for expected application failures that carry a machine-readable code.</summary>
public abstract class AppException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

public sealed class ValidationException(string code, string message) : AppException(code, message);

public sealed class NotFoundException(string code, string message) : AppException(code, message);

public sealed class ForbiddenException(string code, string message) : AppException(code, message);

public sealed class ConflictException(string code, string message) : AppException(code, message);

/// <summary>
/// Raised by the persistence layer when a database unique constraint rejects a write.
/// Use cases translate it into a meaningful <see cref="ConflictException"/>.
/// </summary>
public sealed class UniqueConstraintException(string? constraintName, Exception inner)
    : Exception($"Unique constraint '{constraintName}' violated.", inner)
{
    public string? ConstraintName { get; } = constraintName;
}
