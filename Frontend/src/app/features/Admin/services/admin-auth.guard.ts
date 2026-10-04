import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AdminAuthSession } from './admin-auth-session';

export const adminAuthGuard: CanActivateFn = () => {
  const session = inject(AdminAuthSession);
  const router = inject(Router);
  return session.isAdmin ? true : router.createUrlTree(['/']);
};
