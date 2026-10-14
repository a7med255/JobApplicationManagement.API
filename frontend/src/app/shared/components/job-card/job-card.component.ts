import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  CompanySummary,
  EMPLOYMENT_TYPE_LABELS,
  EmploymentType,
  EXPERIENCE_LEVEL_LABELS,
  ExperienceLevel,
  WORKPLACE_TYPE_LABELS,
  WorkplaceType,
} from '../../../core/models/job.models';

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
  requiredSkills?: string[];
  postedAt?: string | null;
}

@Component({
  selector: 'app-job-card',
  standalone: true,
  imports: [RouterLink],
  template: `
    <article class="card job-card card--hoverable">
      <div class="job-card__header">
        <div>
          <h3 class="job-card__title">
            <a [routerLink]="['/jobs', job.id]" class="job-card__link">{{ job.title }}</a>
          </h3>
          @if (job.company?.name) {
            <p class="job-card__company">{{ job.company.name }}</p>
          }
        </div>
        <span class="badge" [class.badge--active]="job.isActive" [class.badge--closed]="!job.isActive">
          {{ job.isActive ? 'Open' : 'Closed' }}
        </span>
      </div>

      <div class="job-card__body">
        @if (job.city || job.country) {
          <p class="job-card__meta">{{ job.city }}{{ job.city && job.country ? ', ' : '' }}{{ job.country }}</p>
        }
        <div class="job-card__tags">
          @if (job.employmentType !== undefined) { <span>{{ employmentLabels[job.employmentType] }}</span> }
          @if (job.workplaceType !== undefined) { <span>{{ workplaceLabels[job.workplaceType] }}</span> }
          @if (job.experienceLevel !== undefined) { <span>{{ experienceLabels[job.experienceLevel] }}</span> }
        </div>
        @if (job.requiredSkills?.length) {
          <div class="job-card__skills">
            @for (skill of job.requiredSkills.slice(0, 4); track skill) { <span>{{ skill }}</span> }
          </div>
        } @else if (job.description) {
          <p class="job-card__description">{{ job.description }}</p>
        }
      </div>

      <div class="job-card__footer">
        <a [routerLink]="['/jobs', job.id]" class="btn btn--secondary btn--sm">View Details</a>
        @if (showApply && job.isActive) {
          <button class="btn btn--primary btn--sm" type="button" (click)="onApplyClick($event)">View & Apply</button>
        }
        @if (showSave && job.isActive) {
          <button class="btn btn--secondary btn--sm" type="button" (click)="onSaveClick($event)" [disabled]="isSavePending">
            {{ isSavePending ? 'Updating...' : (isSaved ? 'Unsave' : 'Save') }}
          </button>
        }
      </div>
    </article>
  `,
  styles: `
    .job-card { display:flex; flex-direction:column; height:100%; }
    .job-card__header { display:flex; justify-content:space-between; align-items:flex-start; gap:1rem; }
    .job-card__title { margin:0; font-size:1.125rem; }
    .job-card__link { color:var(--text-primary); text-decoration:none; }
    .job-card__link:hover { color:var(--color-primary); }
    .job-card__company,.job-card__meta,.job-card__description { color:var(--text-secondary); margin:.4rem 0 0; }
    .job-card__description { display:-webkit-box; -webkit-line-clamp:3; -webkit-box-orient:vertical; overflow:hidden; }
    .job-card__body { flex:1; padding:1rem 0; }
    .job-card__tags,.job-card__skills { display:flex; flex-wrap:wrap; gap:.4rem; margin-top:.75rem; }
    .job-card__tags span,.job-card__skills span { background:var(--background-secondary); border-radius:999px; padding:.25rem .6rem; font-size:.75rem; }
    .job-card__footer { display:flex; flex-wrap:wrap; gap:.5rem; border-top:1px solid var(--border-color); padding-top:1rem; }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobCardComponent {
  protected readonly employmentLabels = EMPLOYMENT_TYPE_LABELS;
  protected readonly workplaceLabels = WORKPLACE_TYPE_LABELS;
  protected readonly experienceLabels = EXPERIENCE_LEVEL_LABELS;

  @Input({ required: true }) job!: JobCardData;
  @Input() showApply = false;
  @Input() showSave = false;
  @Input() isSaved = false;
  @Input() isSavePending = false;

  @Output() apply = new EventEmitter<number>();
  @Output() saveToggle = new EventEmitter<number>();

  onApplyClick(event: Event): void {
    event.preventDefault();
    this.apply.emit(this.job.id);
  }

  onSaveClick(event: Event): void {
    event.preventDefault();
    this.saveToggle.emit(this.job.id);
  }
}
