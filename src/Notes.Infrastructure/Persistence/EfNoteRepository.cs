using Microsoft.EntityFrameworkCore;
using Notes.Application;
using Notes.Domain;

namespace Notes.Infrastructure.Persistence;

internal sealed class EfNoteRepository(NotesDbContext db) : INoteRepository
{
    public async Task<IReadOnlyList<Note>> ListAsync(CancellationToken cancellationToken) =>
        await db.Notes.AsNoTracking().OrderByDescending(n => n.UpdatedAt).ToListAsync(cancellationToken);

    public async Task<Note?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await db.Notes.FindAsync([id], cancellationToken);

    public async Task SaveAsync(Note note, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(note);

        if (db.Entry(note).State == EntityState.Detached)
        {
            if (await db.Notes.AnyAsync(n => n.Id == note.Id, cancellationToken))
            {
                db.Notes.Update(note);
            }
            else
            {
                db.Notes.Add(note);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken) =>
        await db.Notes.Where(n => n.Id == id).ExecuteDeleteAsync(cancellationToken) > 0;
}
