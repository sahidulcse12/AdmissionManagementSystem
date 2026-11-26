using Admission.Domain.Common;
using Admission.Domain.Holds;
using Admission.Domain.Students;

namespace Admission.Tests.Domain;

/// <summary>Pure unit tests of the hold state machine (no database).</summary>
public sealed class HoldTests
{
    private static readonly DateTimeOffset T0 = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Duration = TimeSpan.FromMinutes(10);

    private static Hold NewHold() => Hold.Place(Guid.NewGuid(), "12345678901234567", T0, Duration);

    [Fact]
    public void New_hold_is_held_until_its_deadline()
    {
        var hold = NewHold();

        Assert.Equal(HoldStatus.HELD, hold.StatusAt(T0 + Duration - TimeSpan.FromTicks(1)));
        Assert.Equal(HoldStatus.EXPIRED, hold.StatusAt(T0 + Duration));
        Assert.True(hold.OccupiesSeatAt(T0));
        Assert.False(hold.OccupiesSeatAt(T0 + Duration));
    }

    [Fact]
    public void Confirm_transitions_once_and_then_is_a_no_op()
    {
        var hold = NewHold();

        Assert.True(hold.Confirm(T0.AddMinutes(1)));
        Assert.False(hold.Confirm(T0.AddMinutes(2)));
        Assert.Equal(HoldStatus.CONFIRMED, hold.StatusAt(T0.AddDays(1)));
        Assert.Equal(T0.AddMinutes(1), hold.ConfirmedAt);
    }

    [Fact]
    public void Confirm_after_deadline_is_rejected_as_gone()
    {
        var ex = Assert.Throws<DomainException>(() => NewHold().Confirm(T0 + Duration));
        Assert.Equal(("HOLD_EXPIRED", DomainErrorKind.Gone), (ex.Code, ex.Kind));
    }

    [Fact]
    public void Cancelled_hold_cannot_be_confirmed()
    {
        var hold = NewHold();
        hold.Cancel(T0);

        var ex = Assert.Throws<DomainException>(() => hold.Confirm(T0));
        Assert.Equal(("HOLD_CANCELLED", DomainErrorKind.Conflict), (ex.Code, ex.Kind));
    }

    [Fact]
    public void Cancel_is_idempotent_but_not_allowed_after_confirmation()
    {
        var cancelled = NewHold();
        Assert.True(cancelled.Cancel(T0));
        Assert.False(cancelled.Cancel(T0.AddSeconds(1)));

        var confirmed = NewHold();
        confirmed.Confirm(T0);
        Assert.Equal("HOLD_ALREADY_CONFIRMED", Assert.Throws<DomainException>(() => confirmed.Cancel(T0)).Code);
    }

    [Theory]
    [InlineData("12345678901234567", true)]
    [InlineData(" 12345678901234567 ", true)]
    [InlineData("1234567890123456", false)]
    [InlineData("1234567890123456a", false)]
    [InlineData("١٢٣٤٥٦٧٨٩٠١٢٣٤٥٦٧", false)] // non-ASCII digits
    public void Student_id_must_be_17_ascii_digits(string raw, bool valid)
    {
        if (valid) Assert.Equal(raw.Trim(), StudentId.Parse(raw));
        else Assert.Throws<DomainException>(() => StudentId.Parse(raw));
    }
}
