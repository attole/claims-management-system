import { FormControl, FormGroup, Validators } from '@angular/forms';
import { FIELD_LENGTHS } from '../../core/api/api-limits';
import { AssetType, RiskObject, RiskObjectRequest } from '../../core/api/api.model';

export const ASSET_TYPES: readonly AssetType[] = ['Property', 'Vehicle', 'Equipment', 'Person', 'Other'];

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
    isPrimary: new FormControl(false, { nonNullable: true }),
  });
}

export type RiskObjectGroup = ReturnType<typeof createRiskObjectGroup>;
export type RiskObjectValue = ReturnType<RiskObjectGroup['getRawValue']>;

export function trimmedRiskObject(value: RiskObjectValue): RiskObjectValue {
  return {
    ...value,
    assetDescription: value.assetDescription.trim(),
    damageDescription: value.damageDescription.trim(),
    assetReference: value.assetReference.trim(),
  };
}

export function withPrimary(riskObject: RiskObject, isPrimary: boolean): RiskObjectRequest {
  return {
    assetType: riskObject.assetType,
    assetDescription: riskObject.assetDescription,
    damageDescription: riskObject.damageDescription,
    assetReference: riskObject.assetReference,
    isPrimary,
  };
}
