import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { apiUrl } from '../api/api-url';
import { AuthService } from './auth.service';
import { DEV_USERS } from './dev-users';

describe('AuthService (S:UI-General)', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('signInAs posts the user id and keeps the token and the user', async () => {
    const supervisor = DEV_USERS[1];
    const signedIn = firstValueFrom(service.signInAs(supervisor));

    const request = http.expectOne(apiUrl('dev/token'));
    request.flush({ token: 'jwt', expiresAt: '', userId: supervisor.id, name: supervisor.name, role: 'Supervisor' });
    await signedIn;

    expect(request.request.body).toEqual({ userId: supervisor.id });
    expect(service.token()).toBe('jwt');
    expect(service.role()).toBe('Supervisor');
    expect(sessionStorage.getItem('claims.userId')).toBe(supervisor.id);
  });

  it('initialize signs in as the stored user', async () => {
    const manager = DEV_USERS[2];
    sessionStorage.setItem('claims.userId', manager.id);
    const initialized = service.initialize();

    const request = http.expectOne(apiUrl('dev/token'));
    request.flush({ token: 'jwt', expiresAt: '', userId: manager.id, name: manager.name, role: 'Manager' });
    await initialized;

    expect(request.request.body).toEqual({ userId: manager.id });
    expect(service.user()).toEqual(manager);
  });

  it('signInAs takes the name and role from the token response', async () => {
    const handler = DEV_USERS[0];
    const signedIn = firstValueFrom(service.signInAs(handler));

    http
      .expectOne(apiUrl('dev/token'))
      .flush({ token: 'jwt', expiresAt: '', userId: handler.id, name: 'Server Name', role: 'Supervisor' });
    await signedIn;

    expect(service.user()).toEqual({ id: handler.id, name: 'Server Name', role: 'Supervisor' });
  });

  it('a sign-in that finishes after a newer one does not replace the newer user', async () => {
    const [handler, supervisor] = DEV_USERS;
    const older = firstValueFrom(service.signInAs(handler));
    const newer = firstValueFrom(service.signInAs(supervisor));
    const [olderCall, newerCall] = http.match(apiUrl('dev/token'));

    newerCall.flush({ token: 'new', expiresAt: '', userId: supervisor.id, name: supervisor.name, role: 'Supervisor' });
    olderCall.flush({ token: 'old', expiresAt: '', userId: handler.id, name: handler.name, role: 'Handler' });
    await Promise.all([older, newer]);

    expect(service.token()).toBe('new');
    expect(service.role()).toBe('Supervisor');
  });

  it('initialize when the API is down keeps the user signed out', async () => {
    const initialized = service.initialize();

    http.expectOne(apiUrl('dev/token')).flush(null, { status: 0, statusText: 'Unknown Error' });
    await initialized;

    expect(service.isSignedIn()).toBe(false);
  });
});
