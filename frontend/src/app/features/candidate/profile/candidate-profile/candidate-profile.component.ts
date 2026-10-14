import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CandidatesService } from '../../../../core/services/candidates.service';
import { ApiErrorService } from '../../../../core/services/api-error.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-candidate-profile',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './candidate-profile.component.html',
  styleUrl: './candidate-profile.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateProfileComponent implements OnInit {
  private readonly candidatesService = inject(CandidatesService);
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
    cvUrl: ['', [Validators.maxLength(500)]],
  });

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading.set(true);
    this.isError.set(false);
    this.errorMessage.set(null);

    this.candidatesService
      .getMine()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (profile) => {
          this.profileId = profile.id;
          this.form.patchValue({
            name: profile.name,
            cvUrl: profile.cvUrl || '',
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

    this.candidatesService
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

  isFieldInvalid(field: 'name' | 'cvUrl'): boolean {
    const control = this.form.get(field);
    return !!(control && control.invalid && control.touched);
  }
}
