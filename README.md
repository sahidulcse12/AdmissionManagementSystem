# School Admission Management System

> বাংলা: [README.bn.md](README.bn.md)

A first-come-first-served school admission system. Parents apply for a seat in a class, the seat is
held for a limited time (10 minutes by default), and paying the admission fee confirms the admission.
Admins create classes with a number of seats and watch them fill up. The system never admits more
students than `totalSeats`, even when many parents apply at the same moment — and even with two backend
processes on the same database.

Each shift is its own class, e.g. "Class 6 - Morning Shift" and "Class 6 - Day Shift".

## Features

- **Student ID box at the top of the page** (17-digit birth registration number); no login.
- **Admin:** create a class with name and total seats; live table of total / held / confirmed /
  available seats; per-class list of applications and admissions.
- **Parent / guardian:** class list with seat counts and an **"আবেদন করুন" (Apply)** button; the
  application shows its status and time left with **"ভর্তি ফি দিন" (Pay admission fee)** and
  **"বাতিল" (Cancel)**; **"আমার আবেদন" (My applications)** lists every application and its status.
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
| **Swagger UI** (try the API in the browser) | http://localhost:8080/swagger |
| Health check | http://localhost:8080/health |

The first build downloads base images and can take several minutes. Database migrations run
automatically when the API starts. Data is stored in the `pgdata` Docker volume and survives
`docker compose down`; use `docker compose down -v` to wipe it.

### Try it

1. Open the **Admin** tab and create a class, e.g. name `Class 6 - Morning Shift`, total seats `2`.
2. In the box at the top of the page, enter a 17-digit number such as `20150123456789012`.
3. On the **Parent / Guardian** tab click **আবেদন করুন (Apply)** — the application shows a 10-minute countdown.
4. Click **ভর্তি ফি দিন (Pay admission fee)** — status becomes *Admitted* in "My applications" and in the admin table.
5. Click **Change** at the top, use another number, and watch the seats run out and the class show **Full**.

To see expiry quickly: `HOLD_DURATION_SECONDS=30 docker compose up` — after 30 s the application shows
**EXPIRED** without reloading and the pay button is disabled.

### Bonus: two backend processes on one database

```bash
docker compose --profile multi up --build
```

This also starts `api2` on http://localhost:8081, sharing the same PostgreSQL. Both APIs can be used at
once; `held + confirmed` still never exceeds `totalSeats`. How this is guaranteed is explained in
[ASSUMPTIONS.md](ASSUMPTIONS.md#bonus-two-backend-processes-on-one-database).

### Running the backend or frontend locally (development)

Docker's `api` container already listens on port 8080, so a backend started from Visual Studio /
`dotnet run` on the same port fails with *"address already in use"*. Keep only the database in Docker:

```bash
docker compose stop api api2 web     # free ports 8080/8081/5173, keep PostgreSQL running
cd backend && dotnet run --project src/Admission.Api        # http://localhost:8080/swagger (F5 in Visual Studio opens it)
cd frontend && npm run dev                                  # http://localhost:5173
```

The local backend connects to `Host=localhost;Port=5432;Database=admission;Username=admission;Password=admission`
by default (the Docker database). Override with the `DATABASE_URL` environment variable if needed.

## Configuration

All settings are environment variables (see [.env.example](.env.example)).

| Variable | Default | Used by | Meaning |
|---|---|---|---|
| `HOLD_DURATION_SECONDS` | `600` | api | How long a hold reserves its seat. |
| `EXPIRY_SWEEP_SECONDS` | `15` | api | How often overdue holds are marked `EXPIRED` in the database. |
| `CORS_ORIGINS` | `*` | api | Allowed browser origins (comma-separated). |
| `API_PORT` | `8080` | api | Host port for the API. |
| `API2_PORT` | `8081` | api2 | Host port for the optional second API (`--profile multi`). |
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
| `POST` | `/classes` | Create a class — body `{ "name", "totalSeats" }` → `201` |
| `GET` | `/classes` | All classes with `totalSeats`, `heldSeats`, `confirmedSeats`, `availableSeats` |
| `GET` | `/classes/{classId}` | One class's seats |
| `POST` | `/classes/{classId}/holds` | Apply / hold a seat (`X-Student-Id`) → `201`, `409` if no seat |
| `GET` | `/holds/{holdId}` | Current status of an application (`holdId`, `status`, `expiresAt`, …) |
| `POST` | `/holds/{holdId}/confirm` | Pay admission fee, confirm (owner only, idempotent) → `200` |
| `DELETE` | `/holds/{holdId}` | Cancel an application (owner only) → `204` |
| `GET` | `/me/holds` | All applications of this student |
| `GET` | `/classes/{classId}/holds` | Extra: admin list of a class's applications |

Request/response formats and every error code: [API.md](API.md). Interactive docs: **http://localhost:8080/swagger**.

**API versioning:** paths are unversioned (as the contract requires). Send `X-Api-Version: 1.0` or
`?api-version=1.0` to pick a version; without it, `1.0` is used. Responses include
`api-supported-versions`; an unsupported version returns `400`. Details in [API.md](API.md#api-versioning).

## Running tests

Backend tests start a throwaway PostgreSQL container, so Docker must be running.

```bash
cd backend
dotnet test                       # 68 tests: contract, domain rules, expiry, races, two instances, versioning, Swagger
```

The concurrency tests are `ConcurrencyTests` (e.g. 50 students racing for 5 seats → exactly 5 holds),
`AdmissionConcurrencyTests` and `MultiInstanceTests` (two backend hosts on one database).
Backend tests need the .NET 9 SDK and a running Docker daemon.

```bash
cd frontend
npm ci
npm test                          # 21 tests: admin, parent, student ID box, expiry, errors
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
- The UI text is English, with the brief's Bangla terms on the main buttons and headings.

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
