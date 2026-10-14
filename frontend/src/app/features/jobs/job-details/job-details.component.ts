import { ChangeDetectionStrategy, Component, Input, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { JobResponse } from '../../../core/models/job.models';
import { JobsService } from '../../../core/services/jobs.service';
import { ApplicationsService } from '../../../core/services/applications.service';
import { AuthService } from '../../../core/auth/auth.service';
import { SavedJobsService } from '../../../core/services/saved-jobs.service';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, finalize } from 'rxjs';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { CandidateCvsService } from '../../../core/services/candidate-cvs.service';
import { CandidateCv } from '../../../core/models/candidate-cv.models';
import { DatePipe } from '@angular/common';
import {
  EMPLOYMENT_TYPE_LABELS,
  EXPERIENCE_LEVEL_LABELS,
  JOB_STATUS_LABELS,
  SALARY_PERIOD_LABELS,
  WORKPLACE_TYPE_LABELS,
} from '../../../core/models/job.models';

@Component({
  selector: 'app-job-details',
  standalone: true,
  imports: [RouterLink, DatePipe],
  templateUrl: './job-details.component.html',
  styleUrl: './job-details.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobDetailsComponent implements OnInit {
  // Provided by the router due to withComponentInputBinding()
  @Input() id!: string;

  private readonly jobsService = inject(JobsService);
  private readonly applicationsService = inject(ApplicationsService);
  private readonly savedJobsService = inject(SavedJobsService);
  protected readonly authService = inject(AuthService);
  private readonly apiError = inject(ApiErrorService);
  private readonly candidateCvsService = inject(CandidateCvsService);

  protected readonly employmentLabels = EMPLOYMENT_TYPE_LABELS;
  protected readonly workplaceLabels = WORKPLACE_TYPE_LABELS;
  protected readonly experienceLabels = EXPERIENCE_LEVEL_LABELS;
  protected readonly salaryPeriodLabels = SALARY_PERIOD_LABELS;
  protected readonly statusLabels = JOB_STATUS_LABELS;

  protected readonly job = signal<JobResponse | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly isError = signal(false);
  protected readonly errorMessage = signal('');
  
  protected readonly isApplying = signal(false);
  protected readonly applySuccess = signal(false);
  protected readonly applyError = signal<string | null>(null);

  protected readonly isSaved = signal(false);
  protected readonly isSaving = signal(false);
  protected readonly saveError = signal<string | null>(null);
  protected readonly cvs = signal<CandidateCv[]>([]);
  protected readonly selectedCvId = signal<number | null>(null);
  protected readonly isLoadingCvs = signal(false);

  ngOnInit(): void {
    this.loadJobDetails();
  }

  loadJobDetails(): void {
    const jobId = this.getJobId();
    if (jobId === null) {
      this.isLoading.set(false);
      this.isError.set(true);
      this.errorMessage.set('The requested job identifier is invalid.');
      return;
    }

    this.isLoading.set(true);
    this.isError.set(false);
    this.errorMessage.set('');

    this.jobsService
      .getById(jobId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (job) => {
          this.job.set(job);
          this.isSaved.set(job.isSaved);
          this.applySuccess.set(job.hasApplied);
          if (this.authService.isCandidate() && !job.hasApplied) {
            this.loadCvs();
          }
        },
        error: (error: unknown) => {
          this.isError.set(true);
          this.errorMessage.set(
            this.apiError.getMessage(error, 'The job could not be loaded. Please try again.'),
          );
        },
      });
  }

  loadCvs(): void {
    this.isLoadingCvs.set(true);
    this.candidateCvsService.getMine().pipe(finalize(() => this.isLoadingCvs.set(false))).subscribe({
      next: (cvs) => {
        this.cvs.set(cvs);
        this.selectedCvId.set(cvs.find((cv) => cv.isDefault)?.id ?? cvs[0]?.id ?? null);
      },
      error: (error: unknown) => this.applyError.set(this.apiError.getMessage(error, 'Your CVs could not be loaded.')),
    });
  }

  selectCv(event: Event): void {
    const value = Number((event.target as HTMLSelectElement).value);
    this.selectedCvId.set(Number.isInteger(value) && value > 0 ? value : null);
  }

  checkIfSaved(jobId: number): void {
    if (this.authService.isCandidate()) {
      this.savedJobsService.isSaved(jobId).subscribe({
        next: (res) => this.isSaved.set(res.isSaved),
        error: () => { /* Ignore silently */ }
      });
    }
  }

  onApply(): void {
    const jobId = this.getJobId();
    const candidateCvId = this.selectedCvId();
    if (jobId === null || candidateCvId === null || !this.authService.isCandidate() || !this.job()?.isActive) {
      return;
    }

    this.isApplying.set(true);
    this.applyError.set(null);

    this.applicationsService
      .apply({ jobId, candidateCvId })
      .pipe(finalize(() => this.isApplying.set(false)))
      .subscribe({
        next: (application) => {
          this.applySuccess.set(true);
          this.job.update((job) => job ? { ...job, hasApplied: true, applicationId: application.id } : job);
        },
        error: (error: HttpErrorResponse) => {
          this.applyError.set(
            this.apiError.getMessage(error, 'The application could not be submitted. Please try again.'),
          );
        },
      });
  }

  toggleSave(): void {
    const jobId = this.getJobId();
    if (jobId === null || !this.authService.isCandidate() || this.isSaving()) {
      return;
    }

    this.isSaving.set(true);
    this.saveError.set(null);
    const request$: Observable<unknown> = this.isSaved()
      ? this.savedJobsService.remove(jobId)
      : this.savedJobsService.save(jobId);

    request$
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => this.isSaved.update((saved) => !saved),
        error: (error: unknown) => {
          this.saveError.set(
            this.apiError.getMessage(error, 'The saved-job status could not be updated.'),
          );
        },
      });
  }

  private getJobId(): number | null {
    const jobId = Number(this.id);
    return Number.isInteger(jobId) && jobId > 0 ? jobId : null;
  }
}
