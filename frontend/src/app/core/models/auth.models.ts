// ── Auth DTOs ─────────────────────────────────────────────────────────────────

export type AppRole = 'Candidate' | 'Recruiter' | 'Admin';
export type RegistrationRole = Exclude<AppRole, 'Admin'>;

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  role: RegistrationRole;
}

export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
}

export interface AuthResponse {
  accessToken?: string;
  refreshToken?: string;
  accessTokenExpiration?: string;
  email: string;
  roles: AppRole[];
}

export interface MessageResponse {
  message: string;
}

export interface VerifyEmailRequest {
  email: string;
  otp: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  otp: string;
  newPassword: string;
}

export interface ExternalLoginRequest {
  provider: string;
  idToken: string;
}

// ── Stored auth state ─────────────────────────────────────────────────────────

export interface AuthUser {
  email: string;
  roles: AppRole[];
  accessToken: string;
  refreshToken: string;
  accessTokenExpiration: string;
}
