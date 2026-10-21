import { IDEMPOTENCY_HEADER, idempotencyHeaders, newIdempotencyKey } from './idempotency';

describe('idempotency (S:API-IdempotencyKey)', () => {
  it('newIdempotencyKey gives a different key each call', () => {
    const first = newIdempotencyKey();
    const second = newIdempotencyKey();

    expect(first).not.toBe(second);
  });

  it('idempotencyHeaders carries the key under the API header name', () => {
    const headers = idempotencyHeaders('key-1');

    expect(headers.get(IDEMPOTENCY_HEADER)).toBe('key-1');
    expect(IDEMPOTENCY_HEADER).toBe('Idempotency-Key');
  });
});
