import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const DATE_RANGE_ERROR = 'dateRange';

export function dateRangeValidator(fromName: string, toName: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const from: unknown = group.get(fromName)?.value;
    const to: unknown = group.get(toName)?.value;
    const isReversed = from instanceof Date && to instanceof Date && to.getTime() < from.getTime();

    return isReversed ? { [DATE_RANGE_ERROR]: true } : null;
  };
}
