import { HttpEventType, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Policy } from '../../core/api/api.model';
import { apiUrl } from '../../core/api/api-url';
import { POLICY_SEARCH_DELAY_MS } from '../../core/api/policies.service';
import { CLAIM_ID, CLAIMANT, claimDetail, PRIMARY_RISK_OBJECT } from './claim-detail.fixtures';
import { ClaimDetailStore } from './claim-detail.store';

describe('ClaimDetailStore (S:UI-Detail-Header, S:UI-Audit, S:API-IdempotencyKey)', () => {
  let store: ClaimDetailStore;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [ClaimDetailStore, provideHttpClient(), provideHttpClientTesting()] });
    store = TestBed.inject(ClaimDetailStore);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    vi.useRealTimers();
  });

  async function load(): Promise<void> {
    store.claimId.set(CLAIM_ID);
    TestBed.tick();
    http.expectOne(apiUrl('reference/claim-statuses')).flush([{ status: 'Open', nextStatuses: ['Closed'] }]);
    http
      .expectOne(apiUrl('reference/cause-of-loss-codes'))
      .flush([{ code: 'COL-WATER', name: 'Water damage', perilCategory: 'Property' }]);
    http.expectOne(apiUrl(`claims/${CLAIM_ID}`)).flush(claimDetail());
    await settle();
    http
      .expectOne((call) => call.url === apiUrl(`claims/${CLAIM_ID}/audit`))
      .flush({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 });
    await settle();
  }

  async function settle(): Promise<void> {
    await Promise.resolve();
    TestBed.tick();
  }

  it('loads the claim, then its audit page, and derives the next statuses and the cause name', async () => {
    const loadedBefore = store.hasLoaded();

    await load();

    expect(loadedBefore).toBe(false);
    expect(store.hasLoaded()).toBe(true);
    expect(store.nextStatuses()).toEqual(['Closed']);
    expect(store.causeName()).toBe('Water damage');
  });

  it('a write that failed without an answer keeps its key for the retry, and a success reloads the claim', async () => {
    await load();
    store.removeParty(CLAIMANT).subscribe({ error: () => undefined });
    const refused = http.expectOne(apiUrl(`claims/${CLAIM_ID}/parties/${CLAIMANT.id}`));
    refused.flush(null, { status: 503, statusText: 'Service Unavailable' });

    store.removeParty(CLAIMANT).subscribe();
    const retried = http.expectOne(apiUrl(`claims/${CLAIM_ID}/parties/${CLAIMANT.id}`));
    retried.flush({ data: CLAIMANT, warnings: [] });
    await settle();

    expect(retried.request.headers.get('Idempotency-Key')).toBe(refused.request.headers.get('Idempotency-Key'));
    http.expectOne(apiUrl(`claims/${CLAIM_ID}`)).flush(claimDetail());
    http
      .expectOne((call) => call.url === apiUrl(`claims/${CLAIM_ID}/audit`))
      .flush({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 });
    expect(store.isSaving()).toBe(false);
  });

  it('a refused write drops its key, so a changed body after it is a new request', async () => {
    await load();
    store.saveNotes('First').subscribe({ error: () => undefined });
    const refused = http.expectOne(apiUrl(`claims/${CLAIM_ID}/notes`));
    refused.flush(null, { status: 422, statusText: 'Unprocessable Entity' });

    store.saveNotes('Second').subscribe({ error: () => undefined });

    const next = http.expectOne(apiUrl(`claims/${CLAIM_ID}/notes`));
    expect(next.request.headers.get('Idempotency-Key')).not.toBe(refused.request.headers.get('Idempotency-Key'));
    next.flush(null, { status: 500, statusText: 'Server Error' });
  });

  it('an upload keeps other writes free and reloads the claim only when it is done', async () => {
    await load();
    const file = new File(['x'], 'photo.png', { type: 'image/png' });
    const percents: number[] = [];
    store
      .uploadDocument({ file, documentType: '', notes: '' })
      .subscribe((progress) => percents.push(progress.kind === 'progress' ? progress.percent : 100));
    const upload = http.expectOne(apiUrl(`claims/${CLAIM_ID}/documents`));

    upload.event({ type: HttpEventType.UploadProgress, loaded: 50, total: 100 });
    TestBed.tick();
    http.expectNone(apiUrl(`claims/${CLAIM_ID}`));
    const busyWhileUploading = [store.isUploading(), store.isSaving()];
    upload.flush({ data: { documentName: 'photo.png' }, warnings: [] });
    await settle();

    http.expectOne(apiUrl(`claims/${CLAIM_ID}`)).flush(claimDetail());
    http
      .expectOne((call) => call.url === apiUrl(`claims/${CLAIM_ID}/audit`))
      .flush({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 });
    expect(percents).toEqual([50, 100]);
    expect(busyWhileUploading).toEqual([true, false]);
    expect(store.isUploading()).toBe(false);
  });

  it('keeps showing the claim when a reload fails', async () => {
    await load();
    store.reload();
    TestBed.tick();

    http.expectOne(apiUrl(`claims/${CLAIM_ID}`)).flush(null, { status: 500, statusText: 'Server Error' });
    http
      .expectOne((call) => call.url === apiUrl(`claims/${CLAIM_ID}/audit`))
      .flush({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 });
    await settle();

    expect(store.hasError()).toBe(true);
    expect(store.hasLoaded()).toBe(true);
  });

  it('searches policies to link only from two characters on, after a short pause', () => {
    vi.useFakeTimers();
    store.policyQuery.set('h');
    TestBed.tick();
    vi.advanceTimersByTime(POLICY_SEARCH_DELAY_MS);
    http.match(apiUrl('reference/claim-statuses')).forEach((call) => call.flush([]));
    http.match(apiUrl('reference/cause-of-loss-codes')).forEach((call) => call.flush([]));
    http.expectNone((call) => call.url === apiUrl('policies/search'));

    store.policyQuery.set('harbor');
    TestBed.tick();
    vi.advanceTimersByTime(POLICY_SEARCH_DELAY_MS);

    const search = http.expectOne((call) => call.url === apiUrl('policies/search'));
    expect(search.request.params.get('q')).toBe('harbor');
    search.flush({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 });
  });

  it('setting the primary sends the whole risk object, with its own key apart from a removal', async () => {
    await load();
    store.setPrimaryRiskObject(PRIMARY_RISK_OBJECT, false).subscribe({ error: () => undefined });
    const update = http.expectOne(apiUrl(`claims/${CLAIM_ID}/risk-objects/${PRIMARY_RISK_OBJECT.id}`));
    update.flush(null, { status: 503, statusText: 'Service Unavailable' });

    store.removeRiskObject(PRIMARY_RISK_OBJECT).subscribe({ error: () => undefined });
    const removal = http.expectOne(apiUrl(`claims/${CLAIM_ID}/risk-objects/${PRIMARY_RISK_OBJECT.id}`));
    removal.flush(null, { status: 503, statusText: 'Service Unavailable' });

    expect(update.request.method).toBe('PUT');
    expect(update.request.body).toEqual({
      assetType: 'Property',
      assetDescription: 'Warehouse office',
      damageDescription: 'Flooded floor',
      assetReference: '',
      isPrimary: false,
    });
    expect(removal.request.method).toBe('DELETE');
    expect(removal.request.headers.get('Idempotency-Key')).not.toBe(update.request.headers.get('Idempotency-Key'));
  });

  it('links the chosen policy and reloads the claim', async () => {
    await load();

    const policy: Policy = {
      id: 'p9',
      policyNumber: 'POL-48230',
      clientName: 'Northwind Foods',
      effectiveDate: '2026-03-01',
      expirationDate: '2027-02-28',
      effectiveEndDate: '2027-02-28',
      status: 'Active',
      coverageTypes: [],
    };

    store.linkPolicy(policy).subscribe();
    const link = http.expectOne(apiUrl(`claims/${CLAIM_ID}/policy`));
    link.flush({ data: claimDetail().claim, warnings: [] });
    await settle();

    expect(link.request.body).toEqual({ policyId: 'p9' });
    http.expectOne(apiUrl(`claims/${CLAIM_ID}`)).flush(claimDetail());
    http
      .expectOne((call) => call.url === apiUrl(`claims/${CLAIM_ID}/audit`))
      .flush({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 });
  });
});
