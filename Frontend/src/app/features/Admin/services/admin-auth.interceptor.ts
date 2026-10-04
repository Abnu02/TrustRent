import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../../../auth/auth.service';

export const adminAuthInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.startsWith('/api/v1/')) {
    return next(request);
  }

  if (/\/api\/v1\/auth\/(login|register)$/.test(request.url)) {
    return next(request);
  }

  const token = inject(AuthService).accessToken;
  if (!token) {
    return next(request);
  }

  return next(request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
