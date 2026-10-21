export type IsoDateTime = string;
export type CalendarDate = string;

export const NO_DATE: CalendarDate = '';

const CALENDAR_DATE = /^\d{4}-\d{2}-\d{2}$/;

export function toIsoDateTime(value: Date): IsoDateTime {
  return value.toISOString();
}

export function toCalendarDate(value: Date): CalendarDate {
  const month = String(value.getMonth() + 1).padStart(2, '0');
  const day = String(value.getDate()).padStart(2, '0');

  return `${value.getFullYear()}-${month}-${day}`;
}

export function calendarDateOf(value: IsoDateTime): CalendarDate {
  return new Date(value).toISOString().slice(0, 10);
}

export function isCalendarDate(value: string): value is CalendarDate {
  return CALENDAR_DATE.test(value);
}
