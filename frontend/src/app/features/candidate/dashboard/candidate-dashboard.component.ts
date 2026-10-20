import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApplicationsService } from '../../../core/services/applications.service';
import { SavedJobsService } from '../../../core/services/saved-jobs.service';
import { AuthService } from '../../../core/auth/auth.service';
import { JobsService } from '../../../core/services/jobs.service';
import { CandidatesService } from '../../../core/services/candidates.service';
import { JobSummary } from '../../../core/models/job.models';
import { JobApplicationResponse } from '../../../core/models/application.models';
import { CandidateResponse } from '../../../core/models/candidate.models';
import { JobCardComponent } from '../../../shared/components/job-card/job-card.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { DatePipe } from '@angular/common';
import { catchError, finalize, forkJoin, of } from 'rxjs';

export interface CandidateUpcomingInterview {
  applicationId: number;
  jobTitle: string;
  companyName: string;
  scheduledAt: string;
  interviewType: string;
  durationMinutes: number;
  meetingLink?: string | null;
  location?: string | null;
}

@Component({
  selector: 'app-candidate-dashboard',
  standalone: true,
  imports: [RouterLink, JobCardComponent, StatusBadgeComponent, DatePipe],
  templateUrl: './candidate-dashboard.component.html',
  styleUrl: './candidate-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateDashboardComponent implements OnInit {
  private readonly applicationsService = inject(ApplicationsService);
  private readonly savedJobsService = inject(SavedJobsService);
  private readonly candidatesService = inject(CandidatesService);
  private readonly jobsService = inject(JobsService);
  protected readonly authService = inject(AuthService);

  protected readonly profile = signal<CandidateResponse | null>(null);
  protected readonly totalApplications = signal(0);
  protected readonly recentApplications = signal<JobApplicationResponse[]>([]);
  protected readonly savedJobsCount = signal(0);
  protected readonly upcomingInterviews = signal<CandidateUpcomingInterview[]>([]);
  protected readonly recommendations = signal<JobSummary[]>([]);

  protected readonly isLoading = signal(true);
  protected readonly hasPartialError = signal(false);

  protected readonly displayName = computed(() => {
    const prof = this.profile();
    if (prof?.name?.trim()) return prof.name;
    const email = this.authService.userEmail();
    return email ? email.split('@')[0] : 'Candidate';
  });

  protected readonly profileCompleteness = computed(() => {
    return this.profile()?.profileCompleteness ?? 0;
  });

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading.set(true);
    this.hasPartialError.set(false);

    forkJoin({
      profile: this.candidatesService.getMine().pipe(catchError(() => of(null))),
      applications: this.applicationsService.getAll(1, 50).pipe(catchError(() => of(null))),
      savedJobs: this.savedJobsService.getMySavedJobs(1, 1).pipe(catchError(() => of(null))),
      recommended: this.jobsService.getRecommended(4).pipe(catchError(() => of([]))),
    })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe(({ profile, applications, savedJobs, recommended }) => {
        if (profile) this.profile.set(profile);
        if (savedJobs) this.savedJobsCount.set(savedJobs.totalCount);
        if (recommended) this.recommendations.set(recommended);

        if (applications) {
          this.totalApplications.set(applications.totalCount);
          this.recentApplications.set(applications.items.slice(0, 4));

          // Extract upcoming interviews
          const now = new Date();
          const interviews: CandidateUpcomingInterview[] = [];

          for (const app of applications.items) {
            if (app.interviews && app.interviews.length > 0) {
              for (const iv of app.interviews) {
                if (iv.status !== 'Cancelled' && new Date(iv.scheduledAt) >= now) {
                  interviews.push({
                    applicationId: app.id,
                    jobTitle: app.jobTitle || `Job #${app.jobId}`,
                    companyName: app.companyName || 'Company not provided',
                    scheduledAt: iv.scheduledAt,
                    interviewType: iv.interviewType,
                    durationMinutes: iv.durationMinutes,
                    meetingLink: iv.meetingLink,
                    location: iv.location,
                  });
                }
              }
            }
          }

          interviews.sort((a, b) => new Date(a.scheduledAt).getTime() - new Date(b.scheduledAt).getTime());
          this.upcomingInterviews.set(interviews);
        }

        this.hasPartialError.set(!profile && !applications);
      });
  }
}
