import { TestBed } from '@angular/core/testing';
import { DateAdapter, MAT_DATE_FORMATS, MatDateFormats } from '@angular/material/core';
import { provideAppDateAdapter } from './app-date-adapter';

describe('provideAppDateAdapter (S:UI-List)', () => {
  beforeEach(() => TestBed.configureTestingModule({ providers: [...provideAppDateAdapter()] }));

  it('date pickers show and read the calendar date format of the API', () => {
    const adapter = TestBed.inject<DateAdapter<Date>>(DateAdapter);
    const formats = TestBed.inject<MatDateFormats>(MAT_DATE_FORMATS);

    const shown = adapter.format(new Date(2026, 0, 5), formats.display.dateInput);
    const read = adapter.parse('2026-01-05', formats.parse.dateInput);

    expect(shown).toBe('2026-01-05');
    expect(read?.getDate()).toBe(5);
  });
});
