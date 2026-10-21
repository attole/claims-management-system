import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ReserveHistoryStatus, WriteResult } from './api.model';
import { apiUrl } from './api-url';
import { ReservesService } from './reserves.service';

const RULES = [
  'S:API-GET-reserves',
  'S:API-POST-reserves',
  'S:API-POST-reserve-approve',
  'S:API-POST-reserve-reject',
  'S:API-POST-reserve-retract',
].join(', ');

describe(`ReservesService (${RULES})`, () => {
  let service: ReservesService;
  let http: HttpTestingController;

  const decided = { reserveHistoryId: 'h1', previousStatus: 'PendingApproval', newStatus: 'Approved' };

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(ReservesService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('get reads the reserves of the claim', () => {
    service.get('c1').subscribe();

    const request = http.expectOne(apiUrl('claims/c1/reserves'));
    expect(request.request.method).toBe('GET');
    request.flush({ approvedTotal: 0, components: [], history: [] });
  });

  it('submit posts the reserve with the given key', () => {
    const reserve = {
      transactionType: 'Add' as const,
      amount: 5000,
      changeReason: 'Initial',
      componentType: 'Indemnity' as const,
    };

    service.submit('c1', reserve, 'key-1').subscribe();

    const request = http.expectOne(apiUrl('claims/c1/reserves'));
    expect(request.request.body).toEqual(reserve);
    expect(request.request.headers.get('Idempotency-Key')).toBe('key-1');
    request.flush({ data: {}, warnings: [] });
  });

  it('approve posts no body with a key and returns the new status', () => {
    let result = {} as WriteResult<ReserveHistoryStatus>;

    service.approve('c1', 'h1', 'key-1').subscribe((value) => (result = value));

    const request = http.expectOne(apiUrl('claims/c1/reserves/h1/approve'));
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    expect(request.request.headers.get('Idempotency-Key')).toBe('key-1');
    request.flush({ data: decided, warnings: [] });
    expect(result.data.newStatus).toBe('Approved');
  });

  it('reject sends the rejection reason', () => {
    service.reject('c1', 'h1', 'Too high').subscribe();

    const request = http.expectOne(apiUrl('claims/c1/reserves/h1/reject'));
    expect(request.request.body).toEqual({ rejectionReason: 'Too high' });
    expect(request.request.headers.has('Idempotency-Key')).toBe(true);
    request.flush({ data: decided, warnings: [] });
  });

  it.each([
    ['retract', 'retract'],
    ['retryPosting', 'retry-posting'],
  ] as const)('%s posts no body with a key to the %s route', (method, route) => {
    service[method]('c1', 'h1').subscribe();

    const request = http.expectOne(apiUrl(`claims/c1/reserves/h1/${route}`));
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    expect(request.request.headers.has('Idempotency-Key')).toBe(true);
    request.flush({ data: decided, warnings: [] });
  });
});
