using Admission.Domain.Common;

namespace Admission.Domain.Students;

/// <summary>
/// A student is identified by their 17-digit birth registration number.
/// </summary>
public static class StudentId
{
    public const int Length = 17;

    public static string Parse(string? raw)
    {
        var value = raw?.Trim();
        if (string.IsNullOrEmpty(value))
            throw new DomainException("MISSING_STUDENT_ID",
                "The X-Student-Id header (student birth registration number) is required.");

        if (value.Length != Length || !value.All(char.IsAsciiDigit))
            throw new DomainException("INVALID_STUDENT_ID",
                $"Student id must be a {Length}-digit birth registration number.");

        return value;
    }
}
