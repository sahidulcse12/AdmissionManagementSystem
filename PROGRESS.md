# Progress

> Bangla version: [PROGRESS.bn.md](PROGRESS.bn.md)

## Current status

- **Current phase:** Project setup complete
- **Current branch:** `feature/project-setup` → merging into `develop`
- **Next action:** Create `feature/database-and-class-management` from `develop`

## Feature branches

| # | Branch | Status | Feature commits | Merge into `develop` |
|---|---|---|---|---|
| 1 | `feature/project-setup` | Done | `03a3853`, `1c104b5`, `0ebe29b`, `3585ae0` | recorded in next update |
| 2 | `feature/database-and-class-management` | Pending | | |
| 3 | `feature/seat-availability-and-hold` | Pending | | |
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

## Known issues

- None yet.
