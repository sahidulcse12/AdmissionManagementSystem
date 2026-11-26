# School Admission Management System

> বাংলা: [README.bn.md](README.bn.md)

A first-come-first-served school admission system. Parents reserve a seat in a class and shift,
hold it for a limited time (10 minutes by default), and confirm admission. Admins create classes,
set seat capacity and watch seats fill up. The system never admits more students than a class's
capacity, even when many parents click at the same moment.

## Features

- **Admin:** create classes per shift with a seat capacity; live table of admitted / held / available
  seats; per-class list of holds and admissions.
- **Parent / guardian:** identify the student by birth registration number; see available seats;
  reserve a seat; live countdown; pay-and-confirm (payment is simulated) or cancel; admission history.
- **Strict first-come-first-served**: no lottery; the order requests reach the database decides.
- **Seat holds** with configurable duration; expired holds free their seat automatically.
- **Concurrency-safe**: row-level locks, transactions, and a partial unique index, verified with
  race tests on a real PostgreSQL database.
- **Idempotent confirmation** and **backend ownership checks** on every hold operation.

## Tech stack

| Layer | Technology |
|---|---|
| Backend | C# · .NET 9 · ASP.NET Core Minimal APIs · EF Core 9 · Clean Architecture |
| Database | PostgreSQL 16 |
| Frontend | React 19 · TypeScript · Vite |
| Tests | xUnit · Testcontainers (real PostgreSQL) · Vitest · Testing Library |
| Runtime | Docker Compose (db, api, web) |

## Quick start

Requirements: **Docker Desktop** (or Docker Engine with Compose v2). Nothing else.

```bash
cp .env.example .env        # optional — every value has a default
docker compose up --build
```

| Service | URL |
|---|---|
| Parent / guardian UI | http://localhost:5173 |
| Admin UI | http://localhost:5173/#/admin |
| Backend API | http://localhost:8080 |
| Health check | http://localhost:8080/health |

The first build downloads base images and can take several minutes. Database migrations run
automatically when the API starts. Data is stored in the `pgdata` Docker volume and survives
`docker compose down`; use `docker compose down -v` to wipe it.

### Try it

1. Open the **Admin** tab and create a class, e.g. `Class 1`, `MORNING`, capacity `2`.
2. Open the **Parent / Guardian** tab, enter a 17-digit number such as `20150123456789012`.
3. Click **Reserve seat** — a 10-minute countdown appears.
4. Click **Pay & confirm admission** — the seat moves to *Admitted* in history and in the admin table.
5. Use **Change student** with another number to see seats run out and the class show **Full**.

## Configuration

All settings are environment variables (see [.env.example](.env.example)).

| Variable | Default | Used by | Meaning |
|---|---|---|---|
| `HOLD_DURATION_SECONDS` | `600` | api | How long a hold reserves its seat. |
| `EXPIRY_SWEEP_SECONDS` | `15` | api | How often overdue holds are marked `EXPIRED` in the database. |
| `CORS_ORIGINS` | `*` | api | Allowed browser origins (comma-separated). |
| `API_PORT` | `8080` | api | Host port for the API. |
| `POSTGRES_DB` / `POSTGRES_USER` / `POSTGRES_PASSWORD` | `admission` | db, api | Database credentials; the API connection string is built from them. |
| `POSTGRES_PORT` | `5432` | db | Host port for PostgreSQL. |
| `API_URL` | `http://localhost:8080` | web | API address used by the browser; written to `config.js` at container start. |
| `FRONTEND_PORT` | `5173` | web | Host port for the UI. |

The API reads its connection string from `DATABASE_URL` (Npgsql format); Compose sets it for you.

## How seat reservation stays correct

1. `POST /classes/{id}/holds` opens a transaction and locks the class row with
   `SELECT … FOR UPDATE`. Concurrent requests for that class wait and run one at a time in arrival order.
2. Under the lock it counts occupied seats (`CONFIRMED` + unexpired `HELD`) and inserts the hold only
   if a seat is free.
3. A partial unique index on `(class_id, student_id) WHERE status IN ('HELD','CONFIRMED')` makes a
   duplicate active hold impossible at the database level.
