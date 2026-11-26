# Progress

## Current status

- **Phase:** Complete — all requirements of the brief implemented and verified; `develop` merged into `main`
- **Backend architecture:** Clean Architecture (`Domain` → `Application` → `Infrastructure` → `Api`)
- **Pending features:** none
- **Commit hashes:** see `git log --oneline --graph --all` (hashes are not copied here so this file
  stays correct if history is ever rewritten)

## Git workflow

`main → develop → feature/* → develop → main`. Every feature branch was created from the latest
`develop`, built in small task-sized commits, and merged back with `--no-ff`.

| # | Branch | What it delivered |
|---|---|---|
| 1 | `feature/project-setup` | .NET and React scaffolds, Docker Compose, `.env.example`, plan |
| 2 | `feature/database-and-class-management` | Clean Architecture layers, EF Core + migration, class API, tests |
| 3 | `feature/seat-availability-and-hold` | Row-locked seat holds, expiry sweeper, hold queries, concurrency tests |
| 4 | `feature/admission-confirmation-and-cancellation` | Idempotent confirm, cancel, ownership checks, race tests |
| — | `develop` (integration fix) | Timestamp precision fix found while re-testing after a merge |
| 5 | `feature/frontend-admin` | API client, admin screens, tests |
| 6 | `feature/frontend-parent` | Parent screens, countdown, history, persistence, tests |
| 7 | `feature/testing-and-documentation` | Docker end-to-end verification, English and Bangla docs |
| 8 | `feature/api-contract-alignment` | Exact API contract (`totalSeats`, `holdId`, `204`), each shift as a class, student ID box at the top, two-backend bonus, `ContractTests`, `MultiInstanceTests` |
| — | `fix/sweeper-graceful-shutdown` | Expiry sweeper stops quietly on shutdown; README explains running locally next to Docker |
| — | `chore/remove-doc-comments` | Removed XML doc / JSDoc comments |
| 9 | `feature/swagger-and-api-versioning` | Swagger UI at `/swagger`, API versioning by header/query (default 1.0), tests |
| 10 | `feature/controllers` | HTTP layer moved to ASP.NET Core controllers (`ClassesController`, `ClassHoldsController`, `HoldsController`, `MeController`); contract unchanged |
| — | `chore/remove-bangla-docs` | Removed the Bangla (`*.bn.md`) copies of the documentation; English docs remain |

## Requirement checklist (brief §1.3–§1.7)

| Requirement | Status | Evidence |
|---|---|---|
| Each shift is a separate class with its own seats | ✅ | `POST /classes { name, totalSeats }` |
| Student ID box at the top of the page; no login | ✅ | `StudentIdBar`, `App.test.tsx` |
| Screens: create class, class list, application, my applications | ✅ | Admin and Parent pages |
| API contract: 8 endpoints, field names, 201/200/204/409 | ✅ | `ContractTests`, manual `curl` on Docker |
| `held + confirmed ≤ totalSeats` under any concurrency | ✅ | `ConcurrencyTests`, 30 and 60 parallel `curl` |
| Hold duration from `HOLD_DURATION_SECONDS` (default 600); auto release | ✅ | Expiry tests; Docker run with 5 s |
| One active application/admission per student per class | ✅ | Partial unique index + tests |
| Only own, unexpired `HELD` holds can be confirmed | ✅ | `403` / `410` / `409` tests |
| Repeated confirm increases `confirmedSeats` once | ✅ | `Parallel_confirmations_of_one_hold_produce_one_admission` |
| Data survives restart and `docker compose down` / `up` | ✅ | Manual check (volume `pgdata`) |
| Bad input → 4xx, never 500 | ✅ | Validation and malformed-body tests |
| Everything doable from the UI | ✅ | Admin + Parent pages |
| Expiry shows EXPIRED and disables "Pay" without reload | ✅ | `ParentPage.test.tsx` |
| Seat counts refresh every few seconds | ✅ | 5 s polling |
| Running application visible after refresh / browser restart | ✅ | `App.test.tsx` |
| Readable errors (no seats, expired, bad input, server down) | ✅ | Frontend tests |
| Bonus: two backend processes on one database | ✅ | `MultiInstanceTests`, `docker compose --profile multi` |
| Bonus: frontend automated tests | ✅ | 21 Vitest tests |
| Extra: Swagger UI and API versioning | ✅ | `/swagger`, `ApiVersioningAndSwaggerTests` |
| README, ASSUMPTIONS, AI_USAGE, Dockerfiles, compose | ✅ | Repository root |
| ≥ 3 backend tests incl. a concurrency test | ✅ | 68 tests |

## Tests executed (latest)

| Command | Result |
|---|---|
| `dotnet test` (Testcontainers PostgreSQL 16) | 68 passed, 0 failed |
| Concurrency + multi-instance tests repeated 3× | 10/10 passed each run |
| `npm test` | 21 passed |
| `npm run lint` / `npm run build` | clean / OK |

Earlier runs: 12 → 31 → 56 backend tests as features landed. After merging feature 4, two tests failed
on `develop` (timestamp precision bug); fixed, then 56 passed three times in a row.

## Docker end-to-end verification

| Check | Result |
|---|---|
| `docker compose --profile multi up --build`: `db`, `api` healthy; `api2`, `web` up | ✅ |
| Migration on a database with old data folds shift into the class name; confirmed admission kept | ✅ |
| All 8 contract endpoints via `curl` | ✅ `201`, `200`, `200`, `201`, `200` (no header), `200`, `204` (empty body), `200` |
| Wrong inputs (`{bad`, `totalSeats:"a"`, empty name, missing seats) | ✅ all `400` |
| 30 parallel `curl` holds for 3 seats (one API) | ✅ 3 × `201`, 27 × `409` |
| 60 parallel `curl` holds for 5 seats split across ports 8080 and 8081 | ✅ 5 × `201`, 55 × `409`; both APIs report 5 held |
| `docker compose down` + `up` | ✅ data still present |
| `HOLD_DURATION_SECONDS=5 EXPIRY_SWEEP_SECONDS=2` | ✅ DB row `EXPIRED` after 8 s; confirm → `410`; seat free again |

## Known issues

- None open. Limitations by design are listed in [README.md](README.md#known-limitations) and
  [ASSUMPTIONS.md](ASSUMPTIONS.md).
