import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { apiUrl } from './api-url';
import { PartiesService } from './parties.service';

describe('PartiesService (S:API-POST-claims-parties, S:API-DELETE-claims-parties)', () => {
  let service: PartiesService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(PartiesService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('add posts the party to the claim', () => {
    const party = { role: 'Claimant' as const, type: 'Person' as const, firstName: 'Ada' };

    service.add('c1', party).subscribe();

    const request = http.expectOne(apiUrl('claims/c1/parties'));
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(party);
    request.flush({ data: {}, warnings: [] });
  });

  it('remove deletes the party of the claim', () => {
    service.remove('c1', 'p1').subscribe();

    const request = http.expectOne(apiUrl('claims/c1/parties/p1'));
    expect(request.request.method).toBe('DELETE');
    expect(request.request.headers.has('Idempotency-Key')).toBe(true);
    request.flush({ data: {}, warnings: [] });
  });
});
