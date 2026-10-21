import { AbstractControl, FormArray, FormGroup } from '@angular/forms';
import { ErrorBody } from '../../core/api/api.model';
import { GENERAL_ERROR_KEY } from '../../core/http/error-body';

export const SERVER_ERROR_KEY = 'server';

export function applyServerErrors(form: AbstractControl, body: ErrorBody): string[] {
  const unmatched: string[] = [];
  clearServerErrors(form);

  for (const [key, messages] of Object.entries(body.errors)) {
    const control = key === GENERAL_ERROR_KEY ? form : findControl(form, key);

    if (control === form) {
      unmatched.push(...messages);
    } else {
      control.setErrors({ ...control.errors, [SERVER_ERROR_KEY]: messages.join(' ') });
      control.markAsTouched();
    }
  }

  return unmatched;
}

export function clearServerErrors(control: AbstractControl): void {
  const children = control instanceof FormGroup || control instanceof FormArray ? Object.values(control.controls) : [];
  children.forEach((child) => clearServerErrors(child));

  if (control.hasError(SERVER_ERROR_KEY)) {
    const rest = Object.fromEntries(Object.entries(control.errors ?? {}).filter(([key]) => key !== SERVER_ERROR_KEY));
    control.setErrors(Object.keys(rest).length > 0 ? rest : null);
  }
}

function findControl(form: AbstractControl, key: string): AbstractControl {
  const segments = key.split(/[.[\]]/).filter((segment) => segment.length > 0);
  let current = form;

  for (const segment of segments) {
    const child = childOf(current, segment);

    if (child === undefined) {
      return form;
    }

    current = child;
  }

  return current;
}

function childOf(control: AbstractControl, segment: string): AbstractControl | undefined {
  if (control instanceof FormArray) {
    const index = Number(segment);

    return Number.isInteger(index) && index >= 0 && index < control.length ? control.at(index) : undefined;
  }

  if (control instanceof FormGroup) {
    const name = Object.keys(control.controls).find((candidate) => candidate.toLowerCase() === segment.toLowerCase());

    return name === undefined ? undefined : control.controls[name];
  }

  return undefined;
}
