import { AbstractControl, FormArray, FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { calendarDateOf, NO_DATE, toIsoDateTime } from '../../core/api/api-dates';
import { FIELD_LENGTHS, MINIMUM_DESCRIPTION_LENGTH } from '../../core/api/api-limits';
import { AssetType, CreateClaimRequest, Policy, ReserveComponentType, ReserveRequest } from '../../core/api/api.model';
import { EMPTY_ID } from '../../core/api/api-ids';
import { PartyGroup, trimmedParty } from '../../shared/forms/party-form';
import {
  atLeastOneClaimant,
  minTrimmedLength,
  nonZero,
  notInFuture,
  oneOf,
} from '../../shared/validators/claim-validators';

export const POLICY_REQUIRED_ERROR = 'policyRequired';
export const AMOUNT_REQUIRED_ERROR = 'amountRequired';
export const RESERVE_NEEDS_POLICY_ERROR = 'reserveNeedsPolicy';
export const INITIAL_RESERVE_REASON = 'Initial reserve at intake';

export const ASSET_TYPES: readonly AssetType[] = ['Property', 'Vehicle', 'Equipment', 'Person', 'Other'];
export const RESERVE_COMPONENTS: readonly ReserveComponentType[] = [
  'Indemnity',
  'Expense',
  'Alae',
  'SubrogationRecoverable',
];

export type InForce = 'unknown' | 'inForce' | 'outside';

export function createRiskObjectGroup() {
  return new FormGroup({
    assetType: new FormControl<AssetType>('Property', { nonNullable: true }),
    assetDescription: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(FIELD_LENGTHS.assetDescription)],
    }),
    damageDescription: new FormControl('', { nonNullable: true }),
    assetReference: new FormControl('', {
      nonNullable: true,
      validators: Validators.maxLength(FIELD_LENGTHS.assetReference),
    }),
  });
}

export type RiskObjectGroup = ReturnType<typeof createRiskObjectGroup>;

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
      loss: new FormGroup({
        lossDate: new FormControl<Date | null>(null, [Validators.required, notInFuture(now)]),
        causeOfLossCode: new FormControl('', {
          nonNullable: true,
          validators: [Validators.required, oneOf(causeCodes)],
        }),
        description: new FormControl('', {
          nonNullable: true,
          validators: [Validators.required, minTrimmedLength(MINIMUM_DESCRIPTION_LENGTH)],
        }),
        location: new FormControl('', { nonNullable: true, validators: Validators.maxLength(FIELD_LENGTHS.location) }),
        estimatedLossAmount: new FormControl<number | null>(null, Validators.min(0)),
      }),
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

export function inForce(policy: Policy | null, lossDate: Date | null): InForce {
  if (!policy || !lossDate) {
    return 'unknown';
  }

  const day = calendarDateOf(toIsoDateTime(lossDate));
  const isInsidePeriod = day >= policy.effectiveDate && day <= policy.expirationDate;

  return policy.status === 'Active' && isInsidePeriod ? 'inForce' : 'outside';
}

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
    loss: {
      lossDate: value.loss.lossDate ? toIsoDateTime(value.loss.lossDate) : NO_DATE,
      description: value.loss.description.trim(),
      causeOfLossCode: value.loss.causeOfLossCode,
      location: value.loss.location.trim(),
      estimatedLossAmount: value.loss.estimatedLossAmount,
    },
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
