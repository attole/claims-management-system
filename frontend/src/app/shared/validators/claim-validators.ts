import { AbstractControl, FormArray, ValidationErrors, ValidatorFn } from '@angular/forms';

export const NOT_IN_FUTURE_ERROR = 'notInFuture';
export const MIN_TRIMMED_LENGTH_ERROR = 'minTrimmedLength';
export const NON_ZERO_ERROR = 'nonZero';
export const CLAIMANT_REQUIRED_ERROR = 'claimantRequired';
export const NAME_REQUIRED_ERROR = 'nameRequired';
export const UNKNOWN_OPTION_ERROR = 'unknownOption';

export function notInFuture(now: () => Date = () => new Date()): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value: unknown = control.value;
    const isFuture = value instanceof Date && value.getTime() > now().getTime();

    return isFuture ? { [NOT_IN_FUTURE_ERROR]: true } : null;
  };
}

export function minTrimmedLength(minimum: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const actual = String(control.value ?? '').trim().length;

    return actual < minimum ? { [MIN_TRIMMED_LENGTH_ERROR]: { requiredLength: minimum, actualLength: actual } } : null;
  };
}

export const nonZero: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value: unknown = control.value;

  return typeof value === 'number' && value === 0 ? { [NON_ZERO_ERROR]: true } : null;
};

export function oneOf(options: () => readonly string[]): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = String(control.value ?? '');

    return value.length > 0 && !options().includes(value) ? { [UNKNOWN_OPTION_ERROR]: true } : null;
  };
}

export const atLeastOneClaimant: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const roles = control instanceof FormArray ? control.controls.map((party) => party.get('role')?.value) : [];

  return roles.includes('Claimant') ? null : { [CLAIMANT_REQUIRED_ERROR]: true };
};

export const partyNameRequired: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const text = (name: string) => String(control.get(name)?.value ?? '').trim();
  const isCompany = control.get('type')?.value === 'Company';
  const hasName = isCompany ? text('companyName').length > 0 : `${text('firstName')}${text('lastName')}`.length > 0;

  return hasName ? null : { [NAME_REQUIRED_ERROR]: true };
};
