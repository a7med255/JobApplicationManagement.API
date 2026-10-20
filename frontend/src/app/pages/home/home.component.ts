import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { JobSummary } from '../../core/models/job.models';
import { JobsService } from '../../core/services/jobs.service';
import { JobCardComponent } from '../../shared/components/job-card/job-card.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule, JobCardComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent implements OnInit {
  protected readonly auth = inject(AuthService);
  private readonly jobs = inject(JobsService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  protected readonly recentJobs = signal<JobSummary[]>([]);
  protected readonly isLoadingJobs = signal(true);
  protected readonly searchForm = this.fb.nonNullable.group({ search: [''], city: [''] });
  protected readonly popularSearches = computed(() =>
    [...new Set(this.recentJobs().flatMap((job) => job.requiredSkills))].slice(0, 6),
  );
  protected readonly categories = computed(() =>
    [...new Set(this.recentJobs().map((job) => job.category).filter(Boolean))].slice(0, 6),
  );

  ngOnInit(): void {
    this.jobs.getAll(1, 6).subscribe({
      next: (result) => {
        this.recentJobs.set(result.items);
        this.isLoadingJobs.set(false);
      },
      error: () => {
        this.recentJobs.set([]);
        this.isLoadingJobs.set(false);
      },
    });
  }

  search(): void {
    const { search, city } = this.searchForm.getRawValue();
    void this.router.navigate(['/jobs'], {
      queryParams: { search: search.trim() || null, city: city.trim() || null },
    });
  }

  applyPopularSearch(skill: string): void {
    void this.router.navigate(['/jobs'], { queryParams: { skill } });
  }

  browseCategory(category: string): void {
    void this.router.navigate(['/jobs'], { queryParams: { category } });
  }
}
