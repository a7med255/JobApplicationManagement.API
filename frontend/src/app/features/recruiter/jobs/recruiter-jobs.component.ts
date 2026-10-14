import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { finalize, Observable } from 'rxjs';
import { JOB_STATUS_LABELS, JobStatus, JobSummary } from '../../../core/models/job.models';
import { JobsService } from '../../../core/services/jobs.service';
import { ApiErrorService } from '../../../core/services/api-error.service';

@Component({
  selector: 'app-recruiter-jobs',
  standalone: true,
  imports: [RouterLink, DatePipe],
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
  protected readonly pageNumber = signal(1);
  protected readonly totalPages = signal(1);

  ngOnInit(): void { this.load(); }

  load(): void {
    this.isLoading.set(true);
    this.jobsService.getMine(this.pageNumber(), 15).pipe(finalize(() => this.isLoading.set(false))).subscribe({
      next: (result) => { this.jobs.set(result.items); this.totalPages.set(result.totalPages); },
      error: (error: unknown) => this.errorMessage.set(this.apiError.getMessage(error, 'Your jobs could not be loaded.')),
    });
  }

  publish(id: number): void { this.run(id, this.jobsService.publish(id)); }
  close(id: number): void { this.run(id, this.jobsService.close(id)); }
  reopen(id: number): void { this.run(id, this.jobsService.reopen(id)); }
  archive(id: number): void { this.run(id, this.jobsService.archive(id)); }
  delete(id: number): void { this.run(id, this.jobsService.delete(id)); }

  previous(): void { if (this.pageNumber() > 1) { this.pageNumber.update((page) => page - 1); this.load(); } }
  next(): void { if (this.pageNumber() < this.totalPages()) { this.pageNumber.update((page) => page + 1); this.load(); } }

  private run(id: number, request: Observable<unknown>): void {
    this.pendingId.set(id);
    this.errorMessage.set(null);
    request.pipe(finalize(() => this.pendingId.set(null))).subscribe({
      next: () => this.load(),
      error: (error: unknown) => this.errorMessage.set(this.apiError.getMessage(error, 'The job could not be updated.')),
    });
  }
}
