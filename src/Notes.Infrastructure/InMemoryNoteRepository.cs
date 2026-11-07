using System.Collections.Concurrent;
using Notes.Application;
using Notes.Domain;

namespace Notes.Infrastructure;

public sealed class InMemoryNoteRepository : INoteRepository
{
    private readonly ConcurrentDictionary<Guid, Note> _notes = new();

    public Task<IReadOnlyList<Note>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Note>>([.. _notes.Values.OrderByDescending(n => n.UpdatedAt)]);

    public Task<Note?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_notes.GetValueOrDefault(id));

    public Task SaveAsync(Note note, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(note);
        _notes[note.Id] = note;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_notes.TryRemove(id, out _));
}
