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
export type ClaimSeverity = 'Unspecified' | 'Minor' | 'Standard' | 'Critical' | 'Catastrophic';
export type IssueState = 'Raised' | 'Cleared' | 'Acknowledged' | 'Waived';
export type IssueSeverity = 'Critical' | 'Warning';

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
type ClaimDetailDto = Schemas['ClaimDetailDto'];
export type Claim = Omit<ClaimDetailDto['claim'], 'state' | 'severity'> & {
  state: ClaimStatus;
  severity: ClaimSeverity;
};
export type LossDetails = ClaimDetailDto['loss'];
export type Party = Omit<ClaimDetailDto['parties'][number], 'role' | 'type'> & { role: PartyRole; type: PartyType };
export type RiskObject = ClaimDetailDto['riskObjects'][number];
export type Issue = Omit<ClaimDetailDto['issues'][number], 'state' | 'severity'> & {
  state: IssueState;
  severity: IssueSeverity;
};
export type ClaimDetail = Omit<ClaimDetailDto, 'claim' | 'parties' | 'issues' | 'reserves'> & {
  claim: Claim;
  parties: Party[];
  issues: Issue[];
  reserves: ClaimReserves;
};
type ClaimReservesDto = Schemas['ClaimReservesDto'];
export type ReserveComponent = Omit<ClaimReservesDto['components'][number], 'state' | 'componentType'> & {
  state: ReserveComponentState;
  componentType: ReserveComponentType;
};
export type ReserveHistory = Omit<
  ClaimReservesDto['history'][number],
  'approvalStatus' | 'postingStatus' | 'componentType' | 'transactionType'
> & {
  approvalStatus: ReserveApprovalStatus;
  postingStatus: ReservePostingStatus;
  componentType: ReserveComponentType;
  transactionType: ReserveTransactionType;
};
export type ClaimReserves = Omit<ClaimReservesDto, 'components' | 'history'> & {
  components: ReserveComponent[];
  history: ReserveHistory[];
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
