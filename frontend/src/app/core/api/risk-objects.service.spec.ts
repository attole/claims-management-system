import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { apiUrl } from './api-url';
import { RiskObjectsService } from './risk-objects.service';

describe('RiskObjectsService (A:UI-General)', () => {
  let service: RiskObjectsService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(RiskObjectsService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('add posts the risk object to the claim', () => {
    const riskObject = { assetType: 'Property' as const, assetDescription: 'Kitchen' };

    service.add('c1', riskObject).subscribe();

    const request = http.expectOne(apiUrl('claims/c1/risk-objects'));
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(riskObject);
    request.flush({ data: {}, warnings: [] });
  });

  it('update puts the risk object with a key', () => {
    const riskObject = { assetType: 'Property' as const, assetDescription: 'Kitchen', isPrimary: true };

    service.update('c1', 'r1', riskObject, 'key-1').subscribe();

    const request = http.expectOne(apiUrl('claims/c1/risk-objects/r1'));
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(riskObject);
    expect(request.request.headers.get('Idempotency-Key')).toBe('key-1');
    request.flush({ data: {}, warnings: [] });
  });

  it('remove deletes the risk object of the claim', () => {
    service.remove('c1', 'r1').subscribe();

    const request = http.expectOne(apiUrl('claims/c1/risk-objects/r1'));
    expect(request.request.method).toBe('DELETE');
    expect(request.request.headers.has('Idempotency-Key')).toBe(true);
    request.flush({ data: {}, warnings: [] });
  });
});
