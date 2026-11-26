# Implementation Plan

> Bangla version: [IMPLEMENTATION_PLAN.bn.md](IMPLEMENTATION_PLAN.bn.md)

## Goal

A School Admission Management System where parents reserve seats first-come-first-served,
hold them for a limited time, and confirm admission. Admins manage classes (each shift is its own class) and their total seats.
The system must never allocate more seats than a class's totalSeats, even under concurrent load.

## Stack

| Layer | Choice |
|---|---|
| Backend | C# / .NET 9, ASP.NET Core Web API (controllers), EF Core 9 + Npgsql |
| Database | PostgreSQL 16 |
| Frontend | React 19 + TypeScript + Vite, Vitest + Testing Library |
| Infrastructure | Docker Compose (db, api, web), named volume `pgdata` |
| Tests | xUnit + WebApplicationFactory + Testcontainers (real PostgreSQL) |

## Git workflow

`main → develop → feature/* → develop → main`. Every feature branch is cut from the latest
`develop`, committed in small task-sized commits, merged back with `--no-ff`, and `develop`
is merged into `main` only after full verification. Nothing is pushed to a remote.

## Feature branches and scope

| # | Branch | Scope |
|---|---|---|
| 1 | `feature/project-setup` | Solution scaffold, frontend scaffold, Docker Compose, `.env.example`, plan/progress docs |
| 2 | `feature/database-and-class-management` | EF Core model, migration, startup migration, error format, `POST/GET /classes`, `GET /classes/{id}`, tests |
| 3 | `feature/seat-availability-and-hold` | Seat counting, `POST /classes/{id}/holds`, `GET /holds/{id}`, `GET /me/holds`, row locking, expiry sweeper, concurrency tests |
| 4 | `feature/admission-confirmation-and-cancellation` | `POST /holds/{id}/confirm` (idempotent), `DELETE /holds/{id}`, ownership checks, tests |
| 5 | `feature/frontend-admin` | API client, admin page: create classes, view seat usage and admissions |
| 6 | `feature/frontend-parent` | Parent page: student ID, reserve, countdown, confirm, cancel, history, persistence, tests |
| 7 | `feature/testing-and-documentation` | End-to-end Docker verification, full docs in English and Bangla |

## Priorities

- **P0:** Docker startup, persistence, class management, capacity enforcement, holds, expiry,
  confirmation, duplicate prevention, ownership, no overbooking under concurrency.
- **P1:** Admin and parent UIs, history, cancellation, automated tests, bilingual docs.
- **P2:** Visual polish — only after P0/P1.

## Key design decisions

1. **Per-class row lock** (`SELECT … FOR UPDATE` on the class row) serialises all hold
   creation for one class, so the capacity check and the insert are atomic.
2. **Partial unique index** on `(class_id, student_id) WHERE status IN ('HELD','CONFIRMED')`
   is a database-level backstop against duplicate active admissions.
3. **Hold row lock** on confirm/cancel makes those transitions race-free and idempotent.
4. **Expiry is enforced at read and write time** (`expires_at <= now` counts as expired) and a
   background sweeper persists the `EXPIRED` status, so seats free up even with no traffic.
5. **Injectable `TimeProvider`** lets tests move time forward instead of sleeping.
