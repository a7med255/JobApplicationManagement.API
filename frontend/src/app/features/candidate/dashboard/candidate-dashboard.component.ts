import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApplicationsService } from '../../../core/services/applications.service';
import { SavedJobsService } from '../../../core/services/saved-jobs.service';
import { AuthService } from '../../../core/auth/auth.service';
import { catchError, finalize, forkJoin, of } from 'rxjs';

@Component({
  selector: 'app-candidate-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './candidate-dashboard.component.html',
  styleUrl: './candidate-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateDashboardComponent implements OnInit {
  private readonly applicationsService = inject(ApplicationsService);
  private readonly savedJobsService = inject(SavedJobsService);
  protected readonly authService = inject(AuthService);

  protected readonly totalApplications = signal(0);
  protected readonly savedJobsCount = signal(0);
  protected readonly isLoading = signal(true);
  protected readonly hasPartialError = signal(false);

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.isLoading.set(true);
    this.hasPartialError.set(false);

    forkJoin({
      applications: this.applicationsService.getAll(1, 1).pipe(catchError(() => of(null))),
      savedJobs: this.savedJobsService.getMySavedJobs(1, 1).pipe(catchError(() => of(null))),
    })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe(({ applications, savedJobs }) => {
        this.totalApplications.set(applications?.totalCount ?? 0);
        this.savedJobsCount.set(savedJobs?.totalCount ?? 0);
        this.hasPartialError.set(!applications || !savedJobs);
      });
  }
}
