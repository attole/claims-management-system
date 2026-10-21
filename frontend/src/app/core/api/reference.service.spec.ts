import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { apiUrl } from './api-url';
import { ReferenceService } from './reference.service';

describe('ReferenceService (S:API-GET-causes, S:API-GET-statuses)', () => {
  let service: ReferenceService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(ReferenceService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('causeCodes loads once and serves every later subscriber from the cache', () => {
    service.causeCodes().subscribe();
    service.causeCodes().subscribe();

    http.expectOne(apiUrl('reference/cause-of-loss-codes')).flush([{ code: 'COL-FIRE', name: 'Fire' }]);
    service.causeCodes().subscribe();

    http.expectNone(apiUrl('reference/cause-of-loss-codes'));
  });

  it('claimStatuses after a failed load tries again on the next call', () => {
    service.claimStatuses().subscribe({ error: () => undefined });
    http.expectOne(apiUrl('reference/claim-statuses')).flush(null, { status: 500, statusText: 'Error' });

    service.claimStatuses().subscribe();

    http.expectOne(apiUrl('reference/claim-statuses')).flush([{ status: 'Draft', nextStatuses: ['Open'] }]);
  });
});
