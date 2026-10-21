import { enumLabel } from './enum-label.pipe';

describe('enumLabel (S:UI-Badges)', () => {
  it('splits a PascalCase name into words', () => {
    const result = enumLabel('UnderInvestigation');

    expect(result).toBe('Under Investigation');
  });

  it('keeps a single word and spells ALAE as the acronym', () => {
    const results = [enumLabel('Open'), enumLabel('Alae')];

    expect(results).toEqual(['Open', 'ALAE']);
  });
});
