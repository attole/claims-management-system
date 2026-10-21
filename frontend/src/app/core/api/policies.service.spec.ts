import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { apiUrl } from './api-url';
import { PoliciesService } from './policies.service';

describe('PoliciesService (S:API-GET-policies-search, S:UI-Intake-Policy)', () => {
  it('search sends the trimmed text and the first page', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const http = TestBed.inject(HttpTestingController);

    TestBed.inject(PoliciesService).search('  harbor ').subscribe();

    const request = http.expectOne((candidate) => candidate.url === apiUrl('policies/search'));
    expect(request.request.params.get('q')).toBe('harbor');
    expect(request.request.params.get('page')).toBe('1');
    expect(request.request.params.get('pageSize')).toBe('10');
    request.flush({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 });
    http.verify();
  });
});
