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
      <div class="auth-card">
        <h1 class="auth-title">Reset Password</h1>
        <p class="auth-subtitle">Enter the OTP and your new password</p>

        @if (successMessage()) {
          <div class="alert alert-success">{{ successMessage() }}</div>
        }
        @if (errorMessage()) {
          <div class="alert alert-error">{{ errorMessage() }}</div>
        }

        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label for="otp">One-Time Password</label>
            <input id="otp" type="text" formControlName="otp" placeholder="123456" />
          </div>
          
          <div class="form-group">
            <label for="newPassword">New Password</label>
            <input id="newPassword" type="password" formControlName="newPassword" />
          </div>

          <button type="submit" class="btn btn-primary btn-block" [disabled]="isLoading() || form.invalid">
            @if (isLoading()) {
              <span class="spinner"></span>
            } @else {
              Reset Password
            }
          </button>
        </form>
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
    otp: ['', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
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
    if (this.form.invalid) return;

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
