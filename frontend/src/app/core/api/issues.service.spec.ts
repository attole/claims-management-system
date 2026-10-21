import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Issue, WriteResult } from './api.model';
import { apiUrl } from './api-url';
import { IssuesService } from './issues.service';

describe('IssuesService (S:ST-Draft-Open)', () => {
  let service: IssuesService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(IssuesService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('acknowledge posts no body to the acknowledge route with a key and returns the issue', () => {
    let result = {} as WriteResult<Issue>;

    service.acknowledge('c1', 'i1', 'key-1').subscribe((value) => (result = value));

    const request = http.expectOne(apiUrl('claims/c1/issues/i1/acknowledge'));
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    expect(request.request.headers.get('Idempotency-Key')).toBe('key-1');
    request.flush({ data: { id: 'i1' }, warnings: [] });
    expect(result.data.id).toBe('i1');
  });

  it('waive posts no body to the waive route with a key', () => {
    service.waive('c1', 'i1').subscribe();

    const request = http.expectOne(apiUrl('claims/c1/issues/i1/waive'));
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    expect(request.request.headers.has('Idempotency-Key')).toBe(true);
    request.flush({ data: {}, warnings: [] });
  });
});
