import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ChangeDetectionStrategy } from '@angular/core';
import { finalize } from 'rxjs';
import { ApplicationsService } from '../../../core/services/applications.service';
import { ApiErrorService } from '../../../core/services/api-error.service';

@Component({
  selector: 'app-recruiter-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './recruiter-dashboard.component.html',
  styleUrl: './recruiter-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecruiterDashboardComponent implements OnInit {
  private readonly applicationsService = inject(ApplicationsService);
  private readonly apiError = inject(ApiErrorService);

  protected readonly totalApplications = signal(0);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.applicationsService
      .getAll(1, 1)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res) => {
          this.totalApplications.set(res.totalCount);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.apiError.getMessage(error, 'Unable to load dashboard statistics.'));
        },
      });
  }
}
