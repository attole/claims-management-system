import { Provider } from '@angular/core';
import { MAT_DATE_LOCALE } from '@angular/material/core';
import { provideDateFnsAdapter } from '@angular/material-date-fns-adapter';
import { enUS } from 'date-fns/locale';
import { CALENDAR_DATE_FORMAT, TIME_FORMAT } from '../api/api-dates';

export function provideAppDateAdapter(): Provider[] {
  return [
    provideDateFnsAdapter({
      parse: { dateInput: CALENDAR_DATE_FORMAT, timeInput: TIME_FORMAT },
      display: {
        dateInput: CALENDAR_DATE_FORMAT,
        timeInput: TIME_FORMAT,
        timeOptionLabel: TIME_FORMAT,
        monthYearLabel: 'MMM yyyy',
        dateA11yLabel: 'PPP',
        monthYearA11yLabel: 'MMMM yyyy',
      },
    }),
    { provide: MAT_DATE_LOCALE, useValue: enUS },
  ];
}
