import { ChangeDetectionStrategy, Component, HostListener, inject, OnInit, signal, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { finalize, Observable } from 'rxjs';
import { JOB_STATUS_LABELS, JobStatus, JobSummary } from '../../../core/models/job.models';
import { JobsService } from '../../../core/services/jobs.service';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

type JobFilterTab = 'all' | 'published' | 'draft' | 'closed' | 'archived';

@Component({
  selector: 'app-recruiter-jobs',
  standalone: true,
  imports: [RouterLink, DatePipe, FormsModule, StatusBadgeComponent, PaginationComponent, EmptyStateComponent],
  templateUrl: './recruiter-jobs.component.html',
  styleUrl: './recruiter-jobs.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecruiterJobsComponent implements OnInit {
  private readonly jobsService = inject(JobsService);
  private readonly apiError = inject(ApiErrorService);

  protected readonly JobStatus = JobStatus;
  protected readonly statusLabels = JOB_STATUS_LABELS;

  protected readonly jobs = signal<JobSummary[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly pendingId = signal<number | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);
  protected readonly deletingJob = signal<JobSummary | null>(null);

  protected readonly selectedTab = signal<JobFilterTab>('all');
  protected readonly searchQuery = signal('');

  protected readonly pageNumber = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly totalCount = signal(0);
  protected readonly pageSize = 12;

  protected readonly filteredJobs = computed(() => {
    return this.jobs();
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.jobsService
      .getMine(this.pageNumber(), this.pageSize, {
        search: this.searchQuery().trim() || undefined,
        status: this.statusForTab(this.selectedTab()),
      })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (result) => {
          this.jobs.set(result.items);
          this.totalPages.set(result.totalPages);
          this.totalCount.set(result.totalCount);
        },
        error: (error: unknown) =>
          this.errorMessage.set(this.apiError.getMessage(error, 'Your jobs could not be loaded.')),
      });
  }

  setTab(tab: JobFilterTab): void {
    this.selectedTab.set(tab);
    this.pageNumber.set(1);
    this.load();
  }

  applySearch(): void {
    this.pageNumber.set(1);
    this.load();
  }

  clearSearch(): void {
    this.searchQuery.set('');
    this.applySearch();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  publish(id: number): void {
    this.run(id, this.jobsService.publish(id), 'Job has been published successfully.');
  }

  close(id: number): void {
    this.run(id, this.jobsService.close(id), 'Job has been closed.');
  }

  reopen(id: number): void {
    this.run(id, this.jobsService.reopen(id), 'Job has been reopened.');
  }

  archive(id: number): void {
    this.run(id, this.jobsService.archive(id), 'Job has been archived.');
  }

  requestDelete(job: JobSummary): void {
    this.deletingJob.set(job);
  }

  cancelDelete(): void {
    this.deletingJob.set(null);
  }

  confirmDelete(): void {
    const job = this.deletingJob();
    if (!job) return;
    this.deletingJob.set(null);
    this.run(job.id, this.jobsService.delete(job.id), 'Job has been deleted.');
  }

  @HostListener('document:keydown.escape')
  closeDeleteDialogOnEscape(): void {
    this.cancelDelete();
  }

  countByTab(tab: JobFilterTab): number {
    const list = this.jobs();
    if (tab === 'all') return list.length;
    return list.filter((job) => {
      switch (tab) {
        case 'published':
          return job.status === JobStatus.Published;
        case 'draft':
          return job.status === JobStatus.Draft;
        case 'closed':
          return job.status === JobStatus.Closed || job.status === JobStatus.Expired;
        case 'archived':
          return job.status === JobStatus.Archived;
        default:
          return false;
      }
    }).length;
  }

  private statusForTab(tab: JobFilterTab): JobStatus | undefined {
    switch (tab) {
      case 'published': return JobStatus.Published;
      case 'draft': return JobStatus.Draft;
      case 'closed': return JobStatus.Closed;
      case 'archived': return JobStatus.Archived;
      default: return undefined;
    }
  }

  private run(id: number, request: Observable<unknown>, successMsg: string): void {
    this.pendingId.set(id);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    request.pipe(finalize(() => this.pendingId.set(null))).subscribe({
      next: () => {
        this.successMessage.set(successMsg);
        this.load();
      },
      error: (error: unknown) =>
        this.errorMessage.set(this.apiError.getMessage(error, 'The job could not be updated.')),
    });
  }
}
