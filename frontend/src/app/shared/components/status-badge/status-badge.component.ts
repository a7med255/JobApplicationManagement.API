import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { JobApplicationStatus, JOB_APPLICATION_STATUS_LABELS } from '../../../core/models/application.models';
import { JobStatus, JOB_STATUS_LABELS } from '../../../core/models/job.models';

export type StatusInput = JobApplicationStatus | JobStatus | string | number | boolean | null | undefined;

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `<span [class]="'badge ' + cssClass()">{{ label() }}</span>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusBadgeComponent {
  readonly status = input.required<StatusInput>();
  readonly customLabel = input<string | undefined>(undefined);

  protected readonly label = computed(() => {
    if (this.customLabel()) return this.customLabel()!;
    const val = this.status();
    if (val === null || val === undefined) return 'Unknown';

    if (typeof val === 'number') {
      if (val in JOB_APPLICATION_STATUS_LABELS) {
        return JOB_APPLICATION_STATUS_LABELS[val as JobApplicationStatus];
      }
      if (val in JOB_STATUS_LABELS) {
        return JOB_STATUS_LABELS[val as JobStatus];
      }
    }

    return String(val);
  });

  protected readonly cssClass = computed(() => {
    const val = this.status();
    if (val === null || val === undefined) return '';

    // Application Statuses
    if (val === JobApplicationStatus.Applied || val === 'Applied') return 'badge--applied';
    if (val === JobApplicationStatus.UnderReview || val === 'UnderReview' || val === 'Under Review') return 'badge--under-review';
    if (val === JobApplicationStatus.Interview || val === 'Interview') return 'badge--interview';
    if (val === JobApplicationStatus.Shortlisted || val === 'Shortlisted') return 'badge--shortlisted';
    if (val === JobApplicationStatus.Accepted || val === 'Accepted') return 'badge--accepted';
    if (val === JobApplicationStatus.Rejected || val === 'Rejected') return 'badge--rejected';
    if (val === JobApplicationStatus.Cancelled || val === 'Cancelled') return 'badge--cancelled';

    // Job Statuses
    if (val === JobStatus.Published || val === 'Published' || val === 'Active' || val === true) return 'badge--active';
    if (val === JobStatus.Closed || val === 'Closed' || val === false) return 'badge--closed';
    if (val === JobStatus.Draft || val === 'Draft') return 'badge--draft';
    if (val === JobStatus.Expired || val === 'Expired') return 'badge--cancelled';
    if (val === JobStatus.Archived || val === 'Archived') return 'badge--closed';

    return 'badge--applied';
  });
}
