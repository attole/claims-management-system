import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { DEV_USERS } from '../auth/dev-users';
import { Theme, ThemeService } from '../theme/theme.service';
import { AppHeader } from './app-header';

describe('AppHeader (S:UI-General)', () => {
  const theme = signal<Theme>('dark');
  const auth = {
    user: signal(DEV_USERS[0]),
    role: signal(DEV_USERS[0].role),
    switchTo: vi.fn(() => Promise.resolve()),
  };
  const themeService = { theme, toggle: vi.fn() };

  beforeEach(() => {
    theme.set('dark');
    auth.switchTo.mockClear();
    TestBed.configureTestingModule({
      imports: [AppHeader],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: auth },
        { provide: ThemeService, useValue: themeService },
      ],
    });
  });

  it('in the dark theme offers the light theme', async () => {
    const fixture = TestBed.createComponent(AppHeader);

    await fixture.whenStable();

    const toggle = (fixture.nativeElement as HTMLElement).querySelector('.theme-toggle');
    expect(toggle?.getAttribute('aria-label')).toBe('Switch to light theme');
    expect(toggle?.textContent).toContain('light_mode');
  });

  it('shows the current role in the role button name', async () => {
    const fixture = TestBed.createComponent(AppHeader);

    await fixture.whenStable();

    const button = (fixture.nativeElement as HTMLElement).querySelector('.role');
    expect(button?.textContent).toContain('current role');
    expect(button?.textContent).toContain('Handler');
  });

  it('choosing a user in the role menu signs in as that user', async () => {
    const fixture = TestBed.createComponent(AppHeader);
    await fixture.whenStable();
    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('.role')?.click();
    await fixture.whenStable();

    const items = document.querySelectorAll<HTMLButtonElement>('[mat-menu-item]');
    items[1].click();

    expect(auth.switchTo).toHaveBeenCalledWith(DEV_USERS[1]);
  });
});
