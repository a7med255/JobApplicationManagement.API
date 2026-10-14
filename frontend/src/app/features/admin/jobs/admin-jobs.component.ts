import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ChangeDetectionStrategy } from '@angular/core';
import { finalize, Observable } from 'rxjs';
import { JobStatus, JobSummary } from '../../../core/models/job.models';
import { JobsService } from '../../../core/services/jobs.service';
import { ApiErrorService } from '../../../core/services/api-error.service';

@Component({
  selector: 'app-admin-jobs',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './admin-jobs.component.html',
  styleUrl: './admin-jobs.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminJobsComponent implements OnInit {
  private readonly jobsService = inject(JobsService);
  private readonly apiError = inject(ApiErrorService);

  protected readonly jobs = signal<JobSummary[]>([]);
  protected readonly JobStatus = JobStatus;
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly actionError = signal<string | null>(null);
  protected readonly pendingId = signal<number | null>(null);
  protected readonly confirmDeleteId = signal<number | null>(null);
  
  protected readonly pageNumber = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly totalCount = signal(0);
  protected readonly pageSize = 15;

  ngOnInit(): void {
    this.loadJobs();
  }

  loadJobs(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.actionError.set(null);

    this.jobsService
      .getAll(this.pageNumber(), this.pageSize, { includeInactive: true })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (result) => {
          this.jobs.set(result.items);
          this.totalPages.set(result.totalPages);
          this.totalCount.set(result.totalCount);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.apiError.getMessage(error, 'Failed to load jobs.'));
        },
      });
  }

  nextPage(): void {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update((page) => page + 1);
      this.loadJobs();
    }
  }

  prevPage(): void {
    if (this.pageNumber() > 1) {
      this.pageNumber.update((page) => page - 1);
      this.loadJobs();
    }
  }

  requestDelete(id: number): void {
    this.confirmDeleteId.set(id);
    this.actionError.set(null);
  }

  deleteJob(id: number): void {
    this.pendingId.set(id);
    this.actionError.set(null);
    this.jobsService
      .delete(id)
      .pipe(finalize(() => this.pendingId.set(null)))
      .subscribe({
        next: () => {
          this.confirmDeleteId.set(null);
          if (this.jobs().length === 1 && this.pageNumber() > 1) {
            this.pageNumber.update((page) => page - 1);
          }
          this.loadJobs();
        },
        error: (error: unknown) => {
          this.actionError.set(this.apiError.getMessage(error, 'Failed to delete the job.'));
        },
      });
  }

  setJobActive(job: JobSummary, isActive: boolean): void {
    this.pendingId.set(job.id);
    this.actionError.set(null);
    const request: Observable<unknown> = isActive
      ? (job.status === JobStatus.Draft ? this.jobsService.publish(job.id) : this.jobsService.reopen(job.id))
      : this.jobsService.close(job.id);

    request
      .pipe(finalize(() => this.pendingId.set(null)))
      .subscribe({
        next: () => {
          this.loadJobs();
        },
        error: (error: unknown) => {
          this.actionError.set(
            this.apiError.getMessage(error, `Failed to ${isActive ? 'reopen' : 'close'} the job.`),
          );
        },
      });
  }
}
