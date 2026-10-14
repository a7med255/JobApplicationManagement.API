import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ChangeDetectionStrategy } from '@angular/core';
import { JobCardData } from '../../../shared/components/job-card/job-card.component';
import { SavedJobsService } from '../../../core/services/saved-jobs.service';
import { JobCardComponent } from '../../../shared/components/job-card/job-card.component';
import { finalize } from 'rxjs';
import { ApiErrorService } from '../../../core/services/api-error.service';

interface SavedJobView {
  savedJobId: number;
  job: JobCardData;
}

@Component({
  selector: 'app-candidate-saved-jobs',
  standalone: true,
  imports: [JobCardComponent],
  templateUrl: './candidate-saved-jobs.component.html',
  styleUrl: './candidate-saved-jobs.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateSavedJobsComponent implements OnInit {
  private readonly savedJobsService = inject(SavedJobsService);
  private readonly router = inject(Router);
  private readonly apiError = inject(ApiErrorService);

  protected readonly savedJobs = signal<SavedJobView[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly isError = signal(false);
  protected readonly errorMessage = signal('');
  protected readonly actionError = signal<string | null>(null);
  protected readonly removingJobId = signal<number | null>(null);
  
  protected readonly pageNumber = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly pageSize = 12;

  ngOnInit(): void {
    this.loadSavedJobs();
  }

  loadSavedJobs(): void {
    this.isLoading.set(true);
    this.isError.set(false);

    this.errorMessage.set('');

    this.savedJobsService
      .getMySavedJobs(this.pageNumber(), this.pageSize)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res) => {
          this.savedJobs.set(
            res.items.map((item) => ({
              savedJobId: item.id,
              job: {
                id: item.jobId,
                title: item.jobTitle,
                description: item.jobDescription,
                isActive: item.isActive,
              },
            })),
          );
          this.totalPages.set(res.totalPages);
        },
        error: (error: unknown) => {
          this.isError.set(true);
          this.errorMessage.set(
            this.apiError.getMessage(error, 'Saved jobs could not be loaded. Please try again.'),
          );
        },
      });
  }

  nextPage(): void {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update(p => p + 1);
      this.loadSavedJobs();
    }
  }

  prevPage(): void {
    if (this.pageNumber() > 1) {
      this.pageNumber.update(p => p - 1);
      this.loadSavedJobs();
    }
  }

  onApply(jobId: number): void {
    void this.router.navigate(['/jobs', jobId]);
  }

  onSaveToggle(jobId: number): void {
    if (this.removingJobId() !== null) {
      return;
    }

    this.removingJobId.set(jobId);
    this.actionError.set(null);
    this.savedJobsService
      .remove(jobId)
      .pipe(finalize(() => this.removingJobId.set(null)))
      .subscribe({
        next: () => this.loadSavedJobs(),
        error: (error: unknown) => {
          this.actionError.set(
            this.apiError.getMessage(error, 'The job could not be removed from your saved jobs.'),
          );
        },
      });
  }
}
