import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { idempotencyHeaders, newIdempotencyKey } from '../http/idempotency';
import { Party, PartyRequest, WriteResult } from './api.model';
import { apiUrl } from './api-url';

@Injectable({ providedIn: 'root' })
export class PartiesService {
  private readonly http = inject(HttpClient);

  add(claimId: string, party: PartyRequest, key = newIdempotencyKey()): Observable<WriteResult<Party>> {
    const url = apiUrl(`claims/${claimId}/parties`);

    return this.http.post<WriteResult<Party>>(url, party, { headers: idempotencyHeaders(key) });
  }

  remove(claimId: string, partyId: string, key = newIdempotencyKey()): Observable<WriteResult<Party>> {
    const url = apiUrl(`claims/${claimId}/parties/${partyId}`);

    return this.http.delete<WriteResult<Party>>(url, { headers: idempotencyHeaders(key) });
  }
}
