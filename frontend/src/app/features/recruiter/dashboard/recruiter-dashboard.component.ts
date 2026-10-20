import { Component, OnInit, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs';
import { ApplicationsService } from '../../../core/services/applications.service';
import { JobsService } from '../../../core/services/jobs.service';
import { CompaniesService } from '../../../core/services/companies.service';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { JobApplicationResponse, JobApplicationStatus } from '../../../core/models/application.models';
import { JobStatus, JobSummary } from '../../../core/models/job.models';
import { Company } from '../../../core/models/company.models';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { CompanyLogoComponent } from '../../../shared/components/company-logo/company-logo.component';

@Component({
  selector: 'app-recruiter-dashboard',
  standalone: true,
  imports: [RouterLink, DatePipe, StatusBadgeComponent, CompanyLogoComponent],
  templateUrl: './recruiter-dashboard.component.html',
  styleUrl: './recruiter-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecruiterDashboardComponent implements OnInit {
  private readonly applicationsService = inject(ApplicationsService);
  private readonly jobsService = inject(JobsService);
  private readonly companiesService = inject(CompaniesService);
  private readonly apiError = inject(ApiErrorService);

  protected readonly totalApplications = signal(0);
  protected readonly activeJobsCount = signal(0);
  protected readonly shortlistedCount = signal(0);
  protected readonly interviewStageCount = signal(0);
  protected readonly pipelineMetricsArePartial = signal(false);

  protected readonly recentApplications = signal<JobApplicationResponse[]>([]);
  protected readonly myJobs = signal<JobSummary[]>([]);
  protected readonly company = signal<Company | null>(null);

  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      applications: this.applicationsService.getAll(1, 50).pipe(catchError(() => of({ items: [], totalCount: 0, pageNumber: 1, pageSize: 50, totalPages: 0 }))),
      jobs: this.jobsService.getMine(1, 5).pipe(catchError(() => of({ items: [], totalCount: 0, pageNumber: 1, pageSize: 5, totalPages: 0 }))),
      activeJobs: this.jobsService.getMine(1, 1, { status: JobStatus.Published }).pipe(catchError(() => of({ items: [], totalCount: 0, pageNumber: 1, pageSize: 1, totalPages: 0 }))),
      company: this.companiesService.getMine().pipe(catchError(() => of(null))),
    })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: ({ applications, jobs, activeJobs, company }) => {
          this.totalApplications.set(applications.totalCount);
          this.recentApplications.set(applications.items.slice(0, 5));

          const shortlisted = applications.items.filter(
            (a) => a.jobApplicationStatus === JobApplicationStatus.Shortlisted
          ).length;
          this.shortlistedCount.set(shortlisted);
          this.interviewStageCount.set(applications.items.filter(
            (a) => a.jobApplicationStatus === JobApplicationStatus.Interview,
          ).length);
          this.pipelineMetricsArePartial.set(applications.totalCount > applications.items.length);

          this.myJobs.set(jobs.items.slice(0, 5));
          this.activeJobsCount.set(activeJobs.totalCount);

          if (company) {
            this.company.set(company);
          }
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.apiError.getMessage(error, 'Unable to load recruiter dashboard statistics.'));
        },
      });
  }
}
