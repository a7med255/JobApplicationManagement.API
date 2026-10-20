import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  CompanySummary,
  EMPLOYMENT_TYPE_LABELS,
  EmploymentType,
  EXPERIENCE_LEVEL_LABELS,
  ExperienceLevel,
  SALARY_PERIOD_LABELS,
  SalaryPeriod,
  WORKPLACE_TYPE_LABELS,
  WorkplaceType,
} from '../../../core/models/job.models';
import { CompanyLogoComponent } from '../company-logo/company-logo.component';

export interface JobCardData {
  id: number;
  title: string;
  isActive: boolean;
  description?: string;
  company?: CompanySummary;
  city?: string;
  country?: string;
  workplaceType?: WorkplaceType;
  employmentType?: EmploymentType;
  experienceLevel?: ExperienceLevel;
  minimumYearsExperience?: number;
  maximumYearsExperience?: number | null;
  minimumSalary?: number | null;
  maximumSalary?: number | null;
  currency?: string | null;
  salaryPeriod?: SalaryPeriod | null;
  isSalaryVisible?: boolean;
  requiredSkills?: string[];
  postedAt?: string | Date | null;
}

@Component({
  selector: 'app-job-card',
  standalone: true,
  imports: [RouterLink, CompanyLogoComponent],
  template: `
    <article class="job-card" [class.job-card--closed]="!job.isActive">
      <div class="job-card__header">
        <app-company-logo
          [name]="job.company?.name"
          [logoUrl]="job.company?.logoUrl"
          size="md"
        />

        <div class="job-card__header-content">
          <div class="job-card__title-row">
            <h3 class="job-card__title">
              <a [routerLink]="['/jobs', job.id]" class="job-card__link">
                {{ job.title }}
              </a>
            </h3>

            @if (showSave) {
              <button
                type="button"
                class="job-card__save-btn"
                [class.job-card__save-btn--saved]="isSaved"
                [attr.aria-label]="isSaved ? 'Remove from saved jobs' : 'Save job'"
                [title]="isSaved ? 'Remove from saved jobs' : 'Save job'"
                (click)="onSaveClick($event)"
                [disabled]="isSavePending"
              >
                <svg
                  width="20"
                  height="20"
                  viewBox="0 0 24 24"
                  [attr.fill]="isSaved ? 'currentColor' : 'none'"
                  stroke="currentColor"
                  stroke-width="2"
                  stroke-linecap="round"
                  stroke-linejoin="round"
                >
                  <path d="M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z"></path>
                </svg>
              </button>
            }
          </div>

          @if (job.company?.name) {
            <p class="job-card__company">{{ job.company.name }}</p>
          }
        </div>
      </div>

      <div class="job-card__body">
        <!-- Metadata row: Location, Workplace, Employment -->
        <div class="job-card__meta-row">
          @if (locationText()) {
            <span class="job-card__meta-item">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path>
                <circle cx="12" cy="10" r="3"></circle>
              </svg>
              {{ locationText() }}
            </span>
          }

          @if (job.workplaceType !== undefined) {
            <span class="job-card__meta-item">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect x="4" y="2" width="16" height="20" rx="2" ry="2"></rect>
                <line x1="9" y1="22" x2="9" y2="22.01"></line>
                <line x1="15" y1="22" x2="15" y2="22.01"></line>
                <line x1="9" y1="6" x2="9" y2="6.01"></line>
                <line x1="15" y1="6" x2="15" y2="6.01"></line>
                <line x1="9" y1="10" x2="9" y2="10.01"></line>
                <line x1="15" y1="10" x2="15" y2="10.01"></line>
                <line x1="9" y1="14" x2="9" y2="14.01"></line>
                <line x1="15" y1="14" x2="15" y2="14.01"></line>
              </svg>
              {{ workplaceLabels[job.workplaceType] }}
            </span>
          }

          @if (job.employmentType !== undefined) {
            <span class="job-card__meta-item">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect x="2" y="7" width="20" height="14" rx="2" ry="2"></rect>
                <path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"></path>
              </svg>
              {{ employmentLabels[job.employmentType] }}
            </span>
          }

          @if (!job.isActive) {
            <span class="badge badge--closed">Closed</span>
          }
        </div>

        <!-- Experience row if available -->
        @if (experienceText()) {
          <div class="job-card__experience">
            <span class="job-card__exp-pill">{{ experienceText() }}</span>
            @if (salaryText()) {
              <span class="job-card__salary">{{ salaryText() }}</span>
            }
          </div>
        } @else if (salaryText()) {
          <p class="job-card__salary">{{ salaryText() }}</p>
        }

        <!-- Skills pills -->
        @if (job.requiredSkills && job.requiredSkills.length > 0) {
          <div class="job-card__skills">
            @for (skill of job.requiredSkills.slice(0, 4); track skill) {
              <span class="job-card__skill">{{ skill }}</span>
            }
            @if (job.requiredSkills.length > 4) {
              <span class="job-card__skill job-card__skill--more">+{{ job.requiredSkills.length - 4 }}</span>
            }
          </div>
        } @else if (job.description) {
          <p class="job-card__desc">{{ job.description }}</p>
        }
      </div>

      <div class="job-card__footer">
        <span class="job-card__posted-time">{{ relativeTime() }}</span>

        <div class="job-card__actions">
          <a [routerLink]="['/jobs', job.id]" class="btn btn--secondary btn--sm">
            View Job
          </a>
          @if (showApply && job.isActive) {
            <button
              class="btn btn--primary btn--sm"
              type="button"
              (click)="onApplyClick($event)"
            >
              Apply Now
            </button>
          }
        </div>
      </div>
    </article>
  `,
  styles: `
    .job-card {
      display: flex;
      flex-direction: column;
      background: #ffffff;
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      padding: 1.25rem 1.5rem;
      box-shadow: var(--shadow-sm);
      transition: border-color 0.15s ease, box-shadow 0.15s ease;
      height: 100%;

      &:hover {
        border-color: #cbd5e1;
        box-shadow: var(--shadow-md);
      }

      &--closed {
        opacity: 0.75;
        background: #f8fafc;
      }
    }

    .job-card__header {
      display: flex;
      align-items: flex-start;
      gap: 1rem;
    }

    .job-card__header-content {
      flex: 1;
      min-width: 0;
    }

    .job-card__title-row {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 0.5rem;
    }

    .job-card__title {
      margin: 0;
      font-size: 1.0625rem;
      font-weight: 700;
      line-height: 1.35;
    }

    .job-card__link {
      color: var(--text-primary);
      text-decoration: none;

      &:hover {
        color: var(--color-primary);
        text-decoration: underline;
      }
    }

    .job-card__save-btn {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 32px;
      height: 32px;
      background: transparent;
      border: 1px solid transparent;
      border-radius: var(--radius-md);
      color: var(--text-muted);
      cursor: pointer;
      padding: 0;
      flex-shrink: 0;
      transition: all 0.15s ease;

      &:hover {
        background: var(--color-primary-light);
        color: var(--color-primary);
      }

      &--saved {
        color: var(--color-primary);
      }
    }

    .job-card__company {
      margin: 0.25rem 0 0;
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-secondary);
    }

    .job-card__body {
      flex: 1;
      padding: 0.875rem 0 1rem;
      display: flex;
      flex-direction: column;
      gap: 0.625rem;
    }

    .job-card__meta-row {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 0.75rem;
      font-size: 0.8125rem;
      color: var(--text-secondary);
    }

    .job-card__meta-item {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;

      svg {
        color: var(--text-muted);
        flex-shrink: 0;
      }
    }

    .job-card__experience {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .job-card__exp-pill {
      font-size: 0.75rem;
      font-weight: 600;
      color: var(--color-primary);
      background: var(--color-primary-light);
      padding: 0.2rem 0.55rem;
      border-radius: var(--radius-pill);
    }

    .job-card__salary {
      margin: 0;
      font-size: 0.8125rem;
      font-weight: 600;
      color: var(--text-secondary);
    }

    .job-card__skills {
      display: flex;
      flex-wrap: wrap;
      gap: 0.375rem;
      margin-top: 0.25rem;
    }

    .job-card__skill {
      font-size: 0.75rem;
      color: var(--text-secondary);
      background: #f1f5f9;
      border: 1px solid #e2e8f0;
      padding: 0.2rem 0.55rem;
      border-radius: var(--radius-sm);

      &--more {
        background: transparent;
        border-style: dashed;
        color: var(--text-muted);
      }
    }

    .job-card__desc {
      font-size: 0.8125rem;
      color: var(--text-secondary);
      line-height: 1.5;
      margin: 0;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .job-card__footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1rem;
      padding-top: 0.875rem;
      border-top: 1px solid #f1f5f9;
      margin-top: auto;
    }

    .job-card__posted-time {
      font-size: 0.75rem;
      color: var(--text-muted);
    }

    .job-card__actions {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobCardComponent {
  protected readonly employmentLabels = EMPLOYMENT_TYPE_LABELS;
  protected readonly workplaceLabels = WORKPLACE_TYPE_LABELS;
  protected readonly experienceLabels = EXPERIENCE_LEVEL_LABELS;
  protected readonly salaryPeriodLabels = SALARY_PERIOD_LABELS;

  @Input({ required: true }) job!: JobCardData;
  @Input() showApply = false;
  @Input() showSave = false;
  @Input() isSaved = false;
  @Input() isSavePending = false;

  @Output() apply = new EventEmitter<number>();
  @Output() saveToggle = new EventEmitter<number>();

  protected locationText(): string {
    const parts = [this.job.city, this.job.country].filter(Boolean);
    return parts.join(', ');
  }

  protected experienceText(): string {
    const level = this.job.experienceLevel !== undefined ? this.experienceLabels[this.job.experienceLevel] : '';
    const min = this.job.minimumYearsExperience;
    const max = this.job.maximumYearsExperience;

    let years = '';
    if (min !== undefined && min !== null) {
      if (max !== undefined && max !== null) {
        years = `${min}–${max} yrs exp`;
      } else if (min > 0) {
        years = `${min}+ yrs exp`;
      }
    }

    if (level && years) return `${level} · ${years}`;
    return level || years || '';
  }

  protected relativeTime(): string {
    if (!this.job.postedAt) return 'Recently added';
    const date = new Date(this.job.postedAt);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffDays = Math.floor(diffHours / 24);

    if (diffHours < 1) return 'Posted just now';
    if (diffHours < 24) return `Posted ${diffHours}h ago`;
    if (diffDays === 1) return 'Posted 1 day ago';
    if (diffDays < 30) return `Posted ${diffDays} days ago`;
    return `Posted on ${date.toLocaleDateString()}`;
  }

  protected salaryText(): string {
    if (!this.job.isSalaryVisible) return '';

    const minimum = this.job.minimumSalary;
    const maximum = this.job.maximumSalary;
    if (minimum === null || minimum === undefined) return '';

    const formatter = new Intl.NumberFormat(undefined, { maximumFractionDigits: 0 });
    const amount = maximum !== null && maximum !== undefined
      ? `${formatter.format(minimum)}–${formatter.format(maximum)}`
      : `From ${formatter.format(minimum)}`;
    const currency = this.job.currency?.trim() ? ` ${this.job.currency.trim().toUpperCase()}` : '';
    const period = this.job.salaryPeriod !== null && this.job.salaryPeriod !== undefined
      ? ` / ${this.salaryPeriodLabels[this.job.salaryPeriod]}`
      : '';

    return `${amount}${currency}${period}`;
  }

  onApplyClick(event: Event): void {
    event.preventDefault();
    this.apply.emit(this.job.id);
  }

  onSaveClick(event: Event): void {
    event.preventDefault();
    this.saveToggle.emit(this.job.id);
  }
}
