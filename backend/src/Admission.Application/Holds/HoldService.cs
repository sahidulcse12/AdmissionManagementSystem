using Admission.Application.Abstractions;
using Admission.Application.Classes;
using Admission.Application.Common;
using Admission.Domain.Holds;
using Admission.Domain.Students;

namespace Admission.Application.Holds;

/// <summary>Use cases for reserving seats (holds) and reading them.</summary>
public sealed class HoldService(
    IClassRepository classes,
    IHoldRepository holds,
    IUnitOfWork unitOfWork,
    HoldOptions options,
    TimeProvider clock)
{
    /// <summary>
    /// Reserves one seat for the student, first-come-first-served.
    /// The class row is locked for the whole transaction, so concurrent requests for the same
    /// class are processed one at a time in arrival order and can never exceed capacity.
    /// </summary>
    public Task<HoldResponse> PlaceAsync(Guid classId, string? rawStudentId, CancellationToken ct)
    {
        var studentId = StudentId.Parse(rawStudentId);

        return unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var schoolClass = await classes.LockAsync(classId, token) ?? throw ClassService.NotFound();
            var now = clock.Now();

            // Seats of overdue holds are already excluded from the count below; this only clears the
            // student's own overdue hold so it no longer blocks them through the unique index.
            await holds.ExpireOverdueForStudentAsync(classId, studentId, now, token);

            var existing = await holds.FindActiveAsync(classId, studentId, token);
            if (existing is not null) throw AlreadyActive(existing.Status);

            var occupied = await holds.CountOccupiedSeatsAsync(classId, now, token);
            if (occupied >= schoolClass.Capacity)
                throw new ConflictException("NO_SEATS_AVAILABLE", "No seats are available in this class.");

            var hold = Hold.Place(classId, studentId, now, options.HoldDuration);
            holds.Add(hold);
            try
            {
                await unitOfWork.SaveChangesAsync(token);
            }
            catch (UniqueConstraintException)
            {
                throw AlreadyActive(HoldStatus.HELD);
            }

            return HoldResponse.From(new HoldWithClass(hold, schoolClass), now);
        }, ct);
    }

    /// <summary>
    /// Confirms admission. The hold row is locked for the transaction, so concurrent confirmations
    /// are serialised: the first one confirms, later ones see CONFIRMED and return the same result.
    /// Expiry is evaluated after the lock is acquired, so a hold cannot expire mid-confirmation.
    /// </summary>
    public Task<HoldResponse> ConfirmAsync(Guid holdId, string? rawStudentId, CancellationToken ct) =>
        TransitionAsync(holdId, rawStudentId, (hold, now) => hold.Confirm(now), ct);

    /// <summary>Cancels a HELD hold and releases its seat. Repeating the call is harmless.</summary>
    public Task<HoldResponse> CancelAsync(Guid holdId, string? rawStudentId, CancellationToken ct) =>
        TransitionAsync(holdId, rawStudentId, (hold, now) => hold.Cancel(now), ct);

    private Task<HoldResponse> TransitionAsync(
        Guid holdId, string? rawStudentId, Func<Hold, DateTimeOffset, bool> transition, CancellationToken ct)
    {
        var studentId = StudentId.Parse(rawStudentId);

        return unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var hold = await holds.LockAsync(holdId, token) ?? throw NotFound();
            EnsureOwner(hold, studentId);

            var now = clock.Now();
            if (transition(hold, now)) await unitOfWork.SaveChangesAsync(token);

            var schoolClass = await classes.GetAsync(hold.ClassId, token) ?? throw ClassService.NotFound();
            return HoldResponse.From(new HoldWithClass(hold, schoolClass), now);
        }, ct);
    }

    public async Task<HoldResponse> GetAsync(Guid holdId, string? rawStudentId, CancellationToken ct)
    {
        var studentId = StudentId.Parse(rawStudentId);
        var item = await holds.GetAsync(holdId, ct) ?? throw NotFound();
        EnsureOwner(item.Hold, studentId);
        return HoldResponse.From(item, clock.Now());
    }

    public async Task<IReadOnlyList<HoldResponse>> ListMineAsync(string? rawStudentId, CancellationToken ct)
    {
        var studentId = StudentId.Parse(rawStudentId);
        var now = clock.Now();
        return (await holds.ListByStudentAsync(studentId, ct)).Select(h => HoldResponse.From(h, now)).ToList();
    }

    /// <summary>Admin view: every hold/admission for one class.</summary>
    public async Task<IReadOnlyList<HoldResponse>> ListForClassAsync(Guid classId, CancellationToken ct)
    {
        _ = await classes.GetAsync(classId, ct) ?? throw ClassService.NotFound();
        var now = clock.Now();
        return (await holds.ListByClassAsync(classId, ct)).Select(h => HoldResponse.From(h, now)).ToList();
    }

    internal static NotFoundException NotFound() => new("HOLD_NOT_FOUND", "Hold not found.");

    internal static void EnsureOwner(Hold hold, string studentId)
    {
        if (!hold.IsOwnedBy(studentId))
            throw new ForbiddenException("HOLD_NOT_OWNED", "This hold belongs to a different student.");
    }

    private static ConflictException AlreadyActive(HoldStatus status) => status == HoldStatus.CONFIRMED
        ? new("ALREADY_ADMITTED", "This student is already admitted to this class.")
        : new("ACTIVE_HOLD_EXISTS", "This student already has an active hold in this class.");
}
