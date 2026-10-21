import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { idempotencyHeaders, newIdempotencyKey } from '../http/idempotency';
import {
  AuditEntry,
  ChangeStatusRequest,
  Claim,
  ClaimDetail,
  ClaimsFilter,
  ClaimStatus,
  ClaimStatusChange,
  ClaimSummary,
  CreateClaimRequest,
  LossDetails,
  LossRequest,
  PagedResult,
  StatusCheck,
  WriteResult,
} from './api.model';
import { apiUrl } from './api-url';

export const DEFAULT_PAGE_SIZE = 25;

@Injectable({ providedIn: 'root' })
export class ClaimsService {
  private readonly http = inject(HttpClient);

  list(filter: ClaimsFilter): Observable<PagedResult<ClaimSummary>> {
    return this.http.get<PagedResult<ClaimSummary>>(apiUrl('claims'), { params: toClaimsParams(filter) });
  }

  get(claimId: string): Observable<ClaimDetail> {
    return this.http.get<ClaimDetail>(apiUrl(`claims/${claimId}`));
  }

  create(request: CreateClaimRequest, key = newIdempotencyKey()): Observable<WriteResult<Claim>> {
    return this.http.post<WriteResult<Claim>>(apiUrl('claims'), request, { headers: idempotencyHeaders(key) });
  }

  changeStatus(
    claimId: string,
    request: ChangeStatusRequest,
    key = newIdempotencyKey(),
  ): Observable<WriteResult<ClaimStatusChange>> {
    const url = apiUrl(`claims/${claimId}/status`);

    return this.http.put<WriteResult<ClaimStatusChange>>(url, request, { headers: idempotencyHeaders(key) });
  }

  checkStatus(claimId: string, targetStatus: ClaimStatus): Observable<StatusCheck> {
    const params = new HttpParams().set('targetStatus', targetStatus);

    return this.http.get<StatusCheck>(apiUrl(`claims/${claimId}/status-check`), { params });
  }

  updateNotes(claimId: string, notes: string, key = newIdempotencyKey()): Observable<WriteResult<Claim>> {
    const url = apiUrl(`claims/${claimId}/notes`);

    return this.http.put<WriteResult<Claim>>(url, { notes }, { headers: idempotencyHeaders(key) });
  }

  assignHandler(claimId: string, handlerId: string, key = newIdempotencyKey()): Observable<WriteResult<Claim>> {
    const url = apiUrl(`claims/${claimId}/handler`);

    return this.http.put<WriteResult<Claim>>(url, { handlerId }, { headers: idempotencyHeaders(key) });
  }

  linkPolicy(claimId: string, policyId: string, key = newIdempotencyKey()): Observable<WriteResult<Claim>> {
    const url = apiUrl(`claims/${claimId}/policy`);

    return this.http.put<WriteResult<Claim>>(url, { policyId }, { headers: idempotencyHeaders(key) });
  }

  updateLoss(claimId: string, loss: LossRequest, key = newIdempotencyKey()): Observable<WriteResult<LossDetails>> {
    const url = apiUrl(`claims/${claimId}/loss`);

    return this.http.put<WriteResult<LossDetails>>(url, loss, { headers: idempotencyHeaders(key) });
  }

  listAudit(claimId: string, page: number, pageSize = DEFAULT_PAGE_SIZE): Observable<PagedResult<AuditEntry>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);

    return this.http.get<PagedResult<AuditEntry>>(apiUrl(`claims/${claimId}/audit`), { params });
  }
}

function toClaimsParams(filter: ClaimsFilter): HttpParams {
  const optional: [string, string][] = [
    ['dateFrom', filter.dateFrom],
    ['dateTo', filter.dateTo],
    ['assignedHandlerId', filter.assignedHandlerId],
    ['causeOfLossCode', filter.causeOfLossCode],
    ['policyId', filter.policyId],
    ['search', filter.search],
  ];
  const withValues = optional.filter(([, value]) => value.length > 0);
  const base = new HttpParams().set('page', filter.page).set('pageSize', filter.pageSize);
  const withOptional = withValues.reduce((params, [name, value]) => params.set(name, value), base);

  return filter.statuses.reduce((params, status) => params.append('status', status), withOptional);
}
