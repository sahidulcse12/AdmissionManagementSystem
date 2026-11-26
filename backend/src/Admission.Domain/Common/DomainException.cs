namespace Admission.Domain.Common;

public enum DomainErrorKind
{
    Validation,

    Conflict,

    Gone,
}

public sealed class DomainException(string code, string message, DomainErrorKind kind = DomainErrorKind.Validation)
    : Exception(message)
{
    public string Code { get; } = code;
    public DomainErrorKind Kind { get; } = kind;
}
