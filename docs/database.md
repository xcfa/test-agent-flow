# Database

The API stores notes either in memory (default) or in a relational database through EF Core.
The database is chosen by configuration; everything except the connection call is shared
(`NotesDbContext`, mapping, `EfNoteRepository`, migrations runner).

| `Database:Provider` | Storage | Migrations project |
| --- | --- | --- |
| `InMemory` (default) | process memory, lost on restart | — |
| `Sqlite` | SQLite file | `src/Notes.Migrations.Sqlite` |
| `SqlServer` | SQL Server | `src/Notes.Migrations.SqlServer` |
| `PostgreSql` | PostgreSQL (Npgsql) | `src/Notes.Migrations.PostgreSql` |

The value is case-insensitive. A non-InMemory provider without `ConnectionString`, or an unknown
provider, stops the application at startup.

## Configuration

```json
"Database": { "Provider": "PostgreSql", "ConnectionString": "Host=localhost;Database=notes;Username=notes;Password=...", "MigrateOnStartup": true }
```

Environment variables: `Database__Provider`, `Database__ConnectionString`, `Database__MigrateOnStartup`.

`MigrateOnStartup` (default `false`) applies pending migrations when the app starts. With several
replicas migrations may run concurrently, so in production prefer a separate deploy step:
`dotnet ef database update --project src/Notes.Migrations.<Provider> --startup-project src/Notes.Api -- --Database:Provider=<Provider> --Database:ConnectionString="..."`.

All timestamps are UTC. PostgreSQL `timestamptz` rejects non-zero offsets, and SQLite stores UTC
`DateTime` (the offset is not preserved).

## SQLite

```bash
Database__Provider=Sqlite Database__ConnectionString="Data Source=notes.db" Database__MigrateOnStartup=true dotnet run --project src/Notes.Api
```

`dotnet run` with the `http` launch profile already uses `notes.db`. In a container the app
directory is read-only for the app user, so use a volume on `/data`:

```bash
docker run -p 8080:8080 -v notes-data:/data \
  -e Database__Provider=Sqlite -e "Database__ConnectionString=Data Source=/data/notes.db" \
  -e Database__MigrateOnStartup=true notes-api
```

## SQL Server

```bash
docker run -d -p 1433:1433 -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='Your_password123' mcr.microsoft.com/mssql/server:2022-latest
```

```
Database__Provider=SqlServer
Database__ConnectionString=Server=localhost,1433;Database=notes;User Id=sa;Password=Your_password123;TrustServerCertificate=True
```

## PostgreSQL

```bash
docker run -d -p 5432:5432 -e POSTGRES_USER=notes -e POSTGRES_PASSWORD=notes -e POSTGRES_DB=notes postgres:17-alpine
```

```
Database__Provider=PostgreSql
Database__ConnectionString=Host=localhost;Database=notes;Username=notes;Password=notes
```

## Adding a migration

Each provider has its own migrations assembly. After changing the model, add a migration to all three
(the connection string is only used for validation, no database is needed):

```bash
dotnet tool restore
dotnet ef migrations add <Name> --project src/Notes.Migrations.Sqlite     --startup-project src/Notes.Api -- --Database:Provider=Sqlite     --Database:ConnectionString="Data Source=design.db"
dotnet ef migrations add <Name> --project src/Notes.Migrations.SqlServer  --startup-project src/Notes.Api -- --Database:Provider=SqlServer  --Database:ConnectionString="Server=.;Database=notes"
dotnet ef migrations add <Name> --project src/Notes.Migrations.PostgreSql --startup-project src/Notes.Api -- --Database:Provider=PostgreSql --Database:ConnectionString="Host=localhost;Database=notes"
```

The `Migrations_MatchModel` test fails if a provider is missing a migration.

## Adding a provider

Add a value to `DatabaseProvider`, one line to the table in `DatabaseProviders.cs`, and a
`Notes.Migrations.<Provider>` project (the assembly name must match).

## Tests

`dotnet test` runs repository contract tests on SQLite always. SQL Server and PostgreSQL tests use
Testcontainers and run when `CI=true` or `NOTES_DOCKER_TESTS=1`; otherwise they are skipped.
