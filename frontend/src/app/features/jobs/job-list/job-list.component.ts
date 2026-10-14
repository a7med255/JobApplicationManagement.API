import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import {
  EmploymentType,
  ExperienceLevel,
  JobSortOrder,
  JobSummary,
  WorkplaceType,
} from '../../../core/models/job.models';
import { JobsService } from '../../../core/services/jobs.service';
import { AuthService } from '../../../core/auth/auth.service';
import { JobCardComponent } from '../../../shared/components/job-card/job-card.component';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-job-list',
  standalone: true,
  imports: [JobCardComponent, ReactiveFormsModule],
  templateUrl: './job-list.component.html',
  styleUrl: './job-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobListComponent implements OnInit {
  private readonly jobsService = inject(JobsService);
  protected readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly apiError = inject(ApiErrorService);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly jobs = signal<JobSummary[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly isError = signal(false);
  protected readonly errorMessage = signal('');
  
  protected readonly pageNumber = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly totalCount = signal(0);
  protected readonly pageSize = 12;
  protected readonly WorkplaceType = WorkplaceType;
  protected readonly EmploymentType = EmploymentType;
  protected readonly ExperienceLevel = ExperienceLevel;
  protected readonly JobSortOrder = JobSortOrder;
  protected readonly searchForm = this.formBuilder.group({
    search: [''],
    city: [''],
    skill: [''],
    workplaceType: this.formBuilder.control<WorkplaceType | null>(null),
    employmentType: this.formBuilder.control<EmploymentType | null>(null),
    experienceLevel: this.formBuilder.control<ExperienceLevel | null>(null),
    sort: this.formBuilder.nonNullable.control(JobSortOrder.Newest),
  });

  ngOnInit(): void {
    this.loadJobs();
  }

  loadJobs(): void {
    this.isLoading.set(true);
    this.isError.set(false);

    this.errorMessage.set('');

    this.jobsService
      .getAll(this.pageNumber(), this.pageSize, this.cleanFilters())
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (result) => {
          this.jobs.set(result.items);
          this.totalPages.set(result.totalPages);
          this.totalCount.set(result.totalCount);
        },
        error: (error: unknown) => {
          this.isError.set(true);
          this.errorMessage.set(
            this.apiError.getMessage(error, 'Jobs could not be loaded. Please try again.'),
          );
        },
      });
  }

  search(): void {
    this.pageNumber.set(1);
    this.loadJobs();
  }

  clearFilters(): void {
    this.searchForm.reset({ search: '', city: '', skill: '', workplaceType: null, employmentType: null, experienceLevel: null, sort: JobSortOrder.Newest });
    this.search();
  }

  nextPage(): void {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update(p => p + 1);
      this.loadJobs();
    }
  }

  prevPage(): void {
    if (this.pageNumber() > 1) {
      this.pageNumber.update(p => p - 1);
      this.loadJobs();
    }
  }

  onApply(jobId: number): void {
    void this.router.navigate(['/jobs', jobId]);
  }

  private cleanFilters() {
    const values = this.searchForm.getRawValue();
    return {
      search: values.search?.trim() || undefined,
      city: values.city?.trim() || undefined,
      skill: values.skill?.trim() || undefined,
      workplaceType: values.workplaceType ?? undefined,
      employmentType: values.employmentType ?? undefined,
      experienceLevel: values.experienceLevel ?? undefined,
      sort: values.sort,
    };
  }
}
