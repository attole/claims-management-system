import { TestBed } from '@angular/core/testing';
import { ThemeService } from './theme.service';

describe('ThemeService (S:UI-General)', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
  });

  it('starts dark when nothing is stored', () => {
    const service = TestBed.inject(ThemeService);

    TestBed.tick();

    expect(service.theme()).toBe('dark');
    expect(document.documentElement.dataset['theme']).toBe('dark');
  });

  it('toggle from dark switches to light and stores it', () => {
    const service = TestBed.inject(ThemeService);

    service.toggle();
    TestBed.tick();

    expect(document.documentElement.dataset['theme']).toBe('light');
    expect(localStorage.getItem('claims.theme')).toBe('light');
  });
});
