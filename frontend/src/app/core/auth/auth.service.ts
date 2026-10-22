import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { finalize, firstValueFrom, map, Observable, of, shareReplay, tap } from 'rxjs';
import { DevToken, DevTokenRequest } from '../api/api.model';
import { apiUrl } from '../api/api-url';
import { DEV_USERS, DevUser } from './dev-users';

export const DEV_TOKEN_PATH = 'dev/token';

const USER_KEY = 'claims.userId';
const NO_REFRESH: Observable<void> = of(undefined);

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly currentUser = signal<DevUser>(DEV_USERS[0]);
  private readonly currentToken = signal('');
  private signInVersion = 0;
  private pendingRefresh = NO_REFRESH;

  readonly user = this.currentUser.asReadonly();
  readonly token = this.currentToken.asReadonly();
  readonly role = computed(() => this.currentUser().role);
  readonly isSignedIn = computed(() => this.currentToken().length > 0);

  signInAs(user: DevUser): Observable<void> {
    const request: DevTokenRequest = { userId: user.id };
    const version = ++this.signInVersion;

    return this.http.post<DevToken>(apiUrl(DEV_TOKEN_PATH), request).pipe(
      tap((devToken) => {
        if (version === this.signInVersion) {
          this.currentUser.set({ id: devToken.userId, name: devToken.name, role: devToken.role });
          this.currentToken.set(devToken.token);
          writeStored(USER_KEY, devToken.userId);
        }
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
