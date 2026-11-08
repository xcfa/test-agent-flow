using Microsoft.EntityFrameworkCore;
using Notes.Domain;
using Notes.Infrastructure.Persistence;

namespace Notes.Infrastructure.Tests;

/// <summary>Behavioural contract every EF-backed provider must satisfy.</summary>
public abstract class NoteRepositoryContract : IAsyncLifetime
{
    private static readonly DateTimeOffset T0 = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    protected virtual bool IsEnabled => true;

    public async Task InitializeAsync()
    {
        if (IsEnabled)
        {
            await StartAsync();
        }
    }

    public async Task DisposeAsync()
    {
        if (IsEnabled)
        {
            await StopAsync();
        }
    }

    protected abstract Task StartAsync();

    protected abstract Task StopAsync();

    protected abstract NotesDbContext CreateContext();

    [SkippableFact]
    public async Task Save_ThenGet()
    {
        Skip.IfNot(IsEnabled, "Docker tests are disabled.");
        var note = Note.Create("title", "body", T0);

        await using (var db = CreateContext())
        {
            await new EfNoteRepository(db).SaveAsync(note, default);
        }

        await using var read = CreateContext();
        var loaded = await new EfNoteRepository(read).GetAsync(note.Id, default);
        Assert.NotNull(loaded);
        Assert.Equal("title", loaded.Title);
        Assert.Equal("body", loaded.Body);
    }

    [SkippableFact]
    public async Task Save_Existing_Updates()
    {
        Skip.IfNot(IsEnabled, "Docker tests are disabled.");
        var note = Note.Create("title", "body", T0);
        await using (var db = CreateContext())
        {
            await new EfNoteRepository(db).SaveAsync(note, default);
        }

        // Entity loaded in one context and saved through another (detached).
        Note detached;
        await using (var db = CreateContext())
        {
            detached = (await new EfNoteRepository(db).GetAsync(note.Id, default))!;
        }

        detached.Update("new title", "new body", T0.AddMinutes(1));
        await using (var db = CreateContext())
        {
            await new EfNoteRepository(db).SaveAsync(detached, default);
        }

        await using var read = CreateContext();
        Assert.Equal(1, await read.Notes.CountAsync());
        var loaded = await new EfNoteRepository(read).GetAsync(note.Id, default);
        Assert.Equal("new title", loaded?.Title);
        Assert.Equal(T0.AddMinutes(1), loaded?.UpdatedAt);
    }

    [SkippableFact]
    public async Task List_OrdersByUpdatedAtDesc()
    {
        Skip.IfNot(IsEnabled, "Docker tests are disabled.");
        var old = Note.Create("old", "", T0);
        var mid = Note.Create("mid", "", T0.AddHours(1));
        var recent = Note.Create("recent", "", T0.AddHours(2));
        await using (var db = CreateContext())
        {
            var repo = new EfNoteRepository(db);
            await repo.SaveAsync(mid, default);
            await repo.SaveAsync(old, default);
            await repo.SaveAsync(recent, default);
        }

        await using var read = CreateContext();
        var list = await new EfNoteRepository(read).ListAsync(default);

        Assert.Equal(["recent", "mid", "old"], list.Select(n => n.Title));
    }

    [SkippableFact]
    public async Task Delete_Existing_ReturnsTrue_ThenFalse()
    {
        Skip.IfNot(IsEnabled, "Docker tests are disabled.");
        var note = Note.Create("t", "b", T0);
        await using var db = CreateContext();
        var repo = new EfNoteRepository(db);
        await repo.SaveAsync(note, default);

        Assert.True(await repo.DeleteAsync(note.Id, default));
        Assert.False(await repo.DeleteAsync(note.Id, default));
    }

    [SkippableFact]
    public async Task Get_Unknown_ReturnsNull()
    {
        Skip.IfNot(IsEnabled, "Docker tests are disabled.");
        await using var db = CreateContext();

        Assert.Null(await new EfNoteRepository(db).GetAsync(Guid.NewGuid(), default));
    }

    [SkippableFact]
    public async Task RoundTrip_PreservesUtcTimestamps()
    {
        Skip.IfNot(IsEnabled, "Docker tests are disabled.");
        var stamp = new DateTimeOffset(2026, 3, 4, 5, 6, 7, TimeSpan.Zero).AddTicks(10); // 1 µs
        var note = Note.Create("t", "b", stamp);
        await using (var db = CreateContext())
        {
            await new EfNoteRepository(db).SaveAsync(note, default);
        }

        await using var read = CreateContext();
        var loaded = await new EfNoteRepository(read).GetAsync(note.Id, default);
        Assert.NotNull(loaded);
        Assert.Equal(stamp, loaded.CreatedAt);
        Assert.Equal(stamp, loaded.UpdatedAt);
    }
}
