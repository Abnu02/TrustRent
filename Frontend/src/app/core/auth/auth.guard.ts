import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { UserRole } from './auth.models';

export const authGuard: CanActivateFn = () => {

  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};

export const roleGuard = (
  allowedRoles: UserRole[]
): CanActivateFn => {

  return () => {

    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
      return router.createUrlTree(['/login']);
    }

    const user = authService.currentUser();

    if (!user) {
      return router.createUrlTree(['/login']);
    }

    if (allowedRoles.includes(user.role)) {
      return true;
    }

    return router.createUrlTree(['/']);
  };
};