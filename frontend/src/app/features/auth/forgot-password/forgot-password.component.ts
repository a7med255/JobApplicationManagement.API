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
      <div class="card auth-card">
        <div class="auth-header">
          <div class="auth-logo" aria-hidden="true">?</div>
          <h1 class="auth-title">Reset your password</h1>
          <p class="auth-subtitle">Enter your account email and we’ll send a one-time code.</p>
        </div>

        @if (successMessage()) {
          <div class="alert alert--success" role="status">{{ successMessage() }}</div>
        }
        @if (errorMessage()) {
          <div class="alert alert--danger" role="alert">{{ errorMessage() }}</div>
        }

        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label for="email" class="form-label">Email address</label>
            <input
              id="email"
              type="email"
              autocomplete="email"
              formControlName="email"
              class="form-control"
              placeholder="name@example.com"
              [attr.aria-invalid]="isFieldInvalid('email')"
            />
            @if (isFieldInvalid('email')) {
              <div class="form-error">Enter a valid email address.</div>
            }
          </div>

          <button type="submit" class="btn btn--primary btn--block btn--lg auth-submit" [disabled]="isLoading()" [attr.aria-busy]="isLoading()">
            @if (isLoading()) {
              <span class="spinner"></span> Sending code...
            } @else {
              Send reset code
            }
          </button>
        </form>

        <div class="auth-footer">
          <p class="auth-footer-text">Remember your password? <a routerLink="/login" class="auth-link">Sign in</a></p>
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
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const email = this.form.controls.email.value;

    this.authService
      .forgotPassword({ email })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res) => {
          this.successMessage.set(res?.message || 'Password reset instructions sent! Redirecting to reset page...');
          setTimeout(() => void this.router.navigate(['/reset-password'], { queryParams: { email } }), 2000);
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
