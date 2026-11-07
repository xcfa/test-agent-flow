import type { Note, NotesClient } from '@xcfa/notes-api-client';
import { type FormEvent, useEffect, useState } from 'react';

export function App({ client }: { client: NotesClient }) {
  const [notes, setNotes] = useState<Note[]>([]);
  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    client.list().then(setNotes, () => setError('Failed to load notes'));
  }, [client]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    try {
      const note = await client.create({ title, body });
      setNotes((current) => [note, ...current]);
      setTitle('');
      setBody('');
      setError(null);
    } catch {
      setError('Failed to save note');
    }
  }

  async function handleDelete(id: string) {
    await client.remove(id);
    setNotes((current) => current.filter((n) => n.id !== id));
  }

  return (
    <main>
      <h1>Notes</h1>
      <form onSubmit={handleSubmit}>
        <input aria-label="Title" placeholder="Title" value={title} onChange={(e) => setTitle(e.target.value)} />
        <textarea aria-label="Body" placeholder="Body" value={body} onChange={(e) => setBody(e.target.value)} />
        <button type="submit" disabled={!title.trim()}>
          Add
        </button>
      </form>
      {error && <p role="alert">{error}</p>}
      <ul>
        {notes.map((note) => (
          <li key={note.id}>
            <h2>{note.title}</h2>
            <p>{note.body}</p>
            <button onClick={() => handleDelete(note.id)}>Delete</button>
          </li>
        ))}
      </ul>
    </main>
  );
}
