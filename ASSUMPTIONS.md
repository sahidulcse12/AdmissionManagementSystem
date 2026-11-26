# Assumptions

> Bangla version: [ASSUMPTIONS.bn.md](ASSUMPTIONS.bn.md)

Decisions made where the requirements left room for interpretation.

## Identity and access

1. **Student id = 17-digit birth registration number.** Bangladesh birth registration numbers are
   17 digits; the API accepts exactly 17 ASCII digits (surrounding spaces are trimmed).
2. **No authentication**, as required. `X-Student-Id` is trusted as the caller's identity; ownership
   checks compare it to the hold's student id on the backend.
3. **Admin endpoints are open.** `POST /classes` and `GET /classes/{id}/holds` have no protection.
   In production they would sit behind admin authentication.
4. A missing or malformed `X-Student-Id` returns `400` (not `401`), because there is no
   authentication scheme to challenge.

## Admission rules

5. **One active hold or admission per student per class.** The same student may hold seats in
   different classes at the same time (the requirement only restricts the same class).
6. **A confirmed admission cannot be cancelled** through `DELETE /holds/{id}` (`409`). Withdrawal
   after admission is an administrative process outside this scope.
7. **An expired hold cannot be cancelled** (`410`): its seat is already free.
8. **Repeated cancel is a no-op** (`200`), mirroring idempotent confirmation.
9. **Payment always succeeds.** "Pay & confirm" calls `POST /holds/{id}/confirm` directly.
10. **Hold boundary:** a hold is valid while `now < expires_at`; at `expires_at` it is expired.
11. **First-come-first-served** means the order in which requests acquire the class row lock in
    PostgreSQL, i.e. arrival order at the database.

## Classes

12. **Shifts** are `MORNING`, `DAY` and `EVENING`, common in Bangladeshi schools.
13. Class names are unique **per shift, case-insensitively** ("Class 1" MORNING and "class 1" MORNING
    conflict; "Class 1" DAY is allowed).
14. **Capacity** is 1–1000 and is set when the class is created. Editing capacity or deleting classes
    was not required and is not implemented.

## Technical

15. **.NET 9** is used (the requirement says .NET 8 or newer).
16. **Seat counts are computed, not stored**, so they can never drift from the holds table.
17. **Background sweep interval** defaults to 15 s (`EXPIRY_SWEEP_SECONDS`). Correctness does not
    depend on it; it only keeps stored statuses tidy.
18. **Timestamps** are truncated to microseconds to match PostgreSQL precision.
19. **CORS** defaults to `*` for local use; set `CORS_ORIGINS` to restrict it.
20. **UI refresh** is polling every 5 seconds (only while the tab is visible) rather than WebSockets —
    simpler and sufficient for this scale.
21. The UI is in English; the documentation is in English and Bangla.
