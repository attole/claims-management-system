import { CalendarDate } from './api-dates';
import { components } from './api-schema';

type Schemas = components['schemas'];

export type ClaimStatus = Schemas['ClaimStateKind'];
export type PartyRole = Schemas['PartyRoleKind'];
export type PartyType = Schemas['PartyTypeKind'];
export type AssetType = Schemas['AssetType'];
export type ReserveComponentType = NonNullable<Schemas['NullableOfReserveComponentTypeKind']>;
export type ReserveTransactionType = Schemas['ReserveTransactionType'];
export type UserRole = 'Handler' | 'Supervisor' | 'Manager';
export type ReserveApprovalStatus = 'AutoApproved' | 'PendingApproval' | 'Approved' | 'Rejected' | 'Cancelled';
export type ReservePostingStatus = 'Pending' | 'Posted' | 'Failed' | 'Cancelled';
export type ReserveComponentState = 'Active' | 'Closed';
export type PolicyStatus = 'Active' | 'Expired' | 'Cancelled';

export type Warning = Schemas['Warning'];
export type ErrorBody = Schemas['ErrorBody'];

export interface WriteResult<TData> {
  data: TData;
  warnings: Warning[];
}

export interface PagedResult<TItem> {
  items: TItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export type ClaimSummary = Omit<Schemas['ClaimSummaryDto'], 'status'> & { status: ClaimStatus };
export type ClaimDetail = Schemas['ClaimDetailDto'];
export type Claim = Omit<ClaimDetail['claim'], 'state'> & { state: ClaimStatus };
export type LossDetails = ClaimDetail['loss'];
export type Party = ClaimDetail['parties'][number];
export type RiskObject = ClaimDetail['riskObjects'][number];
export type Issue = ClaimDetail['issues'][number];
export type ClaimReserves = Schemas['ClaimReservesDto'];
export type ReserveComponent = Omit<ClaimReserves['components'][number], 'state'> & { state: ReserveComponentState };
export type ReserveHistory = Omit<ClaimReserves['history'][number], 'approvalStatus' | 'postingStatus'> & {
  approvalStatus: ReserveApprovalStatus;
  postingStatus: ReservePostingStatus;
};
export type ReserveHistoryStatus = Omit<
  NonNullable<Schemas['ReserveHistoryStatusDto']>,
  'previousStatus' | 'newStatus'
> & {
  previousStatus: ReserveApprovalStatus | ReservePostingStatus;
  newStatus: ReserveApprovalStatus | ReservePostingStatus;
};
export type ClaimDocumentLink = Schemas['ClaimDocumentLinkDto'];
export type ClaimDocument = ClaimDocumentLink['document'];
export type AuditEntry = Schemas['AuditEntryDto'];
export type ClaimStatusChange = Omit<NonNullable<Schemas['ClaimStatusDto']>, 'previousStatus' | 'newStatus'> & {
  previousStatus: ClaimStatus;
  newStatus: ClaimStatus;
};
export type Policy = Omit<Schemas['PolicyDto'], 'status' | 'effectiveDate' | 'expirationDate'> & {
  status: PolicyStatus;
  effectiveDate: CalendarDate;
  expirationDate: CalendarDate;
};
export type CauseCode = Schemas['CauseCodeDto'];
export interface ClaimStatusInfo {
  status: ClaimStatus;
  nextStatuses: ClaimStatus[];
}
export type StatusCheck = Schemas['ClaimStatusCheckDto'];

export type CreateClaimRequest = Schemas['CreateClaimRequest'];
export type LossRequest = Schemas['LossRequest'];
export type PartyRequest = Schemas['PartyRequest'];
export type RiskObjectRequest = Schemas['RiskObjectRequest'];
export type ReserveRequest = Omit<Schemas['ReserveRequest'], 'isNewComponent' | 'hasComponentId'>;
export type ChangeStatusRequest = Schemas['ChangeClaimStatusRequest'];

export interface ClaimsFilter {
  statuses: ClaimStatus[];
  dateFrom: CalendarDate;
  dateTo: CalendarDate;
  assignedHandlerId: string;
  causeOfLossCode: string;
  policyId: string;
  search: string;
  page: number;
  pageSize: number;
}

export interface DocumentUpload {
  file: File;
  documentType: string;
  notes: string;
}

export type UploadProgress =
  { kind: 'progress'; percent: number } | { kind: 'done'; result: WriteResult<ClaimDocument> };

export type DevToken = Omit<Schemas['DevTokenDto'], 'role'> & { role: UserRole };
export type DevTokenRequest = Schemas['DevTokenRequest'];
