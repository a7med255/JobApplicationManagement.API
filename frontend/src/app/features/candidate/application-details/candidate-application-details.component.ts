import { ChangeDetectionStrategy, Component, Input, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { JobApplicationResponse, JobApplicationStatus } from '../../../core/models/application.models';
import { ApplicationsService } from '../../../core/services/applications.service';
import { finalize } from 'rxjs';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { ApplicationStatusBadgeComponent } from '../../../shared/components/application-status-badge/application-status-badge.component';

@Component({
  selector: 'app-candidate-application-details',
  standalone: true,
  imports: [DatePipe, RouterLink, ApplicationStatusBadgeComponent],
  templateUrl: './candidate-application-details.component.html',
  styleUrl: './candidate-application-details.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateApplicationDetailsComponent implements OnInit {
  @Input() id!: string;

  private readonly applicationsService = inject(ApplicationsService);
  private readonly apiError = inject(ApiErrorService);

  protected readonly application = signal<JobApplicationResponse | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly isError = signal(false);
  protected readonly errorMessage = signal('');
  protected readonly isCancelling = signal(false);
  protected readonly showCancelConfirmation = signal(false);
  protected readonly cancelError = signal<string | null>(null);
  protected readonly cancelSuccess = signal(false);
  protected readonly isDownloadingCv = signal(false);

  // Expose enum to template
  protected readonly AppStatus = JobApplicationStatus;

  ngOnInit(): void {
    this.loadApplication();
  }

  loadApplication(): void {
    const appId = this.getApplicationId();
    if (appId === null) {
      this.isLoading.set(false);
      this.isError.set(true);
      this.errorMessage.set('The requested application identifier is invalid.');
      return;
    }

    this.isLoading.set(true);
    this.isError.set(false);
    this.errorMessage.set('');

    this.applicationsService
      .getById(appId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (app) => this.application.set(app),
        error: (error: unknown) => {
          this.isError.set(true);
          this.errorMessage.set(
            this.apiError.getMessage(error, 'The application could not be loaded.'),
          );
        },
      });
  }

  requestCancellation(): void {
    this.cancelError.set(null);
    this.showCancelConfirmation.set(true);
  }

  dismissCancellation(): void {
    this.showCancelConfirmation.set(false);
  }

  confirmCancellation(): void {
    const appId = this.getApplicationId();
    if (appId === null || this.isCancelling()) {
      return;
    }

    this.isCancelling.set(true);
    this.cancelError.set(null);

    this.applicationsService
      .cancel(appId)
      .pipe(finalize(() => this.isCancelling.set(false)))
      .subscribe({
        next: () => {
          this.showCancelConfirmation.set(false);
          this.cancelSuccess.set(true);
          this.loadApplication();
        },
        error: (error: unknown) => {
          this.cancelError.set(
            this.apiError.getMessage(error, 'The application could not be cancelled.'),
          );
        },
      });
  }

  downloadSubmittedCv(): void {
    const app = this.application();
    if (!app?.submittedCvId) return;
    this.isDownloadingCv.set(true);
    this.applicationsService.downloadSubmittedCv(app.id)
      .pipe(finalize(() => this.isDownloadingCv.set(false)))
      .subscribe({
        next: (blob) => {
          const url = URL.createObjectURL(blob);
          window.open(url, '_blank', 'noopener,noreferrer');
          window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
        },
        error: (error: unknown) => this.cancelError.set(this.apiError.getMessage(error, 'The submitted CV could not be downloaded.')),
      });
  }

  private getApplicationId(): number | null {
    const applicationId = Number(this.id);
    return Number.isInteger(applicationId) && applicationId > 0 ? applicationId : null;
  }
}
