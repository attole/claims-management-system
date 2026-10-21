import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { idempotencyHeaders, newIdempotencyKey } from '../http/idempotency';
import { Issue, WriteResult } from './api.model';
import { apiUrl } from './api-url';

@Injectable({ providedIn: 'root' })
export class IssuesService {
  private readonly http = inject(HttpClient);

  acknowledge(claimId: string, issueId: string, key = newIdempotencyKey()): Observable<WriteResult<Issue>> {
    return this.resolve(claimId, issueId, 'acknowledge', key);
  }

  waive(claimId: string, issueId: string, key = newIdempotencyKey()): Observable<WriteResult<Issue>> {
    return this.resolve(claimId, issueId, 'waive', key);
  }

  private resolve(claimId: string, issueId: string, action: string, key: string): Observable<WriteResult<Issue>> {
    const url = apiUrl(`claims/${claimId}/issues/${issueId}/${action}`);

    return this.http.post<WriteResult<Issue>>(url, null, { headers: idempotencyHeaders(key) });
  }
}
