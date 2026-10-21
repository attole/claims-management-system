import { HttpEventType, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { UploadProgress } from './api.model';
import { apiUrl } from './api-url';
import { DocumentsService, EMPTY_UPLOAD_RESPONSE } from './documents.service';

describe('DocumentsService (S:API-POST-claims-documents, S:API-GET-claims-documents, S:UI-Documents)', () => {
  let service: DocumentsService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(DocumentsService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('list reads the documents of the claim with their links', () => {
    service.list('c1').subscribe();

    const request = http.expectOne(apiUrl('claims/c1/documents'));
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('upload sends the file and its fields as a form and reports progress then the result', () => {
    const file = new File([new Uint8Array([1, 2, 3])], 'report.pdf', { type: 'application/pdf' });
    const events: UploadProgress[] = [];

    service
      .upload('c1', { file, documentType: 'Report', notes: 'Site visit' }, 'key-1')
      .subscribe((event) => events.push(event));

    const request = http.expectOne(apiUrl('claims/c1/documents'));
    const form = request.request.body as FormData;
    request.event({ type: HttpEventType.UploadProgress, loaded: 50, total: 100 });
    request.flush({ data: { id: 'd1' }, warnings: [] });

    expect((form.get('file') as File).name).toBe('report.pdf');
    expect(form.get('documentType')).toBe('Report');
    expect(form.get('notes')).toBe('Site visit');
    expect(request.request.headers.get('Idempotency-Key')).toBe('key-1');
    expect(events[0]).toEqual({ kind: 'progress', percent: 50 });
    expect(events.at(-1)?.kind).toBe('done');
  });

  it('upload whose response has no body fails instead of reporting progress', () => {
    const file = new File([new Uint8Array([1])], 'report.pdf');
    let failure = '';

    service
      .upload('c1', { file, documentType: 'Report', notes: '' })
      .subscribe({ error: (error: Error) => (failure = error.message) });

    http.expectOne(apiUrl('claims/c1/documents')).flush(null);

    expect(failure).toBe(EMPTY_UPLOAD_RESPONSE);
  });
});
