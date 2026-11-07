using Notes.Domain;

namespace Notes.Application;

public interface INoteRepository
{
    Task<IReadOnlyList<Note>> ListAsync(CancellationToken cancellationToken);

    Task<Note?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task SaveAsync(Note note, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
