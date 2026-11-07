using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Notes.Client;
using Notes.Contracts;

namespace Notes.Api.Tests;

/// <summary>End-to-end tests of the API through the typed <see cref="NotesClient"/>.</summary>
public sealed class NotesApiTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _http;
    private readonly NotesClient _client;

    public NotesApiTests()
    {
        _http = _factory.CreateClient();
        _client = new NotesClient(_http);
    }

    public void Dispose()
    {
        _http.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        using var response = await _http.GetAsync(new Uri("/health", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Crud_RoundTrip()
    {
        var created = await _client.CreateAsync(new SaveNoteRequest("Shopping", "milk"));
        Assert.Contains(await _client.ListAsync(), n => n.Id == created.Id);

        var updated = await _client.UpdateAsync(created.Id, new SaveNoteRequest("Shopping", "milk, eggs"));
        Assert.Equal("milk, eggs", updated?.Body);

        Assert.True(await _client.DeleteAsync(created.Id));
        Assert.Null(await _client.GetAsync(created.Id));
    }

    [Fact]
    public async Task Create_WithEmptyTitle_ReturnsValidationProblem()
    {
        using var response = await _http.PostAsJsonAsync("/api/notes", new SaveNoteRequest("", "x"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_UnknownId_ReturnsNull() =>
        Assert.Null(await _client.UpdateAsync(Guid.NewGuid(), new SaveNoteRequest("t", "b")));
}
