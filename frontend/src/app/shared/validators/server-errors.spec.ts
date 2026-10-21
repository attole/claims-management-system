import { FormArray, FormControl, FormGroup } from '@angular/forms';
import { ErrorBody } from '../../core/api/api.model';
import { applyServerErrors, SERVER_ERROR_KEY } from './server-errors';

describe('applyServerErrors (S:API-ErrorShape, S:UI-Intake-Submit)', () => {
  function createForm() {
    return new FormGroup({
      reportDate: new FormControl(''),
      loss: new FormGroup({ description: new FormControl('') }),
      parties: new FormArray([new FormGroup({ firstName: new FormControl('') })]),
    });
  }

  function bodyWith(errors: Record<string, string[]>): ErrorBody {
    return { type: 'ValidationError', title: 'Invalid', status: 422, errors };
  }

  it('puts a dotted key on its control whatever the casing', () => {
    const form = createForm();

    applyServerErrors(form, bodyWith({ 'Loss.Description': ['Too short.'] }));

    expect(form.controls.loss.controls.description.errors?.[SERVER_ERROR_KEY]).toBe('Too short.');
  });

  it('follows an array index to the right row', () => {
    const form = createForm();

    applyServerErrors(form, bodyWith({ 'Parties[0].FirstName': ['Required.'] }));

    expect(form.controls.parties.at(0).controls.firstName.errors?.[SERVER_ERROR_KEY]).toBe('Required.');
  });

  it('a second response clears the server errors the first one set', () => {
    const form = createForm();
    applyServerErrors(form, bodyWith({ ReportDate: ['Required.'] }));

    applyServerErrors(form, bodyWith({ 'Loss.Description': ['Too short.'] }));

    expect(form.controls.reportDate.errors).toBeNull();
    expect(form.controls.loss.controls.description.hasError(SERVER_ERROR_KEY)).toBe(true);
  });

  it('returns General and unknown keys for the banner and marks no control', () => {
    const form = createForm();

    const result = applyServerErrors(form, bodyWith({ General: ['Refused.'], 'Loss.Unknown': ['Odd.'] }));

    expect(result).toEqual(['Refused.', 'Odd.']);
    expect(form.controls.loss.controls.description.errors).toBeNull();
  });
});
