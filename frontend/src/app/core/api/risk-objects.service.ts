import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { idempotencyHeaders, newIdempotencyKey } from '../http/idempotency';
import { RiskObject, RiskObjectRequest, WriteResult } from './api.model';
import { apiUrl } from './api-url';

@Injectable({ providedIn: 'root' })
export class RiskObjectsService {
  private readonly http = inject(HttpClient);

  add(claimId: string, riskObject: RiskObjectRequest, key = newIdempotencyKey()): Observable<WriteResult<RiskObject>> {
    const url = apiUrl(`claims/${claimId}/risk-objects`);

    return this.http.post<WriteResult<RiskObject>>(url, riskObject, { headers: idempotencyHeaders(key) });
  }

  update(
    claimId: string,
    riskObjectId: string,
    riskObject: RiskObjectRequest,
    key = newIdempotencyKey(),
  ): Observable<WriteResult<RiskObject>> {
    const url = apiUrl(`claims/${claimId}/risk-objects/${riskObjectId}`);

    return this.http.put<WriteResult<RiskObject>>(url, riskObject, { headers: idempotencyHeaders(key) });
  }

  remove(claimId: string, riskObjectId: string, key = newIdempotencyKey()): Observable<WriteResult<RiskObject>> {
    const url = apiUrl(`claims/${claimId}/risk-objects/${riskObjectId}`);

    return this.http.delete<WriteResult<RiskObject>>(url, { headers: idempotencyHeaders(key) });
  }
}
