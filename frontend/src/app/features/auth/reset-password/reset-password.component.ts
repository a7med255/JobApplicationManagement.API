import { ChangeDetectionStrategy, Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';
import { ApiErrorService } from '../../../core/services/api-error.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <div class="card auth-card">
        <div class="auth-header">
          <div class="auth-logo" aria-hidden="true">6</div>
          <h1 class="auth-title">Choose a new password</h1>
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
            <input id="otp" type="text" inputmode="numeric" autocomplete="one-time-code" maxlength="6" formControlName="otp" class="form-control" placeholder="123456" />
            @if (form.controls.otp.invalid && form.controls.otp.touched) {
              <div class="form-error">Enter the six-digit code.</div>
            }
          </div>
          
          <div class="form-group">
            <label for="newPassword" class="form-label">New password</label>
            <input id="newPassword" type="password" autocomplete="new-password" formControlName="newPassword" class="form-control" aria-describedby="new-password-help" />
            <span id="new-password-help" class="form-helper">Use 8+ characters with uppercase, lowercase, and a number.</span>
            @if (form.controls.newPassword.invalid && form.controls.newPassword.touched) {
              <div class="form-error">Enter a valid password that meets the requirements.</div>
            }
          </div>

          <button type="submit" class="btn btn--primary btn--block btn--lg auth-submit" [disabled]="isLoading()" [attr.aria-busy]="isLoading()">
            @if (isLoading()) {
              <span class="spinner"></span> Updating password...
            } @else {
              Update password
            }
          </button>
        </form>

        <div class="auth-footer">
          <p class="auth-footer-text"><a routerLink="/login" class="auth-link">Back to sign in</a></p>
        </div>
      </div>
    </div>
  `,
  styleUrl: '../login/login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResetPasswordComponent implements OnInit {
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
    newPassword: ['', [
      Validators.required,
      Validators.minLength(8),
      Validators.maxLength(100),
      Validators.pattern(/[a-z]/),
      Validators.pattern(/[A-Z]/),
      Validators.pattern(/\d/),
    ]],
  });

  ngOnInit() {
    const emailParam = this.route.snapshot.queryParamMap.get('email');
    if (emailParam) {
      this.email.set(emailParam);
    } else {
      void this.router.navigate(['/forgot-password']);
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
      .resetPassword({ email: this.email(), otp: this.form.controls.otp.value, newPassword: this.form.controls.newPassword.value })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          this.successMessage.set('Password reset successfully! Redirecting...');
          setTimeout(() => void this.router.navigate(['/login']), 2000);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(this.apiError.getMessage(error, 'Reset failed.'));
        },
      });
  }
}
