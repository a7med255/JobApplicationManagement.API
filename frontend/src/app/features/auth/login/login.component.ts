import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';
import { ApiErrorService } from '../../../core/services/api-error.service';

type LoginField = 'email' | 'password';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly apiError = inject(ApiErrorService);

  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService
      .login(this.form.getRawValue())
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          void this.router.navigateByUrl(this.getSafeReturnUrl());
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401 || error.status === 400) {
            this.errorMessage.set('Invalid email or password.');
          } else {
            this.errorMessage.set(
              this.apiError.getMessage(error, 'An error occurred during login. Please try again later.'),
            );
          }
        },
      });
  }

  loginWithGoogle(): void {
    // For demo/testing, simulate a prompt for the user's Google email
    const simulatedEmail = prompt("Enter your Google email to simulate external login:", "googleuser@gmail.com");
    if (!simulatedEmail) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService
      .externalLogin({ provider: 'Google', idToken: simulatedEmail })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          void this.router.navigate([this.authService.getDefaultRoute()]);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(
            this.apiError.getMessage(error, 'External login failed. Please try again.'),
          );
        },
      });
  }

  isFieldInvalid(field: LoginField): boolean {
    const control = this.form.get(field);
    return !!(control && control.invalid && control.touched);
  }

  private getSafeReturnUrl(): string {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    return returnUrl?.startsWith('/') && !returnUrl.startsWith('//')
      ? returnUrl
      : this.authService.getDefaultRoute();
  }
}
