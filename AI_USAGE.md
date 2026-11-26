# AI Usage

> Bangla version: [AI_USAGE.bn.md](AI_USAGE.bn.md)

## Tool and model

- **Tool:** Claude Code (Anthropic's agentic coding assistant), used from the IDE.
- **Model:** Claude Opus 5.5.

## Where AI was used

| Area | What the AI did | What I did |
|---|---|---|
| Planning | Proposed the feature-branch plan and priorities | Chose the Git-flow workflow, asked for small commits per task |
| Backend | Wrote Domain / Application / Infrastructure / Api layers, EF Core config, migrations | Required Clean Architecture; reviewed the locking design |
| Concurrency | Proposed per-class `SELECT … FOR UPDATE`, hold row locks, partial unique index | Reviewed and verified with race tests and real `curl` races |
| Frontend | Wrote the React/TypeScript screens, API client, polling and countdown | Checked every screen against the brief |
| Tests | Wrote 68 backend tests (incl. concurrency and two-instance tests) and 21 frontend tests | Ran them repeatedly, checked failures |
| Docker / docs | Wrote Dockerfiles, compose file, README and other docs | Verified `docker compose up`, persistence and expiry by hand |

## Where the AI was wrong, and how I caught it

1. **API contract did not match the brief.** The first version used `capacity` + a separate `shift`
   field instead of `totalSeats`, returned `id` instead of `holdId`, returned `200` with a body on
   `DELETE` instead of `204`, and required `X-Student-Id` on `GET /holds/{holdId}`.
   *How caught:* I compared the implementation line by line with the assignment's API section before
   submitting. *Fix:* each shift became its own class with `{ name, totalSeats }`, response fields were
   renamed, `DELETE` returns `204`, and `ContractTests` now assert the exact field names and status codes.
   A migration folds existing shift values into the class name so no data is lost.
2. **Timestamp precision bug.** Confirming the same hold twice returned two slightly different
   `confirmedAt` values (100 ns .NET precision right after the write vs. microsecond precision read back
   from PostgreSQL), which broke idempotent confirmation.
   *How caught:* the test suite failed after merging to `develop` even though it had passed on the
   feature branch — the AI had claimed the feature was done. *Fix:* all times are truncated to
   microseconds; the suite was re-run several times.
3. **Possible deadlock.** The first hold-creation code bulk-expired all overdue holds of a class, which
   could deadlock with the background sweeper doing the same.
   *How caught:* reviewing the locking order while writing the sweeper. *Fix:* hold creation only
   touches the requesting student's own overdue hold (at most one row).
4. **Small slips:** a LINQ ordering EF Core could not translate (500 in tests), guessed commit hashes in
   `PROGRESS.md`, and a countdown that a stale server response could restart after expiry.
   All were caught by tests or by re-checking, and fixed.

## Takeaway

AI output was never accepted on trust: every feature was built, tested, and checked against the brief,
and the concurrency guarantees were verified both by automated tests and by firing real parallel
requests at the running Docker stack.
