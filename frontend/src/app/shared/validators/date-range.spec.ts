import { FormControl, FormGroup } from '@angular/forms';
import { DATE_RANGE_ERROR, dateRangeValidator } from './date-range';

describe('dateRangeValidator (S:UI-List)', () => {
  function rangeOf(from: Date | null, to: Date | null) {
    return new FormGroup(
      { from: new FormControl<Date | null>(from), to: new FormControl<Date | null>(to) },
      { validators: dateRangeValidator('from', 'to') },
    );
  }

  it('flags an end date before the start date', () => {
    const form = rangeOf(new Date(2026, 5, 10), new Date(2026, 5, 1));

    expect(form.hasError(DATE_RANGE_ERROR)).toBe(true);
  });

  it('accepts the same day, an ordered range and an open end', () => {
    const results = [
      rangeOf(new Date(2026, 5, 1), new Date(2026, 5, 1)),
      rangeOf(new Date(2026, 5, 1), new Date(2026, 5, 9)),
      rangeOf(new Date(2026, 5, 1), null),
    ].map((form) => form.valid);

    expect(results).toEqual([true, true, true]);
  });
});
