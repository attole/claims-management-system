import { DOCUMENT } from '@angular/common';
import { effect, inject, Injectable, signal } from '@angular/core';

export type Theme = 'dark' | 'light';

const THEME_KEY = 'claims.theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  private readonly current = signal<Theme>(readStoredTheme());

  readonly theme = this.current.asReadonly();

  constructor() {
    this.apply(this.current());
    effect(() => this.apply(this.current()));
  }

  toggle(): void {
    this.current.update((theme) => (theme === 'dark' ? 'light' : 'dark'));
  }

  private apply(theme: Theme): void {
    this.document.documentElement.dataset['theme'] = theme;
    writeStoredTheme(theme);
  }
}

function readStoredTheme(): Theme {
  try {
    return localStorage.getItem(THEME_KEY) === 'light' ? 'light' : 'dark';
  } catch {
    return 'dark';
  }
}

function writeStoredTheme(theme: Theme): void {
  try {
    localStorage.setItem(THEME_KEY, theme);
  } catch {
    return;
  }
}