4. Confirm and cancel lock the hold row, check ownership, and evaluate expiry *after* the lock is taken.
   Confirming twice returns the same result.
5. Seat counts are always computed from the database; a background job marks overdue holds `EXPIRED`.

Details, diagrams and the full scenario table: [ARCHITECTURE.md](ARCHITECTURE.md).

## API

| Method | Path | Purpose |
|---|---|---|
| `POST` | `/classes` | Create a class |
| `GET` | `/classes` | List classes with live seat counts |
| `GET` | `/classes/{classId}` | One class |
| `POST` | `/classes/{classId}/holds` | Reserve a seat (`X-Student-Id` required) |
| `GET` | `/holds/{holdId}` | Read a hold (owner only) |
| `POST` | `/holds/{holdId}/confirm` | Confirm admission (owner only, idempotent) |
| `DELETE` | `/holds/{holdId}` | Cancel a hold (owner only) |
| `GET` | `/me/holds` | The student's hold and admission history |
| `GET` | `/classes/{classId}/holds` | Admin: holds and admissions of a class |

Request/response formats and every error code: [API.md](API.md).

## Running tests

Backend tests start a throwaway PostgreSQL container, so Docker must be running.

```bash
cd backend
dotnet test                       # 56 tests: domain rules, API, expiry, races
```

```bash
cd frontend
npm ci
npm test                          # 19 tests: admin and parent flows
npm run lint && npm run build
```

Without the .NET SDK or Node installed locally, tests can be run in containers:

```bash
docker run --rm -v "$PWD/frontend:/app" -w /app node:22-alpine sh -c "npm ci && npm test"
```

## Project structure

```
.
├── backend/
│   ├── src/Admission.Domain/          # entities, state machine, rules
│   ├── src/Admission.Application/     # use cases, DTOs, repository interfaces
│   ├── src/Admission.Infrastructure/  # EF Core, PostgreSQL, migrations, expiry job
│   ├── src/Admission.Api/             # endpoints, error middleware, startup
│   └── tests/Admission.Tests/         # unit + integration + concurrency tests
├── frontend/                          # React + TypeScript (Vite), nginx image
├── docker-compose.yml
└── .env.example
```

## Git workflow

```
main ── develop ──┬── feature/project-setup
                  ├── feature/database-and-class-management
                  ├── feature/seat-availability-and-hold
                  ├── feature/admission-confirmation-and-cancellation
                  ├── feature/frontend-admin
                  ├── feature/frontend-parent
                  └── feature/testing-and-documentation
```

Each feature branch was cut from the latest `develop`, built up in small task-sized commits, and
merged back with `--no-ff`. After full verification `develop` was merged into `main`.
History and commit hashes: [PROGRESS.md](PROGRESS.md).

## Known limitations

- No authentication: the student is whoever `X-Student-Id` says, and admin endpoints are open.
- Payment is simulated; confirming always succeeds.
- Classes cannot be edited or deleted; confirmed admissions cannot be withdrawn through the API.
- The UI refreshes by polling every 5 seconds rather than push updates.
- The UI text is English only.

## Documentation

| Topic | English | বাংলা |
|---|---|---|
| Overview (this file) | [README.md](README.md) | [README.bn.md](README.bn.md) |
| Architecture, database, concurrency | [ARCHITECTURE.md](ARCHITECTURE.md) | [ARCHITECTURE.bn.md](ARCHITECTURE.bn.md) |
| API reference | [API.md](API.md) | [API.bn.md](API.bn.md) |
| Implementation plan | [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) | [IMPLEMENTATION_PLAN.bn.md](IMPLEMENTATION_PLAN.bn.md) |
| Progress, commits, test results | [PROGRESS.md](PROGRESS.md) | [PROGRESS.bn.md](PROGRESS.bn.md) |
| Assumptions | [ASSUMPTIONS.md](ASSUMPTIONS.md) | [ASSUMPTIONS.bn.md](ASSUMPTIONS.bn.md) |
| AI usage | [AI_USAGE.md](AI_USAGE.md) | [AI_USAGE.bn.md](AI_USAGE.bn.md) |
