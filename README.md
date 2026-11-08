# test-agent-flow

A deliberately small notes app (ASP.NET Core minimal API + React) that serves as a
sandbox for multi-agent workflows. Little code, many projects, full CI.

## Layout

| Path | What |
| --- | --- |
| `src/Notes.Domain` | `Note` entity and its invariants |
| `src/Notes.Application` | `NoteService`, `INoteRepository` |
| `src/Notes.Infrastructure` | In-memory and EF Core repositories, DI registration |
| `src/Notes.Migrations.*` | EF Core migrations per provider (SQLite, SQL Server, PostgreSQL) |
| `src/Notes.Contracts` | HTTP DTOs — **NuGet package** |
| `src/Notes.Client` | Typed `HttpClient` — **NuGet package** |
| `src/Notes.Api` | Minimal API (`/api/notes`, `/health`) — **container image** |
| `tests/*` | xUnit: domain, application, API end-to-end via `Notes.Client` |
| `web/packages/api-client` | TS fetch client — **npm package** `@xcfa/notes-api-client` |
| `web/apps/notes-web` | React + Vite UI |

## Run locally

```bash
dotnet run --project src/Notes.Api        # http://localhost:5080
cd web && npm ci && npm run build -w @xcfa/notes-api-client && npm run dev -w notes-web
```

Vite proxies `/api` to the API. The `http` launch profile stores data in SQLite (`notes.db`);
without it the API uses in-memory storage. Other databases: see [docs/database.md](docs/database.md).

## Checks

```bash
dotnet format Notes.slnx --verify-no-changes && dotnet test Notes.slnx
cd web && npm run lint && npm run typecheck && npm test && npm run build
```

## Workflows

- `ci.yml` — `dotnet format`, build, tests with coverage; ESLint, `tsc`, Vitest, Vite build.
- `codeql.yml` — CodeQL for C#, JS/TS and Actions (security-and-quality).
- `semgrep.yml` — Semgrep rule packs, SARIF uploaded to code scanning.
- `publish.yml` — on tag `vX.Y.Z`: NuGet packages, npm package and `ghcr.io/<owner>/notes-api` image to GitHub Packages.
- `dependabot.yml` — NuGet, npm, Actions updates.

The npm scope `@xcfa` must match the GitHub owner the repository is pushed to.
