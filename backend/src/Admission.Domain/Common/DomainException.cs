namespace Admission.Domain.Common;

/// <summary>Category of a broken business rule; the API layer maps it to an HTTP status.</summary>
public enum DomainErrorKind
{
    /// <summary>Input violates an invariant (e.g. capacity out of range).</summary>
    Validation,

    /// <summary>The operation conflicts with the current state (e.g. confirming a cancelled hold).</summary>
    Conflict,

    /// <summary>The resource existed but is no longer usable (e.g. an expired hold).</summary>
    Gone,
}

/// <summary>Raised when a domain rule is violated.</summary>
public sealed class DomainException(string code, string message, DomainErrorKind kind = DomainErrorKind.Validation)
    : Exception(message)
{
    public string Code { get; } = code;
    public DomainErrorKind Kind { get; } = kind;
}
