import { HttpErrorResponse, HttpInterceptorFn, HttpStatusCode } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotifyService } from '../notify/notify.service';
import { describeError } from './error-body';

export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const notify = inject(NotifyService);

  return next(request).pipe(
    catchError((error: unknown) => {
      const isFormError = error instanceof HttpErrorResponse && error.status === HttpStatusCode.UnprocessableEntity;

      if (error instanceof HttpErrorResponse && !isFormError) {
        notify.error(describeError(error));
      }

      return throwError(() => error);
    }),
  );
};
