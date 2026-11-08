using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Notes.Client;
using Notes.Contracts;

namespace Notes.Api.Tests;

/// <summary>End-to-end API tests against a real SQLite file, with migrations applied on startup.</summary>
public sealed class NotesApiSqliteTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"notes-api-{Guid.NewGuid():N}.db");
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _http;
    private readonly NotesClient _client;

    public NotesApiSqliteTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b => b
            .UseSetting("Database:Provider", "Sqlite")
            .UseSetting("Database:ConnectionString", $"Data Source={_path}")
            .UseSetting("Database:MigrateOnStartup", "true"));
        _http = _factory.CreateClient();
        _client = new NotesClient(_http);
    }

    public void Dispose()
    {
        _http.Dispose();
        _factory.Dispose();
        SqliteConnection.ClearAllPools();
        File.Delete(_path);
    }

    [Fact]
    public async Task Crud_RoundTrip_PersistsAndOrders()
    {
        var first = await _client.CreateAsync(new SaveNoteRequest("first", "a"));
        await Task.Delay(20);
        var second = await _client.CreateAsync(new SaveNoteRequest("second", "b"));

        var updated = await _client.UpdateAsync(first.Id, new SaveNoteRequest("first", "a2"));
        Assert.Equal("a2", updated?.Body);

        var list = await _client.ListAsync();
        Assert.Equal([first.Id, second.Id], list.Select(n => n.Id));

        Assert.True(await _client.DeleteAsync(second.Id));
        Assert.Null(await _client.GetAsync(second.Id));
    }
}
