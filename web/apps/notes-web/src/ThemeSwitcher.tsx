import { useEffect, useState } from 'react';
import { THEMES, type Theme, applyTheme, isTheme, readStoredTheme, storeTheme } from './theme';

const LABELS: Record<Theme, string> = { system: 'System', light: 'Light', dark: 'Dark', sepia: 'Sepia' };

export function ThemeSwitcher() {
  const [theme, setTheme] = useState<Theme>(() => readStoredTheme());

  useEffect(() => {
    applyTheme(theme);
    storeTheme(theme);
  }, [theme]);

  return (
    <label>
      Theme{' '}
      <select
        aria-label="Theme"
        value={theme}
        onChange={(e) => {
          const next = e.target.value;
          if (isTheme(next)) setTheme(next);
        }}
      >
        {THEMES.map((t) => (
          <option key={t} value={t}>
            {LABELS[t]}
          </option>
        ))}
      </select>
    </label>
  );
}
