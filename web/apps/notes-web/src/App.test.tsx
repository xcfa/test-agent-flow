import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { Note, NotesClient } from '@xcfa/notes-api-client';
import { App } from './App';

const note = (id: string, title: string): Note => ({
  id,
  title,
  body: '',
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
});

function fakeClient(initial: Note[] = []): NotesClient {
  return {
    list: vi.fn().mockResolvedValue(initial),
    create: vi.fn().mockImplementation(async (r) => note('new', r.title)),
    update: vi.fn(),
    remove: vi.fn().mockResolvedValue(undefined),
  };
}

describe('App', () => {
  it('renders notes from the API', async () => {
    render(<App client={fakeClient([note('1', 'Existing')])} />);

    expect(await screen.findByText('Existing')).toBeInTheDocument();
  });

  it('adds a note', async () => {
    const client = fakeClient();
    render(<App client={client} />);

    await userEvent.type(screen.getByLabelText('Title'), 'Groceries');
    await userEvent.click(screen.getByRole('button', { name: 'Add' }));

    expect(await screen.findByText('Groceries')).toBeInTheDocument();
    expect(client.create).toHaveBeenCalledWith({ title: 'Groceries', body: '' });
  });

  it('deletes a note', async () => {
    render(<App client={fakeClient([note('1', 'Old')])} />);

    await userEvent.click(await screen.findByRole('button', { name: 'Delete' }));

    expect(screen.queryByText('Old')).not.toBeInTheDocument();
  });
});
