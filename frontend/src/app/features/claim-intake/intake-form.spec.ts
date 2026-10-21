import { Policy } from '../../core/api/api.model';
import {
  AMOUNT_REQUIRED_ERROR,
  createIntakeForm,
  createPartyGroup,
  createRiskObjectGroup,
  inForce,
  INITIAL_RESERVE_REASON,
  intakeWarnings,
  IntakeForm,
  POLICY_REQUIRED_ERROR,
  RESERVE_NEEDS_POLICY_ERROR,
  toCreateRequest,
} from './intake-form';

describe('intake form (S:UI-Intake-Steps, S:UI-Intake-Policy, S:UI-Intake-Reserve, S:API-POST-claims)', () => {
  const now = () => new Date(2026, 9, 21, 12, 0);
  const policy: Policy = {
    id: 'a1000000-0000-4000-8000-000000000010',
    policyNumber: 'POL-48111',
    clientName: 'Harbor Logistics',
    effectiveDate: '2026-01-01',
    expirationDate: '2026-12-31',
    status: 'Active',
    coverageTypes: [],
  };

  function filledForm(): IntakeForm {
    const form = createIntakeForm(() => ['COL-WATER'], now);
    form.controls.policy.patchValue({ selected: policy });
    form.controls.loss.patchValue({
      lossDate: new Date(Date.UTC(2026, 6, 22, 9, 40)),
      causeOfLossCode: 'COL-WATER',
      description: '  Pipe burst under the kitchen floor  ',
      location: ' Dockside ',
    });
    const claimant = createPartyGroup();
    claimant.patchValue({ role: 'Claimant', firstName: 'Ada', phone: ' +1 555 0100 ' });
    form.controls.parties.push(claimant);

    return form;
  }

  it('a filled form is valid and maps to the create request with trimmed text', () => {
    const form = filledForm();

    const request = toCreateRequest(form.getRawValue(), new Date(Date.UTC(2026, 9, 21, 10, 0)));

    expect(form.valid).toBe(true);
    expect(request.policyId).toBe(policy.id);
    expect(request.reportDate).toBe('2026-10-21T10:00:00.000Z');
    expect(request.loss).toEqual(
      expect.objectContaining({
        lossDate: '2026-07-22T09:40:00.000Z',
        description: 'Pipe burst under the kitchen floor',
      }),
    );
    expect(request.parties[0].phone).toBe('+1 555 0100');
    expect(request.reserves).toEqual([]);
  });

  it('needs a policy or the unknown policy switch', () => {
    const form = filledForm();
    form.controls.policy.patchValue({ selected: null });
    const withoutPolicy = form.controls.policy.hasError(POLICY_REQUIRED_ERROR);

    form.controls.policy.patchValue({ isUnknown: true });

    expect(withoutPolicy).toBe(true);
    expect(form.controls.policy.valid).toBe(true);
    expect(toCreateRequest(form.getRawValue(), now()).policyId).toBe('00000000-0000-0000-0000-000000000000');
  });

  it('a cause that is not in the list and a future loss date are refused', () => {
    const form = filledForm();

    form.controls.loss.patchValue({ causeOfLossCode: 'COL-BOGUS', lossDate: new Date(2026, 9, 22) });

    expect(form.controls.loss.controls.causeOfLossCode.invalid).toBe(true);
    expect(form.controls.loss.controls.lossDate.invalid).toBe(true);
  });

  it('a reserve becomes one Add with its component and needs an amount and a policy', () => {
    const form = filledForm();
    form.controls.reserve.patchValue({ componentType: 'Indemnity' });
    const missingAmount = form.controls.reserve.hasError(AMOUNT_REQUIRED_ERROR);
    form.controls.reserve.patchValue({ amount: 5000 });

    const request = toCreateRequest(form.getRawValue(), now());
    form.controls.policy.patchValue({ selected: null, isUnknown: true });

    expect(missingAmount).toBe(true);
    expect(request.reserves).toEqual([
      { transactionType: 'Add', amount: 5000, changeReason: INITIAL_RESERVE_REASON, componentType: 'Indemnity' },
    ]);
    expect(form.hasError(RESERVE_NEEDS_POLICY_ERROR)).toBe(true);
  });

  it('inForce is green only for an active policy covering the UTC day of the loss', () => {
    const results = [
      inForce(policy, new Date(Date.UTC(2026, 6, 22))),
      inForce(policy, new Date(Date.UTC(2027, 0, 2))),
      inForce({ ...policy, status: 'Expired' }, new Date(Date.UTC(2026, 6, 22))),
      inForce(null, new Date()),
    ];

    expect(results).toEqual(['inForce', 'outside', 'outside', 'unknown']);
  });

  it('warns about a missing policy and a missing risk object, and not when both are there', () => {
    const form = filledForm();
    const complete = createRiskObjectGroup();
    complete.patchValue({ assetDescription: 'Kitchen' });
    const before = intakeWarnings(form.getRawValue());

    form.controls.riskObjects.push(complete);

    expect(before).toEqual(['No risk object was added.']);
    expect(intakeWarnings(form.getRawValue())).toEqual([]);
  });
});
