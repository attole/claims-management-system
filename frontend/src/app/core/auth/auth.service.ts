import { HttpClient, HttpErrorResponse, HttpHeaders, HttpStatusCode } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { finalize, firstValueFrom, map, Observable, of, shareReplay, tap } from 'rxjs';
import { DevToken, DevTokenRequest } from '../api/api.model';
import { apiUrl } from '../api/api-url';
import { DEV_USERS, DevUser } from './dev-users';

export const DEV_TOKEN_PATH = 'dev/token';
export const DEV_AUTH_SECRET_HEADER = 'X-DevAuth-Secret';

const USER_KEY = 'claims.userId';
const ACCESS_CODE_KEY = 'claims.accessCode';
const NO_REFRESH: Observable<void> = of(undefined);

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly currentUser = signal<DevUser>(DEV_USERS[0]);
  private readonly currentToken = signal('');
  private readonly accessCode = signal(readStored(ACCESS_CODE_KEY));
  private readonly isAccessCodeNeeded = signal(false);
  private signInVersion = 0;
  private pendingRefresh = NO_REFRESH;

  readonly user = this.currentUser.asReadonly();
  readonly token = this.currentToken.asReadonly();
  readonly role = computed(() => this.currentUser().role);
  readonly isSignedIn = computed(() => this.currentToken().length > 0);
  readonly needsAccessCode = this.isAccessCodeNeeded.asReadonly();

  signInAs(user: DevUser): Observable<void> {
    const request: DevTokenRequest = { userId: user.id };
    const version = ++this.signInVersion;
    const code = this.accessCode();
    const headers = code.length > 0 ? new HttpHeaders({ [DEV_AUTH_SECRET_HEADER]: code }) : new HttpHeaders();
    this.isAccessCodeNeeded.set(false);

    return this.http.post<DevToken>(apiUrl(DEV_TOKEN_PATH), request, { headers }).pipe(
      tap({
        next: (devToken) => {
          if (version === this.signInVersion) {
            this.currentUser.set({ id: devToken.userId, name: devToken.name, role: devToken.role });
            this.currentToken.set(devToken.token);
            writeStored(USER_KEY, devToken.userId);
          }
        },
        error: (error: unknown) => this.isAccessCodeNeeded.set(isAccessRefused(error)),
      }),
      map(() => undefined),
    );
  }

  refresh(): Observable<void> {
    if (this.pendingRefresh === NO_REFRESH) {
      this.pendingRefresh = this.signInAs(this.currentUser()).pipe(
        finalize(() => (this.pendingRefresh = NO_REFRESH)),
        shareReplay({ bufferSize: 1, refCount: false }),
      );
    }

    return this.pendingRefresh;
  }

  switchTo(user: DevUser): Promise<void> {
    return firstValueFrom(this.signInAs(user));
  }

  useAccessCode(code: string): Promise<void> {
    writeStored(ACCESS_CODE_KEY, code);
    this.accessCode.set(code);

    return this.switchTo(this.currentUser());
  }

  async initialize(): Promise<void> {
    const storedId = readStored(USER_KEY);
    const user = DEV_USERS.find((candidate) => candidate.id === storedId) ?? DEV_USERS[0];

    try {
      await this.switchTo(user);
    } catch {
      this.currentUser.set(user);
    }
  }
}

function isAccessRefused(error: unknown): boolean {
  return error instanceof HttpErrorResponse && error.status === HttpStatusCode.Forbidden;
}

function readStored(key: string): string {
  try {
    return sessionStorage.getItem(key) ?? '';
  } catch {
    return '';
  }
}

function writeStored(key: string, value: string): void {
  try {
    sessionStorage.setItem(key, value);
  } catch {
    return;
  }
}
