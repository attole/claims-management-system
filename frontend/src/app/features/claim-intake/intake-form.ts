import { AbstractControl, FormArray, FormControl, FormGroup, ValidationErrors } from '@angular/forms';
import { toIsoDateTime } from '../../core/api/api-dates';
import { CreateClaimRequest, Policy, ReserveComponentType, ReserveRequest } from '../../core/api/api.model';
import { EMPTY_ID } from '../../core/api/api-ids';
import { createLossGroup, toLossRequest } from '../../shared/forms/loss-form';
import { PartyGroup, trimmedParty } from '../../shared/forms/party-form';
import { RiskObjectGroup } from '../../shared/forms/risk-object-form';
import { inForce } from '../../shared/util/in-force';
import { atLeastOneClaimant, nonZero } from '../../shared/validators/claim-validators';

export const POLICY_REQUIRED_ERROR = 'policyRequired';
export const AMOUNT_REQUIRED_ERROR = 'amountRequired';
export const RESERVE_NEEDS_POLICY_ERROR = 'reserveNeedsPolicy';
export const INITIAL_RESERVE_REASON = 'Initial reserve at intake';

export function createIntakeForm(causeCodes: () => readonly string[], now: () => Date = () => new Date()) {
  return new FormGroup(
    {
      policy: new FormGroup(
        {
          selected: new FormControl<Policy | null>(null),
          isUnknown: new FormControl(false, { nonNullable: true }),
        },
        { validators: policyChosen },
      ),
      loss: createLossGroup(causeCodes, now),
      parties: new FormArray<PartyGroup>([], atLeastOneClaimant),
      riskObjects: new FormArray<RiskObjectGroup>([]),
      reserve: new FormGroup(
        {
          componentType: new FormControl<ReserveComponentType | ''>('', { nonNullable: true }),
          amount: new FormControl<number | null>(null, nonZero),
        },
        { validators: amountWithComponent },
      ),
    },
    { validators: reserveNeedsPolicy },
  );
}

export type IntakeForm = ReturnType<typeof createIntakeForm>;
export type IntakeValue = ReturnType<IntakeForm['getRawValue']>;

export function intakeWarnings(value: IntakeValue): string[] {
  const warnings = [
    value.policy.selected ? '' : 'No policy is linked to the claim.',
    inForce(value.policy.selected, value.loss.lossDate) === 'outside'
      ? 'The loss date is outside the policy period, or the policy is not active.'
      : '',
    value.riskObjects.length > 0 ? '' : 'No risk object was added.',
  ];

  return warnings.filter((warning) => warning.length > 0);
}

export function toCreateRequest(value: IntakeValue, reportedAt: Date): CreateClaimRequest {
  return {
    policyId: value.policy.selected?.id ?? EMPTY_ID,
    reportDate: toIsoDateTime(reportedAt),
    loss: toLossRequest(value.loss),
    parties: value.parties.map((party) => trimmedParty(party)),
    riskObjects: value.riskObjects,
    reserves: reservesOf(value),
  };
}

function reservesOf(value: IntakeValue): ReserveRequest[] {
  const { componentType, amount } = value.reserve;

  if (componentType === '' || amount === null || amount === 0) {
    return [];
  }

  return [{ transactionType: 'Add', amount, changeReason: INITIAL_RESERVE_REASON, componentType }];
}

function amountWithComponent(group: AbstractControl): ValidationErrors | null {
  const hasComponent = group.get('componentType')?.value !== '';
  const hasAmount = group.get('amount')?.value !== null;

  return hasComponent && !hasAmount ? { [AMOUNT_REQUIRED_ERROR]: true } : null;
}

function policyChosen(group: AbstractControl): ValidationErrors | null {
  const hasPolicy = group.get('selected')?.value !== null;
  const isUnknown = group.get('isUnknown')?.value === true;

  return hasPolicy || isUnknown ? null : { [POLICY_REQUIRED_ERROR]: true };
}

function reserveNeedsPolicy(form: AbstractControl): ValidationErrors | null {
  const hasComponent = form.get('reserve.componentType')?.value !== '';
  const hasPolicy = form.get('policy.selected')?.value !== null;

  return hasComponent && !hasPolicy ? { [RESERVE_NEEDS_POLICY_ERROR]: true } : null;
}
