export interface Note {
  id: string;
  title: string;
  body: string;
  createdAt: string;
  updatedAt: string;
}

export interface SaveNoteRequest {
  title: string;
  body: string;
}

export class NotesApiError extends Error {
  constructor(public readonly status: number) {
    super(`Notes API request failed with status ${status}`);
  }
}

export interface NotesClient {
  list(): Promise<Note[]>;
  create(request: SaveNoteRequest): Promise<Note>;
  update(id: string, request: SaveNoteRequest): Promise<Note>;
  remove(id: string): Promise<void>;
}

export function createNotesClient(baseUrl = '', fetchImpl: typeof fetch = fetch): NotesClient {
  const url = (path = '') => `${baseUrl}/api/notes${path}`;

  async function send<T>(input: string, init?: RequestInit): Promise<T> {
    const response = await fetchImpl(input, {
      ...init,
      headers: { 'Content-Type': 'application/json', ...init?.headers },
    });
    if (!response.ok) {
      throw new NotesApiError(response.status);
    }
    return (response.status === 204 ? undefined : await response.json()) as T;
  }

  return {
    list: () => send<Note[]>(url()),
    create: (request) => send<Note>(url(), { method: 'POST', body: JSON.stringify(request) }),
    update: (id, request) =>
      send<Note>(url(`/${encodeURIComponent(id)}`), { method: 'PUT', body: JSON.stringify(request) }),
    remove: (id) => send<void>(url(`/${encodeURIComponent(id)}`), { method: 'DELETE' }),
  };
}
