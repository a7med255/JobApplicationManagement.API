import { AppRole } from './auth.models';

export interface AdminUser {
  userId: string;
  email: string;
  fullName: string;
  emailConfirmed: boolean;
  isLocked: boolean;
  roles: AppRole[];
}

export interface OutboxFailure {
  id: string;
  type: string;
  attemptCount: number;
  lastError: string | null;
  occurredAt: string;
  nextAttemptAt: string | null;
}
