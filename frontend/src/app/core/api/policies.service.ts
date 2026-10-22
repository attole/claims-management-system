import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResult, Policy } from './api.model';
import { apiUrl } from './api-url';

export const POLICY_SEARCH_PAGE_SIZE = 10;
export const POLICY_SEARCH_DELAY_MS = 300;
export const POLICY_SEARCH_MIN_LENGTH = 2;

@Injectable({ providedIn: 'root' })
export class PoliciesService {
  private readonly http = inject(HttpClient);

  search(text: string, pageSize = POLICY_SEARCH_PAGE_SIZE): Observable<PagedResult<Policy>> {
    const params = new HttpParams().set('q', text.trim()).set('page', 1).set('pageSize', pageSize);

    return this.http.get<PagedResult<Policy>>(apiUrl('policies/search'), { params });
  }
}
