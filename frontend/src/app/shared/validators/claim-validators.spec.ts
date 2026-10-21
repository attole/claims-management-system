import { FormArray, FormControl, FormGroup } from '@angular/forms';
import {
  atLeastOneClaimant,
  CLAIMANT_REQUIRED_ERROR,
  MIN_TRIMMED_LENGTH_ERROR,
  minTrimmedLength,
  NAME_REQUIRED_ERROR,
  NON_ZERO_ERROR,
  nonZero,
  NOT_IN_FUTURE_ERROR,
  notInFuture,
  oneOf,
  partyNameRequired,
  UNKNOWN_OPTION_ERROR,
} from './claim-validators';

describe('claim validators (S:BR-C-01, S:UI-Intake-Steps, S:UI-Intake-Parties)', () => {
  const now = () => new Date(2026, 9, 21, 12, 0);

  it('notInFuture flags a loss after now and accepts now', () => {
    const later = new FormControl(new Date(2026, 9, 21, 12, 1), notInFuture(now));
    const same = new FormControl(new Date(2026, 9, 21, 12, 0), notInFuture(now));

    expect(later.hasError(NOT_IN_FUTURE_ERROR)).toBe(true);
    expect(same.valid).toBe(true);
  });

  it('minTrimmedLength ignores surrounding spaces', () => {
    const control = new FormControl(`  ${'a'.repeat(19)}  `, minTrimmedLength(20));

    expect(control.getError(MIN_TRIMMED_LENGTH_ERROR)).toEqual({ requiredLength: 20, actualLength: 19 });
  });

  it('nonZero refuses zero and accepts a negative change', () => {
    const results = [new FormControl(0, nonZero).hasError(NON_ZERO_ERROR), new FormControl(-5, nonZero).valid];

    expect(results).toEqual([true, true]);
  });

  it('oneOf accepts only a listed option, and an empty value', () => {
    const options = () => ['COL-FIRE'];
    const results = [
      new FormControl('COL-WIND', oneOf(options)).hasError(UNKNOWN_OPTION_ERROR),
      new FormControl('COL-FIRE', oneOf(options)).valid,
      new FormControl('', oneOf(options)).valid,
    ];

    expect(results).toEqual([true, true, true]);
  });

  it('atLeastOneClaimant needs a claimant among the parties', () => {
    const party = (role: string) => new FormGroup({ role: new FormControl(role) });
    const witnesses = new FormArray([party('Witness')], atLeastOneClaimant);
    const withClaimant = new FormArray([party('Witness'), party('Claimant')], atLeastOneClaimant);

    expect(witnesses.hasError(CLAIMANT_REQUIRED_ERROR)).toBe(true);
    expect(withClaimant.valid).toBe(true);
  });

  it('partyNameRequired needs a person name or a company name', () => {
    const party = (type: string, firstName: string, companyName: string) =>
      new FormGroup(
        {
          type: new FormControl(type),
          firstName: new FormControl(firstName),
          companyName: new FormControl(companyName),
        },
        partyNameRequired,
      );

    const results = [
      party('Person', ' ', '').hasError(NAME_REQUIRED_ERROR),
      party('Person', 'Ada', '').valid,
      party('Company', 'Ada', '').hasError(NAME_REQUIRED_ERROR),
      party('Company', '', 'Harbor').valid,
    ];

    expect(results).toEqual([true, true, true, true]);
  });
});
