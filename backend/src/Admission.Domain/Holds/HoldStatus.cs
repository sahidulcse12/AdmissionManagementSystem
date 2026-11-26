namespace Admission.Domain.Holds;

/// <summary>
/// Lifecycle of a seat hold:
/// <code>
/// HELD ──confirm──▶ CONFIRMED
///  │ ├──cancel───▶ CANCELLED
///  │ └──time─────▶ EXPIRED
/// </code>
/// CONFIRMED, CANCELLED and EXPIRED are terminal.
/// </summary>
public enum HoldStatus
{
    HELD,
    CONFIRMED,
    EXPIRED,
    CANCELLED,
}
