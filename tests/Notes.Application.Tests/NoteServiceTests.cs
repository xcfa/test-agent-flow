using Microsoft.Extensions.Time.Testing;
using Notes.Application;
using Notes.Infrastructure;

namespace Notes.Application.Tests;

public class NoteServiceTests
{
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
    private readonly NoteService _service;

    public NoteServiceTests() => _service = new NoteService(new InMemoryNoteRepository(), _time);

    [Fact]
    public async Task Create_ThenGet_ReturnsNote()
    {
        var created = await _service.CreateAsync("title", "body");

        var loaded = await _service.GetAsync(created.Id);

        Assert.NotNull(loaded);
        Assert.Equal("title", loaded.Title);
    }

    [Fact]
    public async Task List_ReturnsMostRecentlyUpdatedFirst()
    {
        var first = await _service.CreateAsync("first", "");
        _time.Advance(TimeSpan.FromMinutes(1));
        await _service.CreateAsync("second", "");
        _time.Advance(TimeSpan.FromMinutes(1));
        await _service.UpdateAsync(first.Id, "first edited", "");

        var titles = (await _service.ListAsync()).Select(n => n.Title);

        Assert.Equal(["first edited", "second"], titles);
    }

    [Fact]
    public async Task Update_UnknownId_ReturnsNull() =>
        Assert.Null(await _service.UpdateAsync(Guid.NewGuid(), "t", "b"));

    [Fact]
    public async Task Delete_RemovesNote()
    {
        var note = await _service.CreateAsync("t", "b");

        Assert.True(await _service.DeleteAsync(note.Id));
        Assert.False(await _service.DeleteAsync(note.Id));
        Assert.Null(await _service.GetAsync(note.Id));
    }
}
