import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ChangeDetectionStrategy } from '@angular/core';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import { JobsService } from '../../../core/services/jobs.service';
import { ApplicationsService } from '../../../core/services/applications.service';
import { CandidatesService } from '../../../core/services/candidates.service';
import { RecruitersService } from '../../../core/services/recruiters.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminDashboardComponent implements OnInit {
  private readonly jobsService = inject(JobsService);
  private readonly applicationsService = inject(ApplicationsService);
  private readonly candidatesService = inject(CandidatesService);
  private readonly recruitersService = inject(RecruitersService);

  protected readonly totalJobs = signal(0);
  protected readonly totalApplications = signal(0);
  protected readonly totalCandidates = signal(0);
  protected readonly totalRecruiters = signal(0);
  protected readonly isLoading = signal(true);
  protected readonly hasPartialError = signal(false);

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.isLoading.set(true);
    this.hasPartialError.set(false);

    forkJoin({
      jobs: this.jobsService.getAll(1, 1, { includeInactive: true }).pipe(catchError(() => of(null))),
      applications: this.applicationsService.getAll(1, 1).pipe(catchError(() => of(null))),
      candidates: this.candidatesService.getAll(1, 1).pipe(catchError(() => of(null))),
      recruiters: this.recruitersService.getAll(1, 1).pipe(catchError(() => of(null))),
    })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe(({ jobs, applications, candidates, recruiters }) => {
        this.totalJobs.set(jobs?.totalCount ?? 0);
        this.totalApplications.set(applications?.totalCount ?? 0);
        this.totalCandidates.set(candidates?.totalCount ?? 0);
        this.totalRecruiters.set(recruiters?.totalCount ?? 0);
        this.hasPartialError.set(!jobs || !applications || !candidates || !recruiters);
      });
  }
}
