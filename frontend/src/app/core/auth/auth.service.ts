import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, catchError, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthResponse,
  AuthUser,
  AppRole,
  LoginRequest,
  RegisterRequest,
  RefreshTokenRequest,
  VerifyEmailRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ExternalLoginRequest,
} from '../models/auth.models';

const SESSION_STORAGE_KEY = 'job_portal.auth.session.v1';
const LEGACY_STORAGE_KEY = 'auth_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly apiUrl = `${environment.apiBaseUrl}/Auth`;
  private readonly userSignal = signal<AuthUser | null>(this.loadFromStorage());

  // ── Public reactive state ───────────────────────────────────────────────────
  readonly currentUser = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.userSignal());
  readonly userRoles = computed(() => this.userSignal()?.roles ?? []);
  readonly userEmail = computed(() => this.userSignal()?.email ?? '');

  readonly isCandidate = computed(() => this.hasRole('Candidate'));
  readonly isRecruiter = computed(() => this.hasRole('Recruiter'));
  readonly isAdmin = computed(() => this.hasRole('Admin'));

  // ── Auth operations ─────────────────────────────────────────────────────────

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap((response) => this.setSession(response)),
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request);
  }

  verifyEmail(request: VerifyEmailRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/verify-email`, request);
  }

  resendVerification(request: ForgotPasswordRequest): Observable<{ message: string; otp: string }> {
    return this.http.post<{ message: string; otp: string }>(`${this.apiUrl}/resend-verification`, request);
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<{ message: string; otp: string }> {
    return this.http.post<{ message: string; otp: string }>(`${this.apiUrl}/forgot-password`, request);
  }

  resetPassword(request: ResetPasswordRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/reset-password`, request);
  }

  externalLogin(request: ExternalLoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/external-login`, request).pipe(
      tap((response) => this.setSession(response)),
    );
  }

  refreshToken(): Observable<AuthResponse> {
    const user = this.userSignal();
    if (!user) {
      return throwError(() => new Error('No authenticated user'));
    }

    const request: RefreshTokenRequest = {
      accessToken: user.accessToken,
      refreshToken: user.refreshToken,
    };

    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh-token`, request).pipe(
      tap((response) => this.setSession(response)),
      catchError((err) => {
        this.clearSession();
        return throwError(() => err);
      }),
    );
  }

  logout(): void {
    this.clearSession();
    void this.router.navigate(['/login']);
  }

  // ── Token access ────────────────────────────────────────────────────────────

  getAccessToken(): string | null {
    return this.userSignal()?.accessToken ?? null;
  }

  canRefresh(): boolean {
    return Boolean(this.userSignal()?.refreshToken);
  }

  isTokenExpired(): boolean {
    const user = this.userSignal();
    if (!user?.accessTokenExpiration) return true;
    return new Date(user.accessTokenExpiration) <= new Date();
  }

  // ── Role checks ─────────────────────────────────────────────────────────────

  hasRole(role: AppRole): boolean {
    return this.userSignal()?.roles.includes(role) ?? false;
  }

  hasAnyRole(roles: readonly AppRole[]): boolean {
    const userRoles = this.userSignal()?.roles ?? [];
    return roles.some((role) => userRoles.includes(role));
  }

  // ── Navigation helper ───────────────────────────────────────────────────────

  getDefaultRoute(): string {
    if (this.isAdmin()) return '/admin/dashboard';
    if (this.isRecruiter()) return '/recruiter/dashboard';
    if (this.isCandidate()) return '/candidate/dashboard';
    return '/jobs';
  }

  // ── Private ─────────────────────────────────────────────────────────────────

  private setSession(response: AuthResponse): void {
    const user: AuthUser = {
      email: response.email,
      roles: response.roles,
      accessToken: response.accessToken!,
      refreshToken: response.refreshToken!,
      accessTokenExpiration: response.accessTokenExpiration!,
    };
    this.userSignal.set(user);
    sessionStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(user));
  }

  private clearSession(): void {
    this.userSignal.set(null);
    sessionStorage.removeItem(SESSION_STORAGE_KEY);
    localStorage.removeItem(LEGACY_STORAGE_KEY);
  }

  private loadFromStorage(): AuthUser | null {
    try {
      localStorage.removeItem(LEGACY_STORAGE_KEY);

      const stored = sessionStorage.getItem(SESSION_STORAGE_KEY);
      if (!stored) return null;
      const user: unknown = JSON.parse(stored);
      if (isAuthUser(user)) {
        return user;
      }
      sessionStorage.removeItem(SESSION_STORAGE_KEY);
      return null;
    } catch {
      sessionStorage.removeItem(SESSION_STORAGE_KEY);
      return null;
    }
  }
}

function isAuthUser(value: unknown): value is AuthUser {
  if (typeof value !== 'object' || value === null) {
    return false;
  }

  const candidate = value as Record<string, unknown>;
  return (
    isNonEmptyString(candidate['email']) &&
    isNonEmptyString(candidate['accessToken']) &&
    isNonEmptyString(candidate['refreshToken']) &&
    isNonEmptyString(candidate['accessTokenExpiration']) &&
    Array.isArray(candidate['roles']) &&
    candidate['roles'].every(isAppRole)
  );
}

function isNonEmptyString(value: unknown): value is string {
  return typeof value === 'string' && value.trim().length > 0;
}

function isAppRole(value: unknown): value is AppRole {
  return value === 'Candidate' || value === 'Recruiter' || value === 'Admin';
}
