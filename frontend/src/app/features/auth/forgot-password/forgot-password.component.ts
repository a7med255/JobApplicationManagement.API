import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';
import { ApiErrorService } from '../../../core/services/api-error.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <h1 class="auth-title">Forgot Password</h1>
        <p class="auth-subtitle">Enter your email to receive a reset OTP</p>

        @if (successMessage()) {
          <div class="alert alert-success">{{ successMessage() }}</div>
        }
        @if (errorMessage()) {
          <div class="alert alert-error">{{ errorMessage() }}</div>
        }

        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label for="email">Email Address</label>
            <input
              id="email"
              type="email"
              formControlName="email"
              placeholder="john@example.com"
              [attr.aria-invalid]="isFieldInvalid('email')"
            />
          </div>

          <button type="submit" class="btn btn-primary btn-block" [disabled]="isLoading() || form.invalid">
            @if (isLoading()) {
              <span class="spinner"></span>
            } @else {
              Send Reset OTP
            }
          </button>
        </form>

        <div class="auth-links">
          <p>Remember your password? <a routerLink="/login">Log in</a></p>
        </div>
      </div>
    </div>
  `,
  styleUrl: '../login/login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly apiError = inject(ApiErrorService);

  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const email = this.form.controls.email.value;

    this.authService
      .forgotPassword({ email })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res) => {
          this.successMessage.set('OTP sent! Redirecting to reset page... (For testing: ' + res.otp + ')');
          setTimeout(() => void this.router.navigate(['/reset-password'], { queryParams: { email } }), 3000);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(this.apiError.getMessage(error, 'Request failed.'));
        },
      });
  }

  isFieldInvalid(field: 'email'): boolean {
    const control = this.form.get(field);
    return !!(control && control.invalid && control.touched);
  }
}
