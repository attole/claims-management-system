import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { App } from './app';
import { apiUrl } from './core/api/api-url';
import { AuthService } from './core/auth/auth.service';
import { DEV_USERS } from './core/auth/dev-users';

describe('App (S:UI-General)', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
  });

  it('renders the header and the page outlet', async () => {
    const fixture = TestBed.createComponent(App);

    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('app-header')).not.toBeNull();
    expect(element.querySelector('router-outlet')).not.toBeNull();
  });

  it('asks for the access code when the sign-in needs one, and uses the answer', async () => {
    const auth = TestBed.inject(AuthService);
    const useAccessCode = vi.spyOn(auth, 'useAccessCode').mockReturnValue(new Promise<void>(() => undefined));
    const open = vi
      .spyOn(TestBed.inject(MatDialog), 'open')
      .mockReturnValue({ afterClosed: () => of('code') } as never);
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const refused = auth.switchTo(DEV_USERS[0]).catch(() => undefined);
    TestBed.inject(HttpTestingController)
      .expectOne(apiUrl('dev/token'))
      .flush(null, { status: 403, statusText: 'Forbidden' });
    await refused;

    await fixture.whenStable();

    expect(open).toHaveBeenCalledOnce();
    expect(useAccessCode).toHaveBeenCalledWith('code');
  });
});
