import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { NotifyService } from '../notify/notify.service';
import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor (S:UI-General, A:UI-General)', () => {
  let client: HttpClient;
  let http: HttpTestingController;
  let notify: { error: ReturnType<typeof vi.fn>; success: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    notify = { error: vi.fn(), success: vi.fn() };
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: NotifyService, useValue: notify },
      ],
    });
    client = TestBed.inject(HttpClient);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('shows a 404 as a snackbar with the first message', () => {
    client.get('/api/claims/1').subscribe({ error: () => undefined });

    http
      .expectOne('/api/claims/1')
      .flush(
        { type: 'NotFoundError', title: 'Not found', status: 404, errors: { General: ['Claim was not found.'] } },
        { status: 404, statusText: 'Not Found' },
      );

    expect(notify.error).toHaveBeenCalledWith('Claim was not found.');
  });

  it('leaves a 422 to the form and shows nothing', () => {
    let status = 0;
    client.post('/api/claims', {}).subscribe({ error: (error: { status: number }) => (status = error.status) });

    http
      .expectOne('/api/claims')
      .flush(
        { type: 'ValidationError', title: 'Invalid', status: 422, errors: { ReportDate: ['Required.'] } },
        { status: 422, statusText: 'Unprocessable Entity' },
      );

    expect(notify.error).not.toHaveBeenCalled();
    expect(status).toBe(422);
  });

  it('shows an unreachable server as a snackbar', () => {
    client.get('/api/claims').subscribe({ error: () => undefined });

    http.expectOne('/api/claims').error(new ProgressEvent('error'), { status: 0 });

    expect(notify.error).toHaveBeenCalledWith(expect.stringContaining('cannot be reached'));
  });
});
