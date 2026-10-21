import { ClaimDetail, Issue, Party } from '../../core/api/api.model';
import { DEV_USERS } from '../../core/auth/dev-users';
import { EMPTY_DETAIL } from './claim-detail.store';

export const CLAIM_ID = 'c1000000-0000-4000-8000-000000000001';

export const CLAIMANT: Party = {
  id: 'p1000000-0000-4000-8000-000000000001',
  claimId: CLAIM_ID,
  role: 'Claimant',
  type: 'Person',
  firstName: 'Maria',
  lastName: 'Lopez',
  companyName: '',
  email: 'maria@example.com',
  phone: '',
  notes: '',
  isActive: true,
};

export const RAISED_ISSUE: Issue = {
  id: 'i1000000-0000-4000-8000-000000000001',
  claimId: CLAIM_ID,
  code: 'S:BR-C-02',
  message: 'The loss date is outside the policy period.',
  field: 'LossDate',
  severity: 'Warning',
  state: 'Raised',
  resolvedAt: '0001-01-01T00:00:00+00:00',
  resolvedBy: '00000000-0000-0000-0000-000000000000',
};

export function claimDetail(): ClaimDetail {
  return {
    ...EMPTY_DETAIL,
    claim: {
      ...EMPTY_DETAIL.claim,
      id: CLAIM_ID,
      claimNumber: 'CLM-2026-0001039',
      state: 'Open',
      severity: 'Standard',
      policyId: 'a1000000-0000-4000-8000-000000000010',
      assignedHandlerId: DEV_USERS[0].id,
      notes: 'Plumber report requested.',
    },
    clientName: 'Harbor Logistics',
    policyNumber: 'POL-48111',
    loss: {
      ...EMPTY_DETAIL.loss,
      claimId: CLAIM_ID,
      lossDate: '2026-07-22T09:40:00+00:00',
      reportDate: '2026-07-23T08:00:00+00:00',
      description: 'Pipe burst under the kitchen floor.',
      causeOfLossCode: 'COL-WATER',
      hasEstimatedLossAmount: true,
      estimatedLossAmount: 62500,
    },
    parties: [CLAIMANT],
    issues: [RAISED_ISSUE],
  };
}
