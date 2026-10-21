import { FormControl, FormGroup, Validators } from '@angular/forms';
import { FIELD_LENGTHS } from '../../core/api/api-limits';
import { PartyRole, PartyType } from '../../core/api/api.model';
import { partyNameRequired } from '../validators/claim-validators';

export const PARTY_ROLES: readonly PartyRole[] = ['Claimant', 'Insured', 'ThirdParty', 'Witness', 'Attorney'];
export const PARTY_TYPES: readonly PartyType[] = ['Person', 'Company'];

export function createPartyGroup() {
  return new FormGroup(
    {
      role: new FormControl<PartyRole>('Claimant', { nonNullable: true }),
      type: new FormControl<PartyType>('Person', { nonNullable: true }),
      firstName: new FormControl('', { nonNullable: true, validators: Validators.maxLength(FIELD_LENGTHS.firstName) }),
      lastName: new FormControl('', { nonNullable: true, validators: Validators.maxLength(FIELD_LENGTHS.lastName) }),
      companyName: new FormControl('', {
        nonNullable: true,
        validators: Validators.maxLength(FIELD_LENGTHS.companyName),
      }),
      email: new FormControl('', {
        nonNullable: true,
        validators: [Validators.email, Validators.maxLength(FIELD_LENGTHS.email)],
      }),
      phone: new FormControl('', { nonNullable: true, validators: Validators.maxLength(FIELD_LENGTHS.phone) }),
    },
    { validators: partyNameRequired },
  );
}

export type PartyGroup = ReturnType<typeof createPartyGroup>;
export type PartyValue = ReturnType<PartyGroup['getRawValue']>;

export interface PartyNameParts {
  type: string;
  firstName: string;
  lastName: string;
  companyName: string;
}

export function partyName(party: PartyNameParts): string {
  return party.type === 'Company' ? party.companyName : `${party.firstName} ${party.lastName}`.trim();
}

export function trimmedParty(value: PartyValue): PartyValue {
  const isCompany = value.type === 'Company';

  return {
    ...value,
    firstName: isCompany ? '' : value.firstName.trim(),
    lastName: isCompany ? '' : value.lastName.trim(),
    companyName: isCompany ? value.companyName.trim() : '',
    email: value.email.trim(),
    phone: value.phone.trim(),
  };
}
