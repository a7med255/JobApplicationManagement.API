import { ChangeDetectionStrategy, Component, OnInit, inject, signal, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { JobApplicationResponse, JobApplicationStatus } from '../../../core/models/application.models';
import { ApplicationsService } from '../../../core/services/applications.service';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { CompanyLogoComponent } from '../../../shared/components/company-logo/company-logo.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

type FilterTab = 'all' | 'applied' | 'under_review' | 'shortlisted' | 'interview' | 'accepted' | 'rejected';

@Component({
  selector: 'app-candidate-applications',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink,
    FormsModule,
    StatusBadgeComponent,
    CompanyLogoComponent,
    PaginationComponent,
    EmptyStateComponent,
  ],
  templateUrl: './candidate-applications.component.html',
  styleUrl: './candidate-applications.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateApplicationsComponent implements OnInit {
  private readonly applicationsService = inject(ApplicationsService);
  private readonly apiError = inject(ApiErrorService);

  protected readonly applications = signal<JobApplicationResponse[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly isError = signal(false);
  protected readonly errorMessage = signal('');
  
  protected readonly selectedTab = signal<FilterTab>('all');
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
          (app.jobTitle?.toLowerCase().includes(query) ?? false) ||
          (app.companyName?.toLowerCase().includes(query) ?? false),
      );
    }

    return list;
  });

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications(): void {
    this.isLoading.set(true);
    this.isError.set(false);
    this.errorMessage.set('');

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
          this.isError.set(true);
          this.errorMessage.set(
            this.apiError.getMessage(error, 'Applications could not be loaded. Please try again.'),
          );
        },
      });
  }

  setTab(tab: FilterTab): void {
    this.selectedTab.set(tab);
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadApplications();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  countByStatus(tab: FilterTab): number {
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
