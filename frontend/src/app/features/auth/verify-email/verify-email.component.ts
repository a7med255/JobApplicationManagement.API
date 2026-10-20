import { ChangeDetectionStrategy, Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { APP_CONFIG } from '../../../core/config/app.constants';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <div class="card auth-card">
        <div class="auth-header">
          <div class="auth-logo" aria-hidden="true">6</div>
          <h1 class="auth-title">Verify your email</h1>
          <p class="auth-subtitle">Enter the six-digit code sent to {{ email() }}.</p>
        </div>

        @if (successMessage()) {
          <div class="alert alert--success" role="status">{{ successMessage() }}</div>
        }
        @if (errorMessage()) {
          <div class="alert alert--danger" role="alert">{{ errorMessage() }}</div>
        }

        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label for="otp" class="form-label">One-time code</label>
            <input
              id="otp"
              type="text"
              inputmode="numeric"
              autocomplete="one-time-code"
              maxlength="6"
              formControlName="otp"
              class="form-control"
              placeholder="123456"
              [attr.aria-invalid]="isFieldInvalid('otp')"
            />
            @if (isFieldInvalid('otp')) {
              <div class="form-error">Enter the six-digit code.</div>
            }
          </div>

          <button type="submit" class="btn btn--primary btn--block btn--lg auth-submit" [disabled]="isLoading()" [attr.aria-busy]="isLoading()">
            @if (isLoading()) {
              <span class="spinner"></span> Verifying...
            } @else {
              Verify account
            }
          </button>
        </form>

        <div class="auth-footer">
          <p class="auth-footer-text">Didn't receive it? <button type="button" class="btn-link" (click)="resend()" [disabled]="isLoading()">Resend code</button></p>
          <p class="auth-footer-text mt-2"><a routerLink="/login" class="auth-link">Back to sign in</a></p>
        </div>
      </div>
    </div>
  `,
  styleUrl: '../login/login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VerifyEmailComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly apiError = inject(ApiErrorService);

  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);
  protected readonly email = signal('');

  protected readonly form = this.fb.nonNullable.group({
    otp: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
  });

  ngOnInit() {
    const emailParam = this.route.snapshot.queryParamMap.get('email');
    if (emailParam) {
      this.email.set(emailParam);
    } else {
      void this.router.navigate(['/login']);
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.authService
      .verifyEmail({ email: this.email(), otp: this.form.controls.otp.value })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          this.successMessage.set('Email verified successfully! You can now login.');
          setTimeout(() => void this.router.navigate(['/login']), 2000);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(this.apiError.getMessage(error, 'Verification failed.'));
        },
      });
  }

  resend(): void {
    this.isLoading.set(true);
    this.authService
      .resendVerification({ email: this.email() })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res) => {
          this.successMessage.set(res?.message || 'Verification code resent successfully. Please check your email.');
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(this.apiError.getMessage(error, 'Failed to resend.'));
        },
      });
  }

  isFieldInvalid(field: 'otp'): boolean {
    const control = this.form.get(field);
    return !!(control && control.invalid && control.touched);
  }
}
