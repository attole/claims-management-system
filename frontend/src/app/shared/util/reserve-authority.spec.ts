import { authorityFor, canDecideReserve } from './reserve-authority';

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

describe('canDecideReserve (S:BR-R-02)', () => {
  it.each([
    ['Handler', 5_000, false],
    ['Supervisor', 100_000, true],
    ['Supervisor', -100_000.01, false],
    ['Manager', 5_000_000, true],
  ] as const)('a %s deciding %s is %s', (role, amount, expected) => {
    const result = canDecideReserve(role, amount);

    expect(result).toBe(expected);
  });
});
