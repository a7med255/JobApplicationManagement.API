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
      <div class="auth-card">
        <h1 class="auth-title">Verify Email</h1>
        <p class="auth-subtitle">Enter the 6-digit OTP sent to {{ email() }}</p>

        @if (successMessage()) {
          <div class="alert alert-success">{{ successMessage() }}</div>
        }
        @if (errorMessage()) {
          <div class="alert alert-error">{{ errorMessage() }}</div>
        }

        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label for="otp">One-Time Password</label>
            <input
              id="otp"
              type="text"
              formControlName="otp"
              placeholder="123456"
              [attr.aria-invalid]="isFieldInvalid('otp')"
            />
            @if (isFieldInvalid('otp')) {
              <span class="error-text">OTP is required</span>
            }
          </div>

          <button type="submit" class="btn btn-primary btn-block" [disabled]="isLoading() || form.invalid">
            @if (isLoading()) {
              <span class="spinner"></span>
            } @else {
              Verify Account
            }
          </button>
        </form>

        <div class="auth-links">
          <p>Didn't receive the email? <button class="btn-link" (click)="resend()">Resend OTP</button></p>
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
    otp: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]],
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
    if (this.form.invalid) return;

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
          this.successMessage.set('OTP resent successfully. For testing: ' + res.otp);
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
