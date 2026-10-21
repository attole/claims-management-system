import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, shareReplay } from 'rxjs';
import { CauseCode, ClaimStatusInfo } from './api.model';
import { apiUrl } from './api-url';

@Injectable({ providedIn: 'root' })
export class ReferenceService {
  private readonly http = inject(HttpClient);

  private readonly causeCodeList = this.http
    .get<CauseCode[]>(apiUrl('reference/cause-of-loss-codes'))
    .pipe(shareReplay({ bufferSize: 1, refCount: false }));

  private readonly statusList = this.http
    .get<ClaimStatusInfo[]>(apiUrl('reference/claim-statuses'))
    .pipe(shareReplay({ bufferSize: 1, refCount: false }));

  causeCodes(): Observable<CauseCode[]> {
    return this.causeCodeList;
  }

  claimStatuses(): Observable<ClaimStatusInfo[]> {
    return this.statusList;
  }
}
