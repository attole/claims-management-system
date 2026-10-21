import { Pipe, PipeTransform } from '@angular/core';
import { format } from 'date-fns';
import {
  calendarDateOf,
  DATE_TIME_DISPLAY_FORMAT,
  isCalendarDate,
  isSetDate,
  parseIsoDateTime,
} from '../../core/api/api-dates';

export type DateDisplay = 'day' | 'dateTime';

export function formatClaimDate(value: string, display: DateDisplay = 'day'): string {
  if (!isSetDate(value)) {
    return '';
  }

  if (isCalendarDate(value)) {
    return value;
  }

  return display === 'day' ? calendarDateOf(value) : format(parseIsoDateTime(value), DATE_TIME_DISPLAY_FORMAT);
}

@Pipe({ name: 'claimDate' })
export class ClaimDatePipe implements PipeTransform {
  transform(value: string, display: DateDisplay = 'day'): string {
    return formatClaimDate(value, display);
  }
}
