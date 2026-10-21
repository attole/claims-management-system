import { format } from 'date-fns';
import { formatClaimDate } from './claim-date.pipe';

describe('formatClaimDate (S:UI-List, S:BR-C-02)', () => {
  it('shows a calendar date as it is', () => {
    const result = formatClaimDate('2026-07-22');

    expect(result).toBe('2026-07-22');
  });

  it('shows the day of a timestamp as its UTC day, the day the filters and the backend use', () => {
    const result = formatClaimDate('2026-03-01T01:30:00+02:00');

    expect(result).toBe('2026-02-28');
  });

  it('shows a timestamp with its time in the browser time zone', () => {
    const timestamp = '2026-07-22T09:40:00Z';

    const result = formatClaimDate(timestamp, 'dateTime');

    expect(result).toBe(format(new Date(timestamp), 'yyyy-MM-dd HH:mm'));
  });

  it('shows nothing for an empty value or the API minimum value', () => {
    const results = [formatClaimDate(''), formatClaimDate('0001-01-01T00:00:00+00:00')];

    expect(results).toEqual(['', '']);
  });
});
