import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RecruitersService } from '../../../../core/services/recruiters.service';
import { ApiErrorService } from '../../../../core/services/api-error.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-recruiter-profile',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './recruiter-profile.component.html',
  styleUrl: './recruiter-profile.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecruiterProfileComponent implements OnInit {
  private readonly recruitersService = inject(RecruitersService);
  private readonly apiError = inject(ApiErrorService);
  private readonly fb = inject(FormBuilder);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly isError = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);

  private profileId: number | null = null;

  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
  });

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading.set(true);
    this.isError.set(false);
    this.errorMessage.set(null);

    this.recruitersService
      .getMine()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (profile) => {
          this.profileId = profile.id;
          this.form.patchValue({
            name: profile.name,
          });
        },
        error: (error: unknown) => {
          this.isError.set(true);
          this.errorMessage.set(
            this.apiError.getMessage(error, 'Your profile could not be loaded.'),
          );
        },
      });
  }

  onSubmit(): void {
    if (this.form.invalid || !this.profileId) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.recruitersService
      .update(this.profileId, this.form.getRawValue())
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.successMessage.set('Your profile has been updated successfully.');
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            this.apiError.getMessage(error, 'Your profile could not be updated.'),
          );
        },
      });
  }

  isFieldInvalid(field: 'name'): boolean {
    const control = this.form.get(field);
    return !!(control && control.invalid && control.touched);
  }
}
