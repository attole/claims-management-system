import { authorityFor } from './reserve-authority';

describe('authorityFor (S:BR-R-02, S:UI-Intake-Reserve)', () => {
  it.each([
    [0, 'none'],
    [1, 'auto'],
    [10_000, 'auto'],
    [10_000.01, 'supervisor'],
    [100_000, 'supervisor'],
    [100_000.01, 'manager'],
    [-60_000, 'supervisor'],
  ])('an amount of %s needs %s approval', (amount, authority) => {
    const result = authorityFor(amount);

    expect(result).toBe(authority);
  });
});
