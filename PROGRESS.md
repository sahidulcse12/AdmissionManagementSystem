# Progress

> Bangla version: [PROGRESS.bn.md](PROGRESS.bn.md)

## Current status

- **Current phase:** Seat availability and holds complete
- **Current branch:** `feature/seat-availability-and-hold` → merging into `develop`
- **Next action:** Create `feature/admission-confirmation-and-cancellation` from `develop`
- **Backend architecture:** Clean Architecture (`Domain` → `Application` → `Infrastructure` → `Api`)

## Feature branches

| # | Branch | Status | Feature commits | Merge into `develop` |
|---|---|---|---|---|
| 1 | `feature/project-setup` | Done | `03a3853`, `1c104b5`, `0ebe29b`, `3585ae0`, `b032521` | `b32c602` |
| 2 | `feature/database-and-class-management` | Done | `ba02a60`, `e3ff39c`, `f5f9134`, `d507856`, `08b0f0a` | `2af83aa` |
| 3 | `feature/seat-availability-and-hold` | Done | `a748e5c`, `3937cbd`, `cac6b08`, `050f21e`, `cf7ba54` | recorded in next update |
| 4 | `feature/admission-confirmation-and-cancellation` | Pending | | |
| 5 | `feature/frontend-admin` | Pending | | |
| 6 | `feature/frontend-parent` | Pending | | |
| 7 | `feature/testing-and-documentation` | Pending | | |

Base commit on `main`: `41838aa` (initial repository setup).

## Tests executed

| When | Command | Result |
|---|---|---|
| Project setup | `dotnet build` (backend) | Passed, 0 errors |
| Project setup | `npm run build` (frontend) | Passed |
| Project setup | `docker compose config -q` | Valid |
| Class management | `dotnet test` (Testcontainers PostgreSQL 16) | 12 passed, 0 failed |
| Seat holds | `dotnet test` | 31 passed, 0 failed |
| Seat holds | `ConcurrencyTests` repeated 3× | 4/4 passed each run |

## Known issues

- None yet.
