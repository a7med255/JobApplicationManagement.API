import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ChangeDetectionStrategy } from '@angular/core';
import { computed } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { JobApplicationResponse, JobApplicationStatus } from '../../../core/models/application.models';
import { ApplicationsService } from '../../../core/services/applications.service';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { ApplicationStatusBadgeComponent } from '../../../shared/components/application-status-badge/application-status-badge.component';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-recruiter-application-review',
  standalone: true,
  imports: [DatePipe, RouterLink, ReactiveFormsModule, ApplicationStatusBadgeComponent],
  templateUrl: './recruiter-application-review.component.html',
  styleUrl: './recruiter-application-review.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecruiterApplicationReviewComponent implements OnInit {
  @Input() id!: string;

  private readonly applicationsService = inject(ApplicationsService);
  private readonly apiError = inject(ApiErrorService);
  private readonly authService = inject(AuthService);

  protected readonly application = signal<JobApplicationResponse | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly isUpdating = signal(false);
  protected readonly updateError = signal<string | null>(null);
  protected readonly updateSuccess = signal(false);
  protected readonly isDownloadingCv = signal(false);
  protected readonly applicationStatus = JobApplicationStatus;
  protected readonly backRoute = computed(() =>
    this.authService.isAdmin() ? '/admin/applications' : '/recruiter/applications',
  );

  // Status mapping for the dropdown
  protected readonly statusOptions: ReadonlyArray<{ value: JobApplicationStatus; label: string }> = [
    { value: JobApplicationStatus.Applied, label: 'Applied' },
    { value: JobApplicationStatus.UnderReview, label: 'Under Review' },
    { value: JobApplicationStatus.InterView, label: 'Interview' },
    { value: JobApplicationStatus.Accepted, label: 'Accepted' },
    { value: JobApplicationStatus.Rejected, label: 'Rejected' },
  ];
  
  protected readonly statusControl = new FormControl(JobApplicationStatus.Applied, { nonNullable: true });

  ngOnInit(): void {
    if (!this.getApplicationId()) {
      this.isLoading.set(false);
      this.errorMessage.set('The application identifier is invalid.');
      return;
    }

    this.loadApplication();
  }

  loadApplication(): void {
    const applicationId = this.getApplicationId();
    if (!applicationId) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.applicationsService
      .getById(applicationId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (application) => {
          this.application.set(application);
          this.statusControl.setValue(application.jobApplicationStatus);
          if (application.jobApplicationStatus === JobApplicationStatus.Cancelled) {
            this.statusControl.disable();
          } else {
            this.statusControl.enable();
          }
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.apiError.getMessage(error, 'Application not found.'));
        },
      });
  }

  updateStatus(): void {
    const applicationId = this.getApplicationId();
    if (!this.application() || !applicationId) {
      return;
    }
    
    this.isUpdating.set(true);
    this.updateError.set(null);
    this.updateSuccess.set(false);

    this.applicationsService
      .updateStatus(applicationId, { jobApplicationStatus: this.statusControl.getRawValue() })
      .pipe(finalize(() => this.isUpdating.set(false)))
      .subscribe({
        next: (application) => {
          this.application.set(application);
          this.statusControl.setValue(application.jobApplicationStatus);
          this.updateSuccess.set(true);
        },
        error: (error: unknown) => {
          this.updateError.set(this.apiError.getMessage(error, 'Failed to update application status.'));
        },
      });
  }

  downloadSubmittedCv(): void {
    const application = this.application();
    if (!application?.submittedCvId) return;
    this.isDownloadingCv.set(true);
    this.applicationsService.downloadSubmittedCv(application.id)
      .pipe(finalize(() => this.isDownloadingCv.set(false)))
      .subscribe({
        next: (blob) => {
          const url = URL.createObjectURL(blob);
          window.open(url, '_blank', 'noopener,noreferrer');
          window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
        },
        error: (error: unknown) => this.updateError.set(this.apiError.getMessage(error, 'The submitted CV could not be downloaded.')),
      });
  }

  private getApplicationId(): number | null {
    const parsedId = Number(this.id);
    return Number.isInteger(parsedId) && parsedId > 0 ? parsedId : null;
  }
}
