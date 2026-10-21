import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { idempotencyHeaders, newIdempotencyKey } from '../http/idempotency';
import { ClaimReserves, ReserveHistory, ReserveHistoryStatus, ReserveRequest, WriteResult } from './api.model';
import { apiUrl } from './api-url';

@Injectable({ providedIn: 'root' })
export class ReservesService {
  private readonly http = inject(HttpClient);

  get(claimId: string): Observable<ClaimReserves> {
    return this.http.get<ClaimReserves>(apiUrl(`claims/${claimId}/reserves`));
  }

  submit(claimId: string, reserve: ReserveRequest, key = newIdempotencyKey()): Observable<WriteResult<ReserveHistory>> {
    const url = apiUrl(`claims/${claimId}/reserves`);

    return this.http.post<WriteResult<ReserveHistory>>(url, reserve, { headers: idempotencyHeaders(key) });
  }

  approve(
    claimId: string,
    historyId: string,
    key = newIdempotencyKey(),
  ): Observable<WriteResult<ReserveHistoryStatus>> {
    return this.decide(claimId, historyId, 'approve', null, key);
  }

  reject(
    claimId: string,
    historyId: string,
    rejectionReason: string,
    key = newIdempotencyKey(),
  ): Observable<WriteResult<ReserveHistoryStatus>> {
    return this.decide(claimId, historyId, 'reject', { rejectionReason }, key);
  }

  retract(
    claimId: string,
    historyId: string,
    key = newIdempotencyKey(),
  ): Observable<WriteResult<ReserveHistoryStatus>> {
    return this.decide(claimId, historyId, 'retract', null, key);
  }

  retryPosting(
    claimId: string,
    historyId: string,
    key = newIdempotencyKey(),
  ): Observable<WriteResult<ReserveHistoryStatus>> {
    return this.decide(claimId, historyId, 'retry-posting', null, key);
  }

  private decide(
    claimId: string,
    historyId: string,
    action: string,
    body: object | null,
    key: string,
  ): Observable<WriteResult<ReserveHistoryStatus>> {
    const url = apiUrl(`claims/${claimId}/reserves/${historyId}/${action}`);

    return this.http.post<WriteResult<ReserveHistoryStatus>>(url, body, { headers: idempotencyHeaders(key) });
  }
}
