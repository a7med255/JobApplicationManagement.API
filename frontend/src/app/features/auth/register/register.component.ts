import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';
import { RegistrationRole } from '../../../core/models/auth.models';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { APP_CONFIG } from '../../../core/config/app.constants';

type RegisterField = 'fullName' | 'email' | 'password' | 'role';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: '../login/login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly apiError = inject(ApiErrorService);
  protected readonly appConfig = APP_CONFIG;

  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email]],
    password: [
      '',
      [
        Validators.required,
        Validators.minLength(8),
        Validators.maxLength(100),
        Validators.pattern(/[a-z]/),
        Validators.pattern(/[A-Z]/),
        Validators.pattern(/\d/),
      ],
    ],
    role: ['Candidate' as RegistrationRole, [Validators.required]],
  });

  ngOnInit(): void {
    const roleParam = this.route.snapshot.queryParamMap.get('role');
    if (roleParam === 'Recruiter') {
      this.form.controls.role.setValue('Recruiter');
    } else if (roleParam === 'Candidate') {
      this.form.controls.role.setValue('Candidate');
    }
  }

  setRole(selectedRole: RegistrationRole): void {
    this.form.controls.role.setValue(selectedRole);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService
      .register(this.form.getRawValue())
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          void this.router.navigate(['/verify-email'], { queryParams: { email: this.form.getRawValue().email } });
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(
            this.apiError.getMessage(
              error,
              'An error occurred during registration. Please try again later.',
            ),
          );
        },
      });
  }

  isFieldInvalid(field: RegisterField): boolean {
    const control = this.form.get(field);
    return !!(control && control.invalid && control.touched);
  }

  passwordErrorMessage(): string {
    const password = this.form.controls.password;
    if (password.hasError('required')) return 'Password is required.';
    if (password.hasError('minlength')) return 'Password must be at least 8 characters.';
    if (password.hasError('maxlength')) return 'Password must not exceed 100 characters.';
    return 'Must include at least one uppercase letter, one lowercase letter, and one number.';
  }
}
