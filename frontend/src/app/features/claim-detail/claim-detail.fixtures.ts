import { ClaimDetail, Issue, Party, ReserveComponent, ReserveHistory, RiskObject } from '../../core/api/api.model';
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

export const PRIMARY_RISK_OBJECT: RiskObject = {
  id: 'o1000000-0000-4000-8000-000000000001',
  claimId: CLAIM_ID,
  assetType: 'Property',
  assetDescription: 'Warehouse office',
  damageDescription: 'Flooded floor',
  isPrimary: true,
  assetReference: '',
  isDeleted: false,
};

export const OTHER_RISK_OBJECT: RiskObject = {
  ...PRIMARY_RISK_OBJECT,
  id: 'o1000000-0000-4000-8000-000000000002',
  assetType: 'Equipment',
  assetDescription: 'Server rack',
  damageDescription: 'Water ingress',
  isPrimary: false,
};

export const INDEMNITY: ReserveComponent = {
  id: 'r1000000-0000-4000-8000-000000000001',
  claimId: CLAIM_ID,
  componentType: 'Indemnity',
  state: 'Active',
  approvedBalance: 45000,
  notes: '',
};

export function reserveHistory(change: Partial<ReserveHistory>): ReserveHistory {
  return {
    id: 'h1000000-0000-4000-8000-000000000001',
    reserveComponentId: INDEMNITY.id,
    claimId: CLAIM_ID,
    componentType: 'Indemnity',
    transactionType: 'Adjust',
    amount: 60000,
    changeReason: 'Contractor quote received.',
    changeSequence: 2,
    hasBalances: true,
    previousBalance: 45000,
    newBalance: 105000,
    approvalStatus: 'PendingApproval',
    postingStatus: 'Pending',
    submittedByUserId: DEV_USERS[0].id,
    submittedAt: '2026-08-09T10:00:00+00:00',
    reviewedBy: '00000000-0000-0000-0000-000000000000',
    reviewedAt: '0001-01-01T00:00:00+00:00',
    reviewReason: '',
    jobId: '',
    failureReason: '',
    idempotencyKey: '',
    ...change,
  };
}

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
    riskObjects: [PRIMARY_RISK_OBJECT, OTHER_RISK_OBJECT],
    issues: [RAISED_ISSUE],
  };
}
