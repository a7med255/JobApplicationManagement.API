import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, finalize, shareReplay, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { AuthResponse } from '../models/auth.models';

let refreshRequest$: Observable<AuthResponse> | null = null;

/**
 * Functional HTTP interceptor that:
 * 1. Attaches the JWT access token to outgoing API requests
 * 2. Handles 401 by attempting a token refresh and retrying the original request
 * 3. Redirects to login on auth failure
 */
export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Skip token attachment for auth endpoints (login, register, refresh-token)
  if (isAuthEndpoint(req.url)) {
    return next(req);
  }

  const token = authService.getAccessToken();
  const authedReq = token ? addAuthHeader(req, token) : req;

  return next(authedReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && authService.canRefresh()) {
        return getRefreshRequest(authService).pipe(
          switchMap((response) => {
            const retryReq = addAuthHeader(req, response.accessToken!);
            return next(retryReq);
          }),
          catchError((refreshError) => {
            authService.logout();
            return throwError(() => refreshError);
          }),
        );
      }

      if (error.status === 403) {
        void router.navigate(['/forbidden']);
      }

      return throwError(() => error);
    }),
  );
};

function getRefreshRequest(authService: AuthService): Observable<AuthResponse> {
  if (!refreshRequest$) {
    refreshRequest$ = authService.refreshToken().pipe(
      finalize(() => {
        refreshRequest$ = null;
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
  }

  return refreshRequest$;
}

function addAuthHeader(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });
}

function isAuthEndpoint(url: string): boolean {
  const normalizedUrl = url.toLowerCase();
  return (
    normalizedUrl.includes('/auth/login') ||
    normalizedUrl.includes('/auth/register') ||
    normalizedUrl.includes('/auth/refresh-token') ||
    normalizedUrl.includes('/auth/verify-email') ||
    normalizedUrl.includes('/auth/forgot-password') ||
    normalizedUrl.includes('/auth/reset-password') ||
    normalizedUrl.includes('/auth/resend-verification') ||
    normalizedUrl.includes('/auth/external-login')
  );
}
