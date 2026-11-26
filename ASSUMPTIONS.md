# Assumptions

> Bangla version: [ASSUMPTIONS.bn.md](ASSUMPTIONS.bn.md)

What I assumed where the brief was not explicit, and why.

## Classes and shifts

1. **Each shift is a separate class** with its own name and `totalSeats`
   (e.g. "Class 6 - Morning Shift", "Class 6 - Day Shift"), exactly as the brief describes. There is no
   separate shift field. *Why:* keeps the API contract (`{ name, totalSeats }`) unchanged.
2. **Class names are unique, case-insensitively.** Creating "class 6 - morning shift" when
   "Class 6 - Morning Shift" exists returns `409`. *Why:* two classes with the same name would confuse parents.
3. **`totalSeats` is a whole number from 1 to 10 000.** *Why:* zero or negative seats make no sense, and an
   upper bound rejects typos such as `1200000`.
4. **Classes cannot be edited or deleted.** Not required by the brief, and changing seats after admissions
   started raises policy questions (what if seats drop below confirmed admissions?).

## Identity

5. **Student ID = exactly 17 ASCII digits** (Bangladesh birth registration number). Spaces around it are
   trimmed; anything else is `400`.
6. **A missing or malformed `X-Student-Id` is `400`, not `401`.** There is no login, so there is nothing to
   authenticate against.
7. **`GET /holds/{holdId}` works without `X-Student-Id`.** The contract lists the header for every other
   student endpoint but not this one, so a request without it is answered. If the header *is* sent and
   belongs to another student, the answer is `403`. Confirm, cancel and `/me/holds` always require it.
8. **Admin actions are open** (creating classes, viewing a class's applications). The brief has no admin login.

## Applications (holds)

9. **One active application or admission per student per class** (as required). The same student may apply
   to different classes, e.g. Morning and Day shift. *Why:* the brief only restricts "a class".
10. **A hold is valid while `now < expiresAt`**; at `expiresAt` it is `EXPIRED`.
11. **Expired seats are released without any API call.** Seat counts treat an overdue `HELD` hold as free
    immediately; a background job also writes `EXPIRED` to the database every `EXPIRY_SWEEP_SECONDS` (15 s).
12. **A confirmed admission cannot be cancelled** (`409`). *Why:* the fee has been paid; withdrawal would
    be an office process, not a button.
13. **Cancelling an already-cancelled hold returns `204` again**, mirroring idempotent confirmation.
    Cancelling an expired hold returns `410` because its seat is already free.
14. **Confirming an expired hold returns `410 Gone`; a cancelled one returns `409`.** Both are 4xx as required.
15. **"First come, first served"** means the order in which requests obtain the class row lock in PostgreSQL,
    i.e. arrival order at the database.
16. **Payment always succeeds** — "Pay admission fee" calls confirm directly.

## Bonus: two backend processes on one database

Running two backend processes (e.g. ports 8080 and 8081) on the same PostgreSQL still keeps
`held + confirmed ≤ totalSeats`. Nothing relies on in-memory state; every rule is enforced by the database:

- **Seat check and insert are serialised per class in PostgreSQL.** A new hold runs in a transaction that
  first takes `SELECT … FROM classes WHERE id = $1 FOR UPDATE`. A second process asking for the same class
  waits on that row lock until the first commits, then counts the seats again. The lock lives in the
  database, so it works across any number of processes.
- **Partial unique index** `(class_id, student_id) WHERE status IN ('HELD','CONFIRMED')` makes a second
  active application for the same student impossible even if two processes race.
- **Confirm/cancel lock the hold row** (`SELECT … FOR UPDATE`), so confirming the same hold through two
  processes counts it once.
- **Seat counts are computed from the `holds` table on every request**, never cached per process.
- **Migrations** run at startup with retries, so two processes starting together converge safely.

Verified by `MultiInstanceTests` (two separate hosts, one database, 60 parallel requests for 7 seats →
exactly 7 holds) and manually with `docker compose --profile multi up` (API on 8080 and 8081).

## Technical

17. **.NET 9** (the brief says .NET 8 or newer).
18. **Timestamps are truncated to microseconds** to match PostgreSQL, so a response right after a write equals
    the same record read later.
19. **UI refreshes by polling every 5 seconds** (only while the tab is visible). *Why:* simpler than
    WebSockets and enough for "updates every few seconds".
20. **The student ID is remembered in `localStorage`**; running applications are always re-read from
    `GET /me/holds`, so the backend stays the source of truth after a refresh or browser restart.
21. **UI labels are English with the brief's Bangla terms** ("আবেদন করুন", "ভর্তি ফি দিন", "বাতিল").
