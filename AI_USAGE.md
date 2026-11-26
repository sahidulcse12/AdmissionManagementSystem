# AI Usage

> Bangla version: [AI_USAGE.bn.md](AI_USAGE.bn.md)

## Tool and model

- **Tool:** Claude Code (Anthropic's agentic coding assistant), run from the IDE.
- **Model:** Claude Opus 5.5.

## What the AI did

The AI agent carried out the work end to end under the developer's direction:

| Area | AI contribution |
|---|---|
| Planning | Broke the assignment into seven feature branches and a priority order (P0/P1/P2). |
| Backend | Wrote the Domain, Application, Infrastructure and Api layers, EF Core configuration and migrations. |
| Concurrency design | Chose per-class `SELECT … FOR UPDATE`, per-hold row locks, the partial unique index, and the single-row expiry that avoids deadlocks with the sweeper. |
| Frontend | Wrote the React/TypeScript admin and parent interfaces, API client, polling and countdown hooks. |
| Tests | Wrote 56 backend tests (domain unit + integration on real PostgreSQL via Testcontainers, including race tests) and 19 frontend tests. |
| Infrastructure | Wrote Dockerfiles, `docker-compose.yml`, `.env.example`. |
| Git | Created branches, small task-sized commits and `--no-ff` merges following `main → develop → feature/* → develop → main`. |
| Documentation | Wrote all English and Bangla documents. |

## Developer direction during the work

The developer steered the process with these instructions, which the AI followed:

- Restructure the backend into **Clean Architecture** (applied before the hold logic was written).
- **Commit after every completed task** instead of large commits.
- **Report each completed feature in Bangla.**

## How AI output was verified

AI-generated code was not accepted on trust:

- Every feature branch was built and its tests run before merging; `develop` was re-tested after each merge.
- Concurrency tests were run repeatedly to check for flakiness.
- Re-testing on `develop` exposed a real bug in AI-written code (timestamp precision mismatch that broke
  idempotent confirm responses). It was diagnosed and fixed (`40591d2`) and the suite re-run three times.
- The full stack was started with `docker compose up` and exercised through the HTTP API.

## Limitations of AI assistance

- Design choices (e.g. shift names, 17-digit student id, confirmed admissions not cancellable) are
  documented in [ASSUMPTIONS.md](ASSUMPTIONS.md) and should be reviewed by a human.
- Manual UI review in a real browser remains the developer's responsibility.
