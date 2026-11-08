using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Notes.Infrastructure.Persistence;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;

namespace Notes.Infrastructure.Tests;

internal static class DockerTests
{
    public static bool Enabled =>
        Environment.GetEnvironmentVariable("CI") == "true"
        || Environment.GetEnvironmentVariable("NOTES_DOCKER_TESTS") == "1";
}

internal static class TestDb
{
    public static async Task MigrateAsync(DatabaseOptions options)
    {
        await using var db = Create(options);
        await db.Database.MigrateAsync();
    }

    public static NotesDbContext Create(DatabaseOptions options)
    {
        var builder = new DbContextOptionsBuilder<NotesDbContext>();
        DatabaseProviders.Configure(builder, options);
        return new NotesDbContext(builder.Options);
    }
}

public sealed class SqliteNoteRepositoryTests : NoteRepositoryContract
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"notes-{Guid.NewGuid():N}.db");
    private DatabaseOptions? _options;

    protected override async Task StartAsync()
    {
        _options = new DatabaseOptions { Provider = DatabaseProvider.Sqlite, ConnectionString = $"Data Source={_path}" };
        await TestDb.MigrateAsync(_options);
    }

    protected override Task StopAsync()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_path);
        return Task.CompletedTask;
    }

    protected override NotesDbContext CreateContext() => TestDb.Create(_options!);
}

public sealed class PostgreSqlNoteRepositoryTests : NoteRepositoryContract
{
    private PostgreSqlContainer? _container;
    private DatabaseOptions? _options;

    protected override bool IsEnabled => DockerTests.Enabled;

    protected override async Task StartAsync()
    {
        _container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await _container.StartAsync();
        _options = new DatabaseOptions { Provider = DatabaseProvider.PostgreSql, ConnectionString = _container.GetConnectionString() };
        await TestDb.MigrateAsync(_options);
    }

    protected override async Task StopAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    protected override NotesDbContext CreateContext() => TestDb.Create(_options!);
}

public sealed class SqlServerNoteRepositoryTests : NoteRepositoryContract
{
    private MsSqlContainer? _container;
    private DatabaseOptions? _options;

    protected override bool IsEnabled => DockerTests.Enabled;

    protected override async Task StartAsync()
    {
        _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await _container.StartAsync();
        _options = new DatabaseOptions { Provider = DatabaseProvider.SqlServer, ConnectionString = _container.GetConnectionString() };
        await TestDb.MigrateAsync(_options);
    }

    protected override async Task StopAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    protected override NotesDbContext CreateContext() => TestDb.Create(_options!);
}
