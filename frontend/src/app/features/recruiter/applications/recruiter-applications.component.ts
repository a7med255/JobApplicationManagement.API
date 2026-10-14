import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ChangeDetectionStrategy } from '@angular/core';
import { finalize } from 'rxjs';
import { JobApplicationResponse } from '../../../core/models/application.models';
import { ApplicationsService } from '../../../core/services/applications.service';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { ApplicationStatusBadgeComponent } from '../../../shared/components/application-status-badge/application-status-badge.component';

@Component({
  selector: 'app-recruiter-applications',
  standalone: true,
  imports: [DatePipe, RouterLink, ApplicationStatusBadgeComponent],
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
  
  protected readonly pageNumber = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly totalCount = signal(0);
  protected readonly pageSize = 15;

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

  nextPage(): void {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update((page) => page + 1);
      this.loadApplications();
    }
  }

  prevPage(): void {
    if (this.pageNumber() > 1) {
      this.pageNumber.update((page) => page - 1);
      this.loadApplications();
    }
  }

}
