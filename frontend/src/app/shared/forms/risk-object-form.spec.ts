import { RiskObject } from '../../core/api/api.model';
import { createRiskObjectGroup, trimmedRiskObject, withPrimary } from './risk-object-form';

describe('risk object form (S:RISK-AssetTypes)', () => {
  it('needs an asset description and trims the text', () => {
    const group = createRiskObjectGroup();
    const isEmptyInvalid = group.invalid;

    group.patchValue({ assetDescription: '  Kitchen  ', damageDescription: ' Flooded ', assetReference: ' R-1 ' });

    expect(isEmptyInvalid).toBe(true);
    expect(trimmedRiskObject(group.getRawValue())).toEqual({
      assetType: 'Property',
      assetDescription: 'Kitchen',
      damageDescription: 'Flooded',
      assetReference: 'R-1',
      isPrimary: false,
    });
  });

  it('builds the update request of a saved risk object with the new primary flag', () => {
    const saved: RiskObject = {
      id: 'r1',
      claimId: 'c1',
      assetType: 'Vehicle',
      assetDescription: 'Van',
      damageDescription: 'Dented door',
      isPrimary: false,
      assetReference: 'KX-12',
      isDeleted: false,
    };

    const request = withPrimary(saved, true);

    expect(request).toEqual({
      assetType: 'Vehicle',
      assetDescription: 'Van',
      damageDescription: 'Dented door',
      assetReference: 'KX-12',
      isPrimary: true,
    });
  });
});
