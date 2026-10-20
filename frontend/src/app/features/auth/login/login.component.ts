import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
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
  protected readonly isEmailUnverified = computed(() => {
    const msg = this.errorMessage();
    return !!msg && (msg.toLowerCase().includes('verify') || msg.toLowerCase().includes('confirmed'));
  });

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
          if (error.status === 401) {
            this.errorMessage.set(
              this.apiError.getMessage(error, 'Invalid email or password.'),
            );
          } else if (error.status === 400) {
            this.errorMessage.set(
              this.apiError.getMessage(error, 'Please check your input and try again.'),
            );
          } else if (error.status === 403) {
            this.errorMessage.set(
              this.apiError.getMessage(error, 'Access denied.'),
            );
          } else if (error.status === 404) {
            this.errorMessage.set(
              this.apiError.getMessage(error, 'The requested resource was not found.'),
            );
          } else if (error.status === 409) {
            this.errorMessage.set(
              this.apiError.getMessage(error, 'A conflict occurred. Please try again.'),
            );
          } else {
            this.errorMessage.set(
              this.apiError.getMessage(error, 'An error occurred during login. Please try again later.'),
            );
          }
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
