import { describe, expect, it, vi } from 'vitest';
import { createNotesClient, NotesApiError } from './index';

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });

describe('createNotesClient', () => {
  it('lists notes from /api/notes', async () => {
    const fetchMock = vi.fn().mockResolvedValue(json([{ id: '1', title: 't' }]));
    const client = createNotesClient('http://api', fetchMock);

    const notes = await client.list();

    expect(fetchMock).toHaveBeenCalledWith('http://api/api/notes', expect.anything());
    expect(notes).toHaveLength(1);
  });

  it('posts a JSON body on create', async () => {
    const fetchMock = vi.fn().mockResolvedValue(json({ id: '1' }, 201));
    const client = createNotesClient('', fetchMock);

    await client.create({ title: 'a', body: 'b' });

    const [, init] = fetchMock.mock.calls[0];
    expect(init.method).toBe('POST');
    expect(JSON.parse(init.body)).toEqual({ title: 'a', body: 'b' });
  });

  it('resolves remove on 204', async () => {
    const client = createNotesClient('', vi.fn().mockResolvedValue(new Response(null, { status: 204 })));

    await expect(client.remove('1')).resolves.toBeUndefined();
  });

  it('throws NotesApiError on failure', async () => {
    const client = createNotesClient('', vi.fn().mockResolvedValue(json({}, 400)));

    await expect(client.create({ title: '', body: '' })).rejects.toBeInstanceOf(NotesApiError);
  });
});
