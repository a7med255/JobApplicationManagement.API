import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';
import { AppRole } from '../models/auth.models';

/**
 * Functional route guard that redirects unauthenticated users to /login.
 */
export const authGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

/**
 * Creates a functional route guard that requires a specific role.
 * Redirects to /forbidden if the user lacks the role.
 */
export function roleGuard(...requiredRoles: AppRole[]): CanActivateFn {
  return (_route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
      return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
    }

    if (authService.hasAnyRole(requiredRoles)) {
      return true;
    }

    return router.createUrlTree(['/forbidden']);
  };
}

/**
 * Functional guard that redirects already-authenticated users away from login/register
 * to their role-appropriate dashboard.
 */
export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return router.createUrlTree([authService.getDefaultRoute()]);
  }

  return true;
};
