import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ChangeDetectionStrategy } from '@angular/core';
import { finalize } from 'rxjs';
import { JobApplicationResponse } from '../../../core/models/application.models';
import { ApplicationsService } from '../../../core/services/applications.service';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-admin-applications',
  standalone: true,
  imports: [DatePipe, RouterLink, StatusBadgeComponent, EmptyStateComponent, PaginationComponent],
  templateUrl: './admin-applications.component.html',
  styleUrl: './admin-applications.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminApplicationsComponent implements OnInit {
  private readonly applicationsService = inject(ApplicationsService);
  private readonly apiError = inject(ApiErrorService);

  protected readonly applications = signal<JobApplicationResponse[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly actionError = signal<string | null>(null);
  protected readonly pendingId = signal<number | null>(null);
  protected readonly confirmDeleteId = signal<number | null>(null);
  
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
    this.actionError.set(null);

    this.applicationsService
      .getAll(this.pageNumber(), this.pageSize)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (result) => {
          this.applications.set(result.items);
          this.totalPages.set(result.totalPages);
          this.totalCount.set(result.totalCount);
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

  cancelDelete(): void {
    this.confirmDeleteId.set(null);
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadApplications();
  }

  requestDelete(id: number): void {
    this.confirmDeleteId.set(id);
    this.actionError.set(null);
  }

  deleteApplication(id: number): void {
    this.pendingId.set(id);
    this.actionError.set(null);
    this.applicationsService
      .delete(id)
      .pipe(finalize(() => this.pendingId.set(null)))
      .subscribe({
        next: () => {
          this.confirmDeleteId.set(null);
          if (this.applications().length === 1 && this.pageNumber() > 1) {
            this.pageNumber.update((page) => page - 1);
          }
          this.loadApplications();
        },
        error: (error: unknown) => {
          this.actionError.set(this.apiError.getMessage(error, 'Failed to delete the application.'));
        },
      });
  }
}
