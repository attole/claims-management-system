import { HttpErrorResponse } from '@angular/common/http';
import { ErrorBody } from '../api/api.model';

export const GENERAL_ERROR_KEY = 'General';

const UNREACHABLE_MESSAGE = 'The server cannot be reached. Check that the API is running.';

export function readErrorBody(error: HttpErrorResponse): ErrorBody {
  const body: unknown = error.error;
  const fallback: ErrorBody = {
    type: 'UnknownError',
    title: `The request failed with status ${error.status}.`,
    status: error.status,
    errors: {},
  };

  return isErrorBody(body) ? body : fallback;
}

export function describeError(error: HttpErrorResponse): string {
  if (error.status === 0) {
    return UNREACHABLE_MESSAGE;
  }

  const body = readErrorBody(error);
  const firstMessage = Object.values(body.errors).flat()[0] ?? '';

  return firstMessage.length > 0 ? firstMessage : body.title;
}

function isErrorBody(value: unknown): value is ErrorBody {
  return typeof value === 'object' && value !== null && 'errors' in value && 'title' in value && 'type' in value;
}
