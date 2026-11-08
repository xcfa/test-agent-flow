import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it } from 'vitest';
import { ThemeSwitcher } from './ThemeSwitcher';

afterEach(() => {
  localStorage.clear();
  document.documentElement.removeAttribute('data-theme');
});

const select = () => screen.getByRole('combobox', { name: 'Theme' }) as HTMLSelectElement;

describe('ThemeSwitcher', () => {
  it('defaults to system', () => {
    render(<ThemeSwitcher />);
    expect(select().value).toBe('system');
    expect(document.documentElement.hasAttribute('data-theme')).toBe(false);
  });

  it('applies and stores the chosen theme', async () => {
    render(<ThemeSwitcher />);
    await userEvent.selectOptions(select(), 'dark');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
    expect(localStorage.getItem('notes-web:theme')).toBe('dark');
  });

  it('restores the stored theme', () => {
    localStorage.setItem('notes-web:theme', 'sepia');
    render(<ThemeSwitcher />);
    expect(select().value).toBe('sepia');
    expect(document.documentElement.getAttribute('data-theme')).toBe('sepia');
  });

  it('system clears attribute and key', async () => {
    render(<ThemeSwitcher />);
    await userEvent.selectOptions(select(), 'dark');
    await userEvent.selectOptions(select(), 'system');
    expect(document.documentElement.hasAttribute('data-theme')).toBe(false);
    expect(localStorage.getItem('notes-web:theme')).toBeNull();
  });
});
