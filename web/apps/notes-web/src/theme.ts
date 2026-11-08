// Синхронизировать с inline-скриптом в index.html (ключ и список тем).
export const THEMES = ['system', 'light', 'dark', 'sepia'] as const;
export type Theme = (typeof THEMES)[number];
export const THEME_STORAGE_KEY = 'notes-web:theme';

export function isTheme(value: unknown): value is Theme {
  return THEMES.some((t) => t === value);
}

/** Читает сохранённую тему; при отсутствии, мусоре или исключении storage — 'system'. */
export function readStoredTheme(storage?: Storage): Theme {
  try {
    const value = (storage ?? window.localStorage).getItem(THEME_STORAGE_KEY);
    return isTheme(value) ? value : 'system';
  } catch {
    return 'system';
  }
}

/** 'system' → removeItem, иначе setItem. Исключения storage проглатываются. */
export function storeTheme(theme: Theme, storage?: Storage): void {
  try {
    const target = storage ?? window.localStorage;
    if (theme === 'system') target.removeItem(THEME_STORAGE_KEY);
    else target.setItem(THEME_STORAGE_KEY, theme);
  } catch {
    // storage недоступен — тема живёт только в рамках сессии
  }
}

/** 'system' → removeAttribute('data-theme'), иначе setAttribute('data-theme', theme). */
export function applyTheme(theme: Theme, root: HTMLElement = document.documentElement): void {
  if (theme === 'system') root.removeAttribute('data-theme');
  else root.setAttribute('data-theme', theme);
}
