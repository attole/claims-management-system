import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { apiUrl } from '../api/api-url';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';
import { DEV_USERS } from './dev-users';

describe('authInterceptor (S:UI-General, A:UI-General)', () => {
  let client: HttpClient;
  let http: HttpTestingController;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptors([authInterceptor])), provideHttpClientTesting()],
    });
    client = TestBed.inject(HttpClient);
    http = TestBed.inject(HttpTestingController);
    await signIn('first');
  });

  afterEach(() => http.verify());

  it('adds the bearer token to an API request', () => {
    client.get(apiUrl('claims')).subscribe();

    const request = http.expectOne(apiUrl('claims'));

    expect(request.request.headers.get('Authorization')).toBe('Bearer first');
    request.flush({});
  });

  it('leaves a request to another host without a token', () => {
    client.get('https://example.com/data').subscribe();

    const request = http.expectOne('https://example.com/data');

    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({});
  });

  it('never sends a token on the dev token call', () => {
    TestBed.inject(AuthService).signInAs(DEV_USERS[1]).subscribe();

    const request = http.expectOne(apiUrl('dev/token'));

    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush(tokenFor(1, 'other'));
  });

  it('on a 401 signs in again once and repeats the request with the new token', async () => {
    const result = firstValueFrom(client.get<{ ok: boolean }>(apiUrl('claims')));
    http.expectOne(apiUrl('claims')).flush(null, { status: 401, statusText: 'Unauthorized' });
    http.expectOne(apiUrl('dev/token')).flush(tokenFor(0, 'second'));

    const retry = http.expectOne(apiUrl('claims'));
    retry.flush({ ok: true });

    expect(retry.request.headers.get('Authorization')).toBe('Bearer second');
    expect(await result).toEqual({ ok: true });
  });

  it('on two parallel 401s signs in only once and repeats both requests', () => {
    client.get(apiUrl('claims')).subscribe();
    client.get(apiUrl('policies')).subscribe();
    http.expectOne(apiUrl('claims')).flush(null, { status: 401, statusText: 'Unauthorized' });
    http.expectOne(apiUrl('policies')).flush(null, { status: 401, statusText: 'Unauthorized' });

    const tokenCalls = http.match(apiUrl('dev/token'));
    tokenCalls.forEach((call) => call.flush(tokenFor(0, 'second')));

    expect(tokenCalls.length).toBe(1);
    http.expectOne(apiUrl('claims')).flush({});
    http.expectOne(apiUrl('policies')).flush({});
  });

  async function signIn(token: string): Promise<void> {
    const signedIn = firstValueFrom(TestBed.inject(AuthService).signInAs(DEV_USERS[0]));
    http.expectOne(apiUrl('dev/token')).flush(tokenFor(0, token));
    await signedIn;
  }

  function tokenFor(index: number, token: string) {
    const user = DEV_USERS[index];

    return { token, expiresAt: '', userId: user.id, name: user.name, role: user.role };
  }
});
