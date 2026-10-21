import { createPartyGroup, partyName, trimmedParty } from './party-form';

describe('party form (S:UI-Intake-Parties)', () => {
  it('a party needs a name for its type', () => {
    const party = createPartyGroup();
    const withoutName = party.valid;

    party.patchValue({ firstName: 'Ada' });

    expect(withoutName).toBe(false);
    expect(party.valid).toBe(true);
  });

  it('trimmedParty keeps only the name fields of its type and trims the rest', () => {
    const party = createPartyGroup();
    party.patchValue({ type: 'Company', firstName: 'Ada', companyName: ' Harbor ', phone: ' +1 555 0100 ' });

    const result = trimmedParty(party.getRawValue());

    expect(result).toEqual(
      expect.objectContaining({ firstName: '', lastName: '', companyName: 'Harbor', phone: '+1 555 0100' }),
    );
  });

  it('partyName shows the company name or the person name', () => {
    const names = [
      partyName({ type: 'Company', firstName: 'Ada', lastName: '', companyName: 'Harbor' }),
      partyName({ type: 'Person', firstName: 'Ada', lastName: 'Lovelace', companyName: '' }),
    ];

    expect(names).toEqual(['Harbor', 'Ada Lovelace']);
  });
});
