import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const isPublicAuthRequest = /\/api\/v1\/auth\/(login|register)$/.test(request.url);
  const isLandlordRequest = request.url.startsWith('/api/v1/landlord/properties');
  const isLogoutRequest = request.url.endsWith('/api/v1/auth/logout');
  if (!isPublicAuthRequest && !isLandlordRequest && !isLogoutRequest) {
    return next(request);
  }

  const token = isPublicAuthRequest ? null : auth.accessToken;
  const authorizedRequest = token
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : request;

  return next(authorizedRequest).pipe(catchError((error: unknown) => {
    if ((isLandlordRequest || isLogoutRequest)
      && error instanceof HttpErrorResponse
      && error.status === 401) {
      auth.clearSession();
      void router.navigate(['/auth']);
    }
    return throwError(() => error);
  }));
};