import { Pipe, PipeTransform } from '@angular/core';
import { ValidationErrors } from '@angular/forms';
import { CALENDAR_DATE_FORMAT } from '../../core/api/api-dates';
import {
  CLAIMANT_REQUIRED_ERROR,
  MIN_TRIMMED_LENGTH_ERROR,
  NAME_REQUIRED_ERROR,
  NON_ZERO_ERROR,
  NOT_IN_FUTURE_ERROR,
  UNKNOWN_OPTION_ERROR,
} from './claim-validators';
import { DATE_RANGE_ERROR } from './date-range';
import { SERVER_ERROR_KEY } from './server-errors';

type MessageFor = (detail: Record<string, unknown>) => string;

const MESSAGES: Readonly<Record<string, MessageFor>> = {
  [SERVER_ERROR_KEY]: (detail) => String(detail),
  required: () => 'This field is required.',
  email: () => 'Enter a valid email address.',
  min: (detail) => `Enter ${String(detail['min'])} or more.`,
  maxlength: (detail) => `Use at most ${String(detail['requiredLength'])} characters.`,
  matDatepickerParse: () => `Enter a date as ${CALENDAR_DATE_FORMAT}.`,
  matDatepickerMax: () => 'The date cannot be in the future.',
  matTimepickerParse: () => 'Enter a time as HH:mm.',
  [NOT_IN_FUTURE_ERROR]: () => 'The loss date cannot be in the future.',
  [MIN_TRIMMED_LENGTH_ERROR]: (detail) => `Enter at least ${String(detail['requiredLength'])} characters.`,
  [NON_ZERO_ERROR]: () => 'The amount cannot be zero.',
  [UNKNOWN_OPTION_ERROR]: () => 'Choose a value from the list.',
  [CLAIMANT_REQUIRED_ERROR]: () => 'Add at least one claimant.',
  [NAME_REQUIRED_ERROR]: () => 'Enter a name.',
  [DATE_RANGE_ERROR]: () => 'The end date is before the start date.',
};

export function errorMessage(errors: ValidationErrors | null): string {
  const [key, detail] = Object.entries(errors ?? {})[0] ?? ['', ''];
  const messageFor = MESSAGES[key];

  if (key.length === 0) {
    return '';
  }

  return messageFor ? messageFor(detail as Record<string, unknown>) : 'This value is not valid.';
}

@Pipe({ name: 'errorMessage' })
export class ErrorMessagePipe implements PipeTransform {
  transform(errors: ValidationErrors | null): string {
    return errorMessage(errors);
  }
}
