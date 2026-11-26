# Progress

> Bangla version: [PROGRESS.bn.md](PROGRESS.bn.md)

## Current status

- **Current phase:** All features complete; final verification on `develop`, then merge into `main`
- **Current branch:** `feature/testing-and-documentation` → merging into `develop`
- **Backend architecture:** Clean Architecture (`Domain` → `Application` → `Infrastructure` → `Api`)
- **Pending features:** none
- **Next action:** none — final merge hashes are listed under *Final integration* below and in `git log`

## Feature branches

All feature branches were created from the latest `develop` and merged back with `--no-ff`.

| # | Branch | Status | Feature commits | Merge into `develop` |
|---|---|---|---|---|
| 1 | `feature/project-setup` | Done | `03a3853`, `1c104b5`, `0ebe29b`, `3585ae0`, `b032521` | `b32c602` |
| 2 | `feature/database-and-class-management` | Done | `ba02a60`, `e3ff39c`, `f5f9134`, `d507856`, `97128ac` | `2af83aa` |
| 3 | `feature/seat-availability-and-hold` | Done | `a748e5c`, `3937cbd`, `cac6b08`, `050f21e`, `cf7ba54`, `7ccc2ac` | `51af863` |
| 4 | `feature/admission-confirmation-and-cancellation` | Done | `6878a9a`, `0e2240d`, `c454a1d`, `0502091` | `5251875` |
| — | `develop` (integration fix) | Done | `40591d2` fix: truncate timestamps to PostgreSQL precision | direct on `develop` |
| 5 | `feature/frontend-admin` | Done | `258daee`, `ff3a7e5`, `56b3e9a`, progress commit | `804d9f5` |
| 6 | `feature/frontend-parent` | Done | `3bbb3b4`, `39ba60e`, `f5c445c` | `106423d` |
| 7 | `feature/testing-and-documentation` | Done | `01ca03b`, `41b3b77`, `269b7fd`, `8ed0691`, `5fda5bd`, this progress commit | see *Final integration* |

Base commit on `main`: `41838aa` (initial repository setup).

## Completed features

- Class management with shift and capacity (`POST/GET /classes`, `GET /classes/{id}`)
- First-come-first-served seat holds with configurable duration (`POST /classes/{id}/holds`)
- Hold expiry (read-time + background sweeper), hold reads and history (`GET /holds/{id}`, `GET /me/holds`)
- Idempotent confirmation and cancellation with ownership checks (`POST /holds/{id}/confirm`, `DELETE /holds/{id}`)
- Admin view of a class's holds (`GET /classes/{id}/holds`)
- Admin UI and parent/guardian UI with live seats, countdown, history and persistence across reloads
- Docker Compose stack with PostgreSQL volume, health checks and automatic migrations
- English and Bangla documentation

## Tests executed

| When | Command | Result |
|---|---|---|
| Project setup | `dotnet build` (backend) | Passed, 0 errors |
| Project setup | `npm run build` (frontend) | Passed |
| Project setup | `docker compose config -q` | Valid |
| Class management | `dotnet test` (Testcontainers PostgreSQL 16) | 12 passed, 0 failed |
| Seat holds | `dotnet test` | 31 passed, 0 failed |
| Seat holds | `ConcurrencyTests` repeated 3× | 4/4 passed each run |
| Seat holds (on `develop` after merge) | `dotnet test` | 31 passed, 0 failed |
| Confirmation & cancellation | `dotnet test` | 56 passed, 0 failed |
| After merge on `develop` | `dotnet test` | 2 failed (timestamp precision bug) → fixed in `40591d2`, then 56 passed ×3 |
| Admin interface | `npm test`, `npm run lint`, `npm run build` | 6 passed, lint clean, build OK |
| Parent interface | `npm test`, `npm run lint`, `npm run build` | 19 passed, lint clean, build OK |

## Docker end-to-end verification (`docker compose up --build`)

| Check | Result |
|---|---|
| `db`, `api` healthy; `web` up | ✅ |
| Migrations applied on startup (`InitialClasses`, `AddHolds`) | ✅ |
| Create class → hold → confirm via HTTP | ✅ `201`, `HELD` (600 s), `CONFIRMED` |
| Duplicate hold by same student | ✅ `409` |
| Other student confirms someone's hold | ✅ `403 HOLD_NOT_OWNED` |
| Confirm twice | ✅ `200`, identical `confirmedAt` |
| Missing `X-Student-Id` / malformed id | ✅ `400 MISSING_STUDENT_ID` / `400 INVALID_ID` |
| 30 parallel `curl` holds for 3 seats | ✅ 3 × `201`, 27 × `409`; 3 active rows in DB |
| `docker compose down` + `up` | ✅ classes and confirmed admission still present (volume persisted) |
| `HOLD_DURATION_SECONDS=5 EXPIRY_SWEEP_SECONDS=2` | ✅ hold `remainingSeconds: 5`; DB row `EXPIRED` after 8 s; confirm → `410`; seat available again |
| Frontend served, `config.js` generated from `API_URL`, SPA fallback | ✅ |
| CORS preflight allows `X-Student-Id` | ✅ |

## Final integration

Recorded after the last feature merge — see [Final integration on `develop`](#final-integration-on-develop) below.

## Known issues

- None open. Resolved: responses returned right after a write had 100 ns precision while
  later reads had PostgreSQL's microsecond precision, breaking idempotent confirm responses (`40591d2`).
- Limitations by design are listed in [README.md](README.md#known-limitations) and [ASSUMPTIONS.md](ASSUMPTIONS.md).

## Final integration on `develop`

| Step | Result |
|---|---|
| `feature/testing-and-documentation` → `develop` | merge `ba959eb` |
| Backend `dotnet build` / `dotnet test` on `develop` | 0 warnings, 0 errors / 56 passed, 0 failed |
| Frontend `npm test` / `npm run lint` / `npm run build` on `develop` | 19 passed / clean / OK |
| `docker compose up -d --build` from `develop` + smoke test | all services up, hold → confirm → `CONFIRMED`, full class → `409`, UI `200` |
| `develop` → `main` | merged with `--no-ff` after this commit; hash: `git log main -1` |
