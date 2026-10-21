import { HttpErrorResponse, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { isApiUrl } from '../api/api-url';
import { AuthService, DEV_TOKEN_PATH } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const isTokenCall = request.url.endsWith(DEV_TOKEN_PATH);

  if (!isApiUrl(request.url) || isTokenCall) {
    return next(request);
  }

  return next(withToken(request, auth.token())).pipe(
    catchError((error: unknown) =>
      isUnauthorized(error) ? retryWithNewToken(request, next, auth) : throwError(() => error),
    ),
  );
};

function retryWithNewToken(request: HttpRequest<unknown>, next: HttpHandlerFn, auth: AuthService) {
  return auth.refresh().pipe(switchMap(() => next(withToken(request, auth.token()))));
}

function withToken(request: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return token.length === 0 ? request : request.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

function isUnauthorized(error: unknown): boolean {
  return error instanceof HttpErrorResponse && error.status === 401;
}
