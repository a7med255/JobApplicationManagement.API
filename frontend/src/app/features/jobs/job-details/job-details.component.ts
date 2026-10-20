import { ChangeDetectionStrategy, Component, computed, HostListener, Input, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { JobResponse, JobSummary } from '../../../core/models/job.models';
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
import { JobCardComponent } from '../../../shared/components/job-card/job-card.component';
import { CompanyLogoComponent } from '../../../shared/components/company-logo/company-logo.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

const MAX_CV_BYTES = 5 * 1024 * 1024; // 5 MB

@Component({
  selector: 'app-job-details',
  standalone: true,
  imports: [RouterLink, DatePipe, JobCardComponent, CompanyLogoComponent, StatusBadgeComponent],
  templateUrl: './job-details.component.html',
  styleUrl: './job-details.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobDetailsComponent implements OnInit {
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
  
  // Apply Modal & Flow
  protected readonly applyModalOpen = signal(false);
  protected readonly isApplying = signal(false);
  protected readonly applySuccess = signal(false);
  protected readonly applyError = signal<string | null>(null);

  // CV Selection
  protected readonly cvs = signal<CandidateCv[]>([]);
  protected readonly selectedCvId = signal<number | null>(null);
  protected readonly isLoadingCvs = signal(false);
  protected readonly coverLetter = signal('');

  // Quick CV Upload inside Modal
  protected readonly showUploadForm = signal(false);
  protected readonly isUploadingCv = signal(false);
  protected readonly uploadError = signal<string | null>(null);
  protected readonly pendingCvName = signal('');
  protected readonly selectedFile = signal<File | null>(null);

  // Saved Jobs
  protected readonly isSaved = signal(false);
  protected readonly isSaving = signal(false);
  protected readonly saveError = signal<string | null>(null);

  // Similar Jobs
  protected readonly similarJobs = signal<JobSummary[]>([]);

  // Computed properties
  protected readonly salaryText = computed(() => {
    const j = this.job();
    if (!j) return '';
    if (!j.isSalaryVisible) return 'Confidential / Not disclosed';
    const min = j.minimumSalary;
    const max = j.maximumSalary;
    const cur = j.currency?.trim() ? ` ${j.currency.trim().toUpperCase()}` : '';
    const period = j.salaryPeriod !== null && j.salaryPeriod !== undefined ? ` / ${this.salaryPeriodLabels[j.salaryPeriod]}` : '';

    if (min !== null && min !== undefined && max !== null && max !== undefined) {
      return `${min.toLocaleString()} – ${max.toLocaleString()}${cur}${period}`;
    }
    if (min !== null && min !== undefined) {
      return `From ${min.toLocaleString()}${cur}${period}`;
    }
    return 'Not disclosed';
  });

  protected readonly locationString = computed(() => {
    const j = this.job();
    if (!j) return '';
    const parts = [j.city, j.country].filter(Boolean);
    return parts.join(', ');
  });

  protected readonly experienceString = computed(() => {
    const j = this.job();
    if (!j) return '';
    const level = this.experienceLabels[j.experienceLevel] || '';
    const min = j.minimumYearsExperience;
    const max = j.maximumYearsExperience;
    let years = '';
    if (min !== undefined && min !== null) {
      if (max !== undefined && max !== null) {
        years = `${min}–${max} years`;
      } else if (min > 0) {
        years = `${min}+ years`;
      }
    }
    if (level && years) return `${level} (${years})`;
    return level || years || 'Not specified';
  });

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
          if (this.authService.isCandidate()) {
            this.loadCvs();
          }
          this.jobsService.getSimilar(job.id, 4).subscribe({
            next: (jobs) => this.similarJobs.set(jobs),
            error: () => this.similarJobs.set([]),
          });
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
    this.candidateCvsService
      .getMine()
      .pipe(finalize(() => this.isLoadingCvs.set(false)))
      .subscribe({
        next: (cvs) => {
          this.cvs.set(cvs);
          const defaultCv = cvs.find((cv) => cv.isDefault) ?? cvs[0];
          if (defaultCv) {
            this.selectedCvId.set(defaultCv.id);
          }
        },
        error: (error: unknown) => {
          this.applyError.set(this.apiError.getMessage(error, 'Your CVs could not be loaded.'));
        },
      });
  }

  openApplyModal(): void {
    this.applyError.set(null);
    this.applyModalOpen.set(true);
    if (this.cvs().length === 0) {
      this.loadCvs();
    }
  }

  closeApplyModal(): void {
    this.applyModalOpen.set(false);
    this.showUploadForm.set(false);
    this.uploadError.set(null);
  }

  @HostListener('document:keydown.escape')
  closeApplyModalOnEscape(): void {
    if (this.applyModalOpen() && !this.isApplying()) {
      this.closeApplyModal();
    }
  }

  selectCv(id: number): void {
    this.selectedCvId.set(id);
  }

  updateCoverLetter(event: Event): void {
    this.coverLetter.set((event.target as HTMLTextAreaElement).value);
  }

  // Quick CV upload inside Apply Modal
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.validateAndSetFile(input.files[0]);
    }
  }

  onFileDropped(event: DragEvent): void {
    event.preventDefault();
    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      this.validateAndSetFile(event.dataTransfer.files[0]);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  private validateAndSetFile(file: File): void {
    this.uploadError.set(null);

    if (file.type !== 'application/pdf' && !file.name.toLowerCase().endsWith('.pdf')) {
      this.uploadError.set('Please select a PDF document. Other formats are not supported.');
      return;
    }

    if (file.size > MAX_CV_BYTES) {
      this.uploadError.set('PDF file size must not exceed 5 MB.');
      return;
    }

    this.selectedFile.set(file);
    if (!this.pendingCvName()) {
      const defaultName = file.name.replace(/\.pdf$/i, '').replace(/[-_]/g, ' ');
      this.pendingCvName.set(defaultName);
    }
  }

  updatePendingCvName(event: Event): void {
    this.pendingCvName.set((event.target as HTMLInputElement).value);
  }

  uploadNewCv(): void {
    const file = this.selectedFile();
    const name = this.pendingCvName().trim() || file?.name || 'Resume';

    if (!file) {
      this.uploadError.set('Please choose a PDF file to upload.');
      return;
    }

    this.isUploadingCv.set(true);
    this.uploadError.set(null);

    this.candidateCvsService
      .upload(name, file, this.cvs().length === 0)
      .pipe(finalize(() => this.isUploadingCv.set(false)))
      .subscribe({
        next: (newCv) => {
          this.cvs.update((list) => [newCv, ...list]);
          this.selectedCvId.set(newCv.id);
          this.showUploadForm.set(false);
          this.selectedFile.set(null);
          this.pendingCvName.set('');
        },
        error: (err: unknown) => {
          this.uploadError.set(this.apiError.getMessage(err, 'Failed to upload CV. Please verify file format and size.'));
        },
      });
  }

  submitApplication(): void {
    const jobId = this.getJobId();
    const candidateCvId = this.selectedCvId();

    if (jobId === null || candidateCvId === null || !this.authService.isCandidate() || !this.job()?.isActive) {
      return;
    }

    this.isApplying.set(true);
    this.applyError.set(null);

    this.applicationsService
      .apply({
        jobId,
        candidateCvId,
        coverLetter: this.coverLetter().trim() || null,
      })
      .pipe(finalize(() => this.isApplying.set(false)))
      .subscribe({
        next: (application) => {
          this.applySuccess.set(true);
          this.applyModalOpen.set(false);
          this.job.update((job) =>
            job ? { ...job, hasApplied: true, applicationId: application.id } : job,
          );
        },
        error: (error: HttpErrorResponse) => {
          this.applyError.set(
            this.apiError.getMessage(error, 'Your application could not be submitted. Please try again.'),
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
