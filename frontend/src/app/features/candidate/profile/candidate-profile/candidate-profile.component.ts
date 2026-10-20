import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CandidatesService } from '../../../../core/services/candidates.service';
import { ApiErrorService } from '../../../../core/services/api-error.service';
import { finalize } from 'rxjs';
import { APP_CONFIG } from '../../../../core/config/app.constants';

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
  protected readonly isUploadingImage = signal(false);
  protected readonly profileImageUrl = signal<string | null>(null);
  protected readonly profileCompleteness = signal(0);
  protected readonly maximumImageMegabytes = APP_CONFIG.maximumImageBytes / 1024 / 1024;

  private profileId: number | null = null;

  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    cvUrl: ['', [Validators.maxLength(500)]],
    professionalTitle: ['', Validators.maxLength(200)],
    summary: ['', Validators.maxLength(2000)],
    country: ['', Validators.maxLength(100)],
    city: ['', Validators.maxLength(100)],
    yearsOfExperience: this.fb.control<number | null>(null, [Validators.min(0), Validators.max(70)]),
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
            professionalTitle: profile.professionalTitle ?? '',
            summary: profile.summary ?? '',
            country: profile.country ?? '',
            city: profile.city ?? '',
            yearsOfExperience: profile.yearsOfExperience,
          });
          this.profileImageUrl.set(profile.profileImageUrl);
          this.profileCompleteness.set(profile.profileCompleteness);
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
      .update(this.profileId, this.toRequest())
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

  isFieldInvalid(field: string): boolean {
    const control = this.form.get(field);
    return !!(control && control.invalid && control.touched);
  }

  uploadImage(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type) || file.size > APP_CONFIG.maximumImageBytes || file.size === 0) {
      this.errorMessage.set(`Choose a non-empty JPEG, PNG, or WebP image no larger than ${this.maximumImageMegabytes} MB.`);
      return;
    }
    this.isUploadingImage.set(true);
    this.errorMessage.set(null);
    this.candidatesService.uploadProfileImage(file).pipe(finalize(() => this.isUploadingImage.set(false))).subscribe({
      next: (profile) => {
        this.profileImageUrl.set(profile.profileImageUrl);
        this.profileCompleteness.set(profile.profileCompleteness);
        this.successMessage.set('Profile image updated.');
      },
      error: (error: unknown) => this.errorMessage.set(this.apiError.getMessage(error, 'Profile image could not be uploaded.')),
    });
  }

  private toRequest() {
    const value = this.form.getRawValue();
    return {
      name: value.name,
      cvUrl: value.cvUrl,
      professionalTitle: value.professionalTitle.trim() || null,
      summary: value.summary.trim() || null,
      country: value.country.trim() || null,
      city: value.city.trim() || null,
      yearsOfExperience: value.yearsOfExperience,
    };
  }
}
