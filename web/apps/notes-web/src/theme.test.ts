import { afterEach, describe, expect, it } from 'vitest';
import html from '../index.html?raw';
import { THEMES, THEME_STORAGE_KEY, applyTheme, isTheme, readStoredTheme, storeTheme } from './theme';

afterEach(() => {
  localStorage.clear();
  document.documentElement.removeAttribute('data-theme');
});

const throwing = {
  getItem() {
    throw new Error('denied');
  },
  setItem() {
    throw new Error('denied');
  },
  removeItem() {
    throw new Error('denied');
  },
} as unknown as Storage;

describe('isTheme', () => {
  it('accepts known themes and rejects others', () => {
    for (const t of THEMES) expect(isTheme(t)).toBe(true);
    for (const v of ['blue', '', null]) expect(isTheme(v)).toBe(false);
  });
});

describe('readStoredTheme', () => {
  it('defaults to system', () => expect(readStoredTheme()).toBe('system'));
  it('reads stored theme', () => {
    localStorage.setItem(THEME_STORAGE_KEY, 'dark');
    expect(readStoredTheme()).toBe('dark');
  });
  it('falls back on garbage', () => {
    localStorage.setItem(THEME_STORAGE_KEY, 'blue');
    expect(readStoredTheme()).toBe('system');
  });
  it('falls back when storage throws', () => expect(readStoredTheme(throwing)).toBe('system'));
});

describe('storeTheme', () => {
  it('writes and removes the key', () => {
    storeTheme('dark');
    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBe('dark');
    storeTheme('system');
    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBeNull();
  });
  it('swallows storage errors', () => {
    expect(() => storeTheme('dark', throwing)).not.toThrow();
  });
});

describe('applyTheme', () => {
  it('sets and clears the attribute', () => {
    applyTheme('sepia');
    expect(document.documentElement.getAttribute('data-theme')).toBe('sepia');
    applyTheme('system');
    expect(document.documentElement.hasAttribute('data-theme')).toBe(false);
  });
});

describe('index.html sync', () => {
  it('mentions every theme and the storage key', () => {
    for (const t of THEMES.filter((x) => x !== 'system')) expect(html).toContain(`'${t}'`);
    expect(html).toContain(THEME_STORAGE_KEY);
  });
});
