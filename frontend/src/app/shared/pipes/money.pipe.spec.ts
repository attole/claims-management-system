import { formatMoney } from './money.pipe';

describe('formatMoney (S:UI-List)', () => {
  it('shows dollars with two decimals and thousands separators', () => {
    const result = formatMoney(17338);

    expect(result).toBe('$17,338.00');
  });

  it('shows a decrease with a minus sign', () => {
    const result = formatMoney(-500);

    expect(result).toBe('-$500.00');
  });

  it('shows a negative zero and a tiny negative amount without a minus sign', () => {
    const results = [formatMoney(-0), formatMoney(-0.004)];

    expect(results).toEqual(['$0.00', '$0.00']);
  });
});
