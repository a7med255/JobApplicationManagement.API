import { Component, OnInit, inject, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { JobApplicationResponse, JobApplicationStatus } from '../../../core/models/application.models';
import { ApplicationsService } from '../../../core/services/applications.service';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

type RecruiterAppFilterTab = 'all' | 'applied' | 'under_review' | 'shortlisted' | 'interview' | 'accepted' | 'rejected';

@Component({
  selector: 'app-recruiter-applications',
  standalone: true,
  imports: [DatePipe, RouterLink, FormsModule, StatusBadgeComponent, PaginationComponent, EmptyStateComponent],
  templateUrl: './recruiter-applications.component.html',
  styleUrl: './recruiter-applications.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecruiterApplicationsComponent implements OnInit {
  private readonly applicationsService = inject(ApplicationsService);
  private readonly apiError = inject(ApiErrorService);

  protected readonly applications = signal<JobApplicationResponse[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly selectedTab = signal<RecruiterAppFilterTab>('all');
  protected readonly searchQuery = signal('');

  protected readonly pageNumber = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly totalCount = signal(0);
  protected readonly pageSize = 50;

  protected readonly filteredApplications = computed(() => {
    let list = this.applications();
    const tab = this.selectedTab();
    const query = this.searchQuery().trim().toLowerCase();

    if (tab !== 'all') {
      list = list.filter((app) => {
        switch (tab) {
          case 'applied':
            return app.jobApplicationStatus === JobApplicationStatus.Applied;
          case 'under_review':
            return app.jobApplicationStatus === JobApplicationStatus.UnderReview;
          case 'shortlisted':
            return app.jobApplicationStatus === JobApplicationStatus.Shortlisted;
          case 'interview':
            return app.jobApplicationStatus === JobApplicationStatus.Interview;
          case 'accepted':
            return app.jobApplicationStatus === JobApplicationStatus.Accepted;
          case 'rejected':
            return app.jobApplicationStatus === JobApplicationStatus.Rejected;
          default:
            return true;
        }
      });
    }

    if (query) {
      list = list.filter(
        (app) =>
          (app.candidateName?.toLowerCase().includes(query) ?? false) ||
          (app.jobTitle?.toLowerCase().includes(query) ?? false) ||
          String(app.id).includes(query)
      );
    }

    return list;
  });

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.applicationsService
      .getAll(this.pageNumber(), this.pageSize)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res) => {
          this.applications.set(res.items);
          this.totalPages.set(res.totalPages);
          this.totalCount.set(res.totalCount);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.apiError.getMessage(error, 'Failed to load applications.'));
        },
      });
  }

  setTab(tab: RecruiterAppFilterTab): void {
    this.selectedTab.set(tab);
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadApplications();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  countByTab(tab: RecruiterAppFilterTab): number {
    const list = this.applications();
    if (tab === 'all') return list.length;
    return list.filter((app) => {
      switch (tab) {
        case 'applied':
          return app.jobApplicationStatus === JobApplicationStatus.Applied;
        case 'under_review':
          return app.jobApplicationStatus === JobApplicationStatus.UnderReview;
        case 'shortlisted':
          return app.jobApplicationStatus === JobApplicationStatus.Shortlisted;
        case 'interview':
          return app.jobApplicationStatus === JobApplicationStatus.Interview;
        case 'accepted':
          return app.jobApplicationStatus === JobApplicationStatus.Accepted;
        case 'rejected':
          return app.jobApplicationStatus === JobApplicationStatus.Rejected;
        default:
          return false;
      }
    }).length;
  }
}
