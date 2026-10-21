import { calendarDateOf, isCalendarDate, toCalendarDate, toIsoDateTime } from './api-dates';

describe('api-dates (S:BR-C-02, S:UI-List)', () => {
  it('toIsoDateTime sends a timestamp in UTC', () => {
    const result = toIsoDateTime(new Date(Date.UTC(2026, 9, 21, 8, 30)));

    expect(result).toBe('2026-10-21T08:30:00.000Z');
  });

  it('toCalendarDate keeps the day the user picked', () => {
    const result = toCalendarDate(new Date(2026, 0, 5));

    expect(result).toBe('2026-01-05');
  });

  it('calendarDateOf takes the UTC day of a timestamp with an offset', () => {
    const result = calendarDateOf('2026-03-01T01:30:00+02:00');

    expect(result).toBe('2026-02-28');
  });

  it('isCalendarDate accepts only yyyy-MM-dd', () => {
    const accepted = isCalendarDate('2026-10-21');
    const rejected = isCalendarDate('2026-10-21T00:00:00Z');

    expect(accepted).toBe(true);
    expect(rejected).toBe(false);
  });
});
