import { HttpErrorResponse } from '@angular/common/http';
import { describeError, readErrorBody } from './error-body';

describe('error-body (S:API-ErrorShape)', () => {
  it('readErrorBody returns the API error body as sent', () => {
    const body = { type: 'ConflictError', title: 'Conflict', status: 409, errors: { General: ['Changed meanwhile.'] } };

    const result = readErrorBody(new HttpErrorResponse({ status: 409, error: body }));

    expect(result).toEqual(body);
  });

  it('describeError for a body that is not JSON names the status, not the status text', () => {
    const error = new HttpErrorResponse({ status: 502, statusText: '', error: '<html>Bad gateway</html>' });

    const result = describeError(error);

    expect(result).toBe('The request failed with status 502.');
  });

  it('describeError for a framework problem details body without errors names the status', () => {
    const problem = { type: 'https://tools.ietf.org/html/rfc9110#section-15.5.16', title: 'Unsupported', status: 415 };

    const result = describeError(new HttpErrorResponse({ status: 415, error: problem }));

    expect(result).toBe('The request failed with status 415.');
  });

  it('describeError for an unreachable server says so', () => {
    const result = describeError(new HttpErrorResponse({ status: 0 }));

    expect(result).toContain('cannot be reached');
  });
});
