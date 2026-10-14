import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { JobApplicationResponse } from '../../../core/models/application.models';
import { ApplicationsService } from '../../../core/services/applications.service';
import { finalize } from 'rxjs';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { ApplicationStatusBadgeComponent } from '../../../shared/components/application-status-badge/application-status-badge.component';

@Component({
  selector: 'app-candidate-applications',
  standalone: true,
  imports: [DatePipe, RouterLink, ApplicationStatusBadgeComponent],
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
  
  protected readonly pageNumber = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly totalCount = signal(0);
  protected readonly pageSize = 10;

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

  nextPage(): void {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update(p => p + 1);
      this.loadApplications();
    }
  }

  prevPage(): void {
    if (this.pageNumber() > 1) {
      this.pageNumber.update(p => p - 1);
      this.loadApplications();
    }
  }
}
