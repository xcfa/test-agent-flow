using Notes.Domain;

namespace Notes.Application;

public sealed class NoteService(INoteRepository repository, TimeProvider timeProvider)
{
    public Task<IReadOnlyList<Note>> ListAsync(CancellationToken cancellationToken = default) =>
        repository.ListAsync(cancellationToken);

    public Task<Note?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.GetAsync(id, cancellationToken);

    public async Task<Note> CreateAsync(string title, string body, CancellationToken cancellationToken = default)
    {
        var note = Note.Create(title, body, timeProvider.GetUtcNow());
        await repository.SaveAsync(note, cancellationToken);
        return note;
    }

    public async Task<Note?> UpdateAsync(Guid id, string title, string body, CancellationToken cancellationToken = default)
    {
        var note = await repository.GetAsync(id, cancellationToken);
        if (note is null)
        {
            return null;
        }

        note.Update(title, body, timeProvider.GetUtcNow());
        await repository.SaveAsync(note, cancellationToken);
        return note;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.DeleteAsync(id, cancellationToken);
}
