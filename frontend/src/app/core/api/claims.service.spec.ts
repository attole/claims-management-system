import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ClaimsFilter, CreateClaimRequest } from './api.model';
import { apiUrl } from './api-url';
import { ClaimsService } from './claims.service';

const RULES = [
  'S:API-GET-claims',
  'S:API-GET-claim',
  'S:API-POST-claims',
  'S:API-PUT-claims-status',
  'S:API-GET-claims-audit',
  'A:UI-General',
].join(', ');

describe(`ClaimsService (${RULES})`, () => {
  let service: ClaimsService;
  let http: HttpTestingController;

  const emptyFilter: ClaimsFilter = {
    statuses: [],
    dateFrom: '',
    dateTo: '',
    assignedHandlerId: '',
    causeOfLossCode: '',
    policyId: '',
    search: '',
    page: 1,
    pageSize: 25,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(ClaimsService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('list sends paging, repeats status and leaves empty filters out', () => {
    const filter = { ...emptyFilter, statuses: ['Open', 'Draft'] as ClaimsFilter['statuses'], search: 'CLM' };

    service.list(filter).subscribe();

    const request = http.expectOne((candidate) => candidate.url === apiUrl('claims'));
    expect(request.request.params.getAll('status')).toEqual(['Open', 'Draft']);
    expect(request.request.params.get('search')).toBe('CLM');
    expect(request.request.params.get('page')).toBe('1');
    expect(request.request.params.has('dateFrom')).toBe(false);
    request.flush({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  });

  it('create posts the request with the given idempotency key and returns the envelope', () => {
    let warnings = -1;
    const draft: CreateClaimRequest = {
      policyId: '00000000-0000-0000-0000-000000000000',
      reportDate: '2026-10-21T10:00:00Z',
      loss: { lossDate: '2026-10-20T08:00:00Z', description: 'Water damage in the kitchen', causeOfLossCode: '' },
      parties: [],
      riskObjects: [],
      reserves: [],
    };

    service.create(draft, 'key-1').subscribe((result) => {
      warnings = result.warnings.length;
    });

    const request = http.expectOne(apiUrl('claims'));
    expect(request.request.method).toBe('POST');
    expect(request.request.headers.get('Idempotency-Key')).toBe('key-1');
    request.flush({ data: { id: 'c1' }, warnings: [{ code: 'W', message: 'm', field: '' }] });
    expect(warnings).toBe(1);
  });

  it('a write without a key still sends a fresh idempotency key', () => {
    service.updateNotes('c1', 'Called the insured').subscribe();

    const request = http.expectOne(apiUrl('claims/c1/notes'));
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ notes: 'Called the insured' });
    expect(request.request.headers.get('Idempotency-Key')?.length).toBeGreaterThan(0);
    request.flush({ data: {}, warnings: [] });
  });

  it('changeStatus puts the target status to the status route', () => {
    service.changeStatus('c1', { targetStatus: 'Open' }).subscribe();

    const request = http.expectOne(apiUrl('claims/c1/status'));
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ targetStatus: 'Open' });
    request.flush({ data: {}, warnings: [] });
  });

  it('checkStatus asks the status check route for the target', () => {
    service.checkStatus('c1', 'Closed').subscribe();

    const request = http.expectOne((candidate) => candidate.url === apiUrl('claims/c1/status-check'));
    expect(request.request.params.get('targetStatus')).toBe('Closed');
    request.flush({ errors: [], warnings: [] });
  });

  it('the draft set-up writes go to their routes with their bodies', () => {
    service.assignHandler('c1', 'h1').subscribe();
    service.linkPolicy('c1', 'p1').subscribe();

    expect(http.expectOne(apiUrl('claims/c1/handler')).request.body).toEqual({ handlerId: 'h1' });
    expect(http.expectOne(apiUrl('claims/c1/policy')).request.body).toEqual({ policyId: 'p1' });
  });

  it('get reads the detail of the claim', () => {
    let claimNumber = '';

    service.get('c1').subscribe((detail) => (claimNumber = detail.claim.claimNumber));

    const request = http.expectOne(apiUrl('claims/c1'));
    expect(request.request.method).toBe('GET');
    request.flush({ claim: { claimNumber: 'CLM-2026-0000001' } });
    expect(claimNumber).toBe('CLM-2026-0000001');
  });

  it('updateLoss puts the loss details with a key', () => {
    const loss = { lossDate: '2026-10-20T08:00:00.000Z', description: 'Pipe burst', causeOfLossCode: 'COL-WATER' };

    service.updateLoss('c1', loss, 'key-1').subscribe();

    const request = http.expectOne(apiUrl('claims/c1/loss'));
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(loss);
    expect(request.request.headers.get('Idempotency-Key')).toBe('key-1');
    request.flush({ data: {}, warnings: [] });
  });

  it('listAudit sends the page and the page size', () => {
    service.listAudit('c1', 2).subscribe();

    const request = http.expectOne((candidate) => candidate.url === apiUrl('claims/c1/audit'));
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('25');
    request.flush({ items: [], page: 2, pageSize: 25, totalCount: 0, totalPages: 0 });
  });
});
