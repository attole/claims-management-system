import { UTCDate } from '@date-fns/utc';
import { format, getYear, isMatch, isValid, parse, parseISO } from 'date-fns';

export type IsoDateTime = string;
export type CalendarDate = string;

export const CALENDAR_DATE_FORMAT = 'yyyy-MM-dd';
export const DATE_TIME_DISPLAY_FORMAT = 'yyyy-MM-dd HH:mm';
export const NO_DATE: CalendarDate = '';

export function toIsoDateTime(value: Date): IsoDateTime {
  return new UTCDate(value).toISOString();
}

export function toCalendarDate(value: Date): CalendarDate {
  return format(value, CALENDAR_DATE_FORMAT);
}

export function calendarDateOf(value: IsoDateTime): CalendarDate {
  return format(new UTCDate(parseISO(value)), CALENDAR_DATE_FORMAT);
}

export function isCalendarDate(value: string): value is CalendarDate {
  return value.length === CALENDAR_DATE_FORMAT.length && isMatch(value, CALENDAR_DATE_FORMAT);
}

export function parseCalendarDate(value: CalendarDate): Date {
  return parse(value, CALENDAR_DATE_FORMAT, new Date());
}

export function parseIsoDateTime(value: IsoDateTime): Date {
  return parseISO(value);
}

export function isSetDate(value: string): boolean {
  const date = parseISO(value);

  return isValid(date) && getYear(date) > 1;
}
