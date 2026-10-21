import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CreateClaimRequest } from '../../core/api/api.model';
import { apiUrl } from '../../core/api/api-url';
import { IntakeStore, POLICY_SEARCH_DELAY_MS } from './intake.store';

describe('IntakeStore (S:UI-Intake-Policy, S:API-IdempotencyKey)', () => {
  let store: IntakeStore;
  let http: HttpTestingController;

  const request: CreateClaimRequest = {
    policyId: '',
    reportDate: '',
    loss: { lossDate: '', description: '', causeOfLossCode: '' },
    parties: [],
    riskObjects: [],
    reserves: [],
  };

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [IntakeStore, provideHttpClient(), provideHttpClientTesting()] });
    store = TestBed.inject(IntakeStore);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.match(apiUrl('reference/cause-of-loss-codes')).forEach((call) => call.flush([]));
    http.verify();
    vi.useRealTimers();
  });

  it('searches policies only from two characters on, after a short pause', () => {
    vi.useFakeTimers();
    store.policyQuery.set('h');
    TestBed.tick();
    vi.advanceTimersByTime(POLICY_SEARCH_DELAY_MS);
    http.expectNone((call) => call.url === apiUrl('policies/search'));

    store.policyQuery.set('harbor');
    TestBed.tick();
    vi.advanceTimersByTime(POLICY_SEARCH_DELAY_MS);

    const search = http.expectOne((call) => call.url === apiUrl('policies/search'));
    expect(search.request.params.get('q')).toBe('harbor');
    search.flush({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 });
  });

  it('a retry after a failure without an answer sends the same key, and a success starts a new one', () => {
    store.create(request).subscribe({ error: () => undefined });
    const failed = http.expectOne(apiUrl('claims'));
    failed.flush(null, { status: 503, statusText: 'Service Unavailable' });

    store.create(request).subscribe();
    const succeeded = http.expectOne(apiUrl('claims'));
    succeeded.flush({ data: { id: 'c1' }, warnings: [] });
    store.create(request).subscribe();
    const next = http.expectOne(apiUrl('claims'));
    next.flush({ data: { id: 'c2' }, warnings: [] });

    const key = (call: typeof failed) => call.request.headers.get('Idempotency-Key');
    expect(key(succeeded)).toBe(key(failed));
    expect(key(next)).not.toBe(key(succeeded));
    expect(store.isSubmitting()).toBe(false);
  });

  it('a refusal starts a new key, so the corrected form is a new request', () => {
    store.create(request).subscribe({ error: () => undefined });
    const refused = http.expectOne(apiUrl('claims'));
    refused.flush(null, { status: 422, statusText: 'Unprocessable Entity' });

    store.create(request).subscribe();

    const corrected = http.expectOne(apiUrl('claims'));
    expect(corrected.request.headers.get('Idempotency-Key')).not.toBe(refused.request.headers.get('Idempotency-Key'));
    corrected.flush({ data: { id: 'c1' }, warnings: [] });
  });
});
