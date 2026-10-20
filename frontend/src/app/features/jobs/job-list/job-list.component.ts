import { ChangeDetectionStrategy, Component, computed, HostListener, inject, OnInit, signal } from '@angular/core';
import {
  EMPLOYMENT_TYPE_LABELS,
  EmploymentType,
  EXPERIENCE_LEVEL_LABELS,
  ExperienceLevel,
  JobSortOrder,
  JobSummary,
  WORKPLACE_TYPE_LABELS,
  WorkplaceType,
} from '../../../core/models/job.models';
import { JobsService } from '../../../core/services/jobs.service';
import { AuthService } from '../../../core/auth/auth.service';
import { JobCardComponent } from '../../../shared/components/job-card/job-card.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';

export interface ActiveFilterChip {
  key: string;
  label: string;
}

@Component({
  selector: 'app-job-list',
  standalone: true,
  imports: [JobCardComponent, PaginationComponent, EmptyStateComponent, ReactiveFormsModule],
  templateUrl: './job-list.component.html',
  styleUrl: './job-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobListComponent implements OnInit {
  private readonly jobsService = inject(JobsService);
  protected readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly apiError = inject(ApiErrorService);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly jobs = signal<JobSummary[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly isError = signal(false);
  protected readonly errorMessage = signal('');
  
  protected readonly pageNumber = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly totalCount = signal(0);
  protected readonly pageSize = 12;

  // Mobile Filter Drawer State
  protected readonly mobileFiltersOpen = signal(false);
  private readonly filterVersion = signal(0);

  // Enums and labels for template
  protected readonly WorkplaceType = WorkplaceType;
  protected readonly EmploymentType = EmploymentType;
  protected readonly ExperienceLevel = ExperienceLevel;
  protected readonly JobSortOrder = JobSortOrder;

  protected readonly workplaceLabels = WORKPLACE_TYPE_LABELS;
  protected readonly employmentLabels = EMPLOYMENT_TYPE_LABELS;
  protected readonly experienceLabels = EXPERIENCE_LEVEL_LABELS;

  protected readonly searchForm = this.formBuilder.group({
    search: [''],
    city: [''],
    country: [''],
    category: [''],
    skill: [''],
    workplaceType: this.formBuilder.control<WorkplaceType | null>(null),
    employmentType: this.formBuilder.control<EmploymentType | null>(null),
    experienceLevel: this.formBuilder.control<ExperienceLevel | null>(null),
    minimumSalary: this.formBuilder.control<number | null>(null),
    maximumSalary: this.formBuilder.control<number | null>(null),
    postedWithinDays: this.formBuilder.control<number | null>(null),
    sort: this.formBuilder.nonNullable.control(JobSortOrder.Newest),
  });

  // Reactive Active Filter Chips
  protected readonly activeFilterChips = computed<ActiveFilterChip[]>(() => {
    this.filterVersion();
    const chips: ActiveFilterChip[] = [];
    const val = this.searchForm.value;

    if (val.search) chips.push({ key: 'search', label: `Keyword: "${val.search}"` });
    if (val.city) chips.push({ key: 'city', label: `Location: ${val.city}` });
    if (val.country) chips.push({ key: 'country', label: `Country: ${val.country}` });
    if (val.category) chips.push({ key: 'category', label: `Category: ${val.category}` });
    if (val.skill) chips.push({ key: 'skill', label: `Skill: ${val.skill}` });
    if (val.workplaceType !== null && val.workplaceType !== undefined) {
      chips.push({ key: 'workplaceType', label: this.workplaceLabels[val.workplaceType] });
    }
    if (val.employmentType !== null && val.employmentType !== undefined) {
      chips.push({ key: 'employmentType', label: this.employmentLabels[val.employmentType] });
    }
    if (val.experienceLevel !== null && val.experienceLevel !== undefined) {
      chips.push({ key: 'experienceLevel', label: this.experienceLabels[val.experienceLevel] });
    }
    if (val.minimumSalary !== null && val.minimumSalary !== undefined) {
      chips.push({ key: 'minimumSalary', label: `Salary from ${val.minimumSalary}` });
    }
    if (val.maximumSalary !== null && val.maximumSalary !== undefined) {
      chips.push({ key: 'maximumSalary', label: `Salary to ${val.maximumSalary}` });
    }
    if (val.postedWithinDays) {
      const daysText = val.postedWithinDays === 1 ? 'Past 24 hours' : `Past ${val.postedWithinDays} days`;
      chips.push({ key: 'postedWithinDays', label: daysText });
    }

    return chips;
  });

  protected readonly activeFilterCount = computed(() => this.activeFilterChips().length);

  ngOnInit(): void {
    this.route.queryParamMap.subscribe((params) => {
      const page = Number(params.get('page'));
      this.pageNumber.set(Number.isInteger(page) && page > 0 ? page : 1);

      this.searchForm.patchValue({
        search: params.get('search') ?? '',
        city: params.get('city') ?? '',
        country: params.get('country') ?? '',
        category: params.get('category') ?? '',
        skill: params.get('skill') ?? '',
        workplaceType: this.enumValue<WorkplaceType>(params.get('workplaceType')),
        employmentType: this.enumValue<EmploymentType>(params.get('employmentType')),
        experienceLevel: this.enumValue<ExperienceLevel>(params.get('experienceLevel')),
        minimumSalary: this.decimalValue(params.get('minimumSalary')),
        maximumSalary: this.decimalValue(params.get('maximumSalary')),
        postedWithinDays: this.numberValue(params.get('postedWithinDays')),
        sort: this.enumValue<JobSortOrder>(params.get('sort')) ?? JobSortOrder.Newest,
      }, { emitEvent: false });

      this.filterVersion.update((version) => version + 1);

      this.loadJobs();
    });
  }

  loadJobs(): void {
    this.isLoading.set(true);
    this.isError.set(false);
    this.errorMessage.set('');

    this.jobsService
      .getAll(this.pageNumber(), this.pageSize, this.cleanFilters())
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (result) => {
          this.jobs.set(result.items);
          this.totalPages.set(result.totalPages);
          this.totalCount.set(result.totalCount);
        },
        error: (error: unknown) => {
          this.isError.set(true);
          this.errorMessage.set(
            this.apiError.getMessage(error, 'Jobs could not be loaded. Please try again.'),
          );
        },
      });
  }

  submitSearch(): void {
    this.pageNumber.set(1);
    this.closeMobileFilters();
    this.syncUrlAndLoad();
  }

  onSortChange(): void {
    this.pageNumber.set(1);
    this.syncUrlAndLoad();
  }

  onFilterSelect(): void {
    this.filterVersion.update((version) => version + 1);
    this.pageNumber.set(1);
    this.syncUrlAndLoad();
  }

  removeFilter(key: string): void {
    if (key === 'search') this.searchForm.controls.search.setValue('');
    else if (key === 'city') this.searchForm.controls.city.setValue('');
    else if (key === 'country') this.searchForm.controls.country.setValue('');
    else if (key === 'category') this.searchForm.controls.category.setValue('');
    else if (key === 'skill') this.searchForm.controls.skill.setValue('');
    else if (key === 'workplaceType') this.searchForm.controls.workplaceType.setValue(null);
    else if (key === 'employmentType') this.searchForm.controls.employmentType.setValue(null);
    else if (key === 'experienceLevel') this.searchForm.controls.experienceLevel.setValue(null);
    else if (key === 'minimumSalary') this.searchForm.controls.minimumSalary.setValue(null);
    else if (key === 'maximumSalary') this.searchForm.controls.maximumSalary.setValue(null);
    else if (key === 'postedWithinDays') this.searchForm.controls.postedWithinDays.setValue(null);

    this.filterVersion.update((version) => version + 1);
    this.pageNumber.set(1);
    this.syncUrlAndLoad();
  }

  clearFilters(): void {
    this.searchForm.reset({
      search: '',
      city: '',
      country: '',
      category: '',
      skill: '',
      workplaceType: null,
      employmentType: null,
      experienceLevel: null,
      minimumSalary: null,
      maximumSalary: null,
      postedWithinDays: null,
      sort: JobSortOrder.Newest,
    });
    this.filterVersion.update((version) => version + 1);
    this.pageNumber.set(1);
    this.closeMobileFilters();
    this.syncUrlAndLoad();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.syncUrlAndLoad();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  toggleMobileFilters(): void {
    this.mobileFiltersOpen.update((open) => !open);
  }

  closeMobileFilters(): void {
    this.mobileFiltersOpen.set(false);
  }

  @HostListener('document:keydown.escape')
  closeFiltersOnEscape(): void {
    this.closeMobileFilters();
  }

  onApply(jobId: number): void {
    void this.router.navigate(['/jobs', jobId]);
  }

  private syncUrlAndLoad(): void {
    const filters = this.cleanFilters();
    const queryParams: Record<string, string | number | undefined> = {
      ...filters,
      page: this.pageNumber() > 1 ? this.pageNumber() : undefined,
    };

    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      replaceUrl: true,
    });
  }

  private cleanFilters() {
    const values = this.searchForm.getRawValue();
    return {
      search: values.search?.trim() || undefined,
      city: values.city?.trim() || undefined,
      country: values.country?.trim() || undefined,
      category: values.category?.trim() || undefined,
      skill: values.skill?.trim() || undefined,
      workplaceType: values.workplaceType ?? undefined,
      employmentType: values.employmentType ?? undefined,
      experienceLevel: values.experienceLevel ?? undefined,
      minimumSalary: values.minimumSalary ?? undefined,
      maximumSalary: values.maximumSalary ?? undefined,
      postedWithinDays: values.postedWithinDays ?? undefined,
      sort: values.sort,
    };
  }

  private enumValue<T extends number>(value: string | null): T | null {
    if (value === null || value === '') return null;
    const parsed = Number(value);
    return Number.isInteger(parsed) ? (parsed as T) : null;
  }

  private numberValue(value: string | null): number | null {
    if (value === null || value === '') return null;
    const parsed = Number(value);
    return Number.isInteger(parsed) ? parsed : null;
  }

  private decimalValue(value: string | null): number | null {
    if (value === null || value === '') return null;
    const parsed = Number(value);
    return Number.isFinite(parsed) && parsed >= 0 ? parsed : null;
  }
}
