import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import {
  JOB_APPLICATION_STATUS_LABELS,
  JobApplicationStatus,
} from '../../../core/models/application.models';

const STATUS_CLASSES: Record<JobApplicationStatus, string> = {
  [JobApplicationStatus.Applied]: 'badge--applied',
  [JobApplicationStatus.UnderReview]: 'badge--under-review',
  [JobApplicationStatus.Cancelled]: 'badge--cancelled',
  [JobApplicationStatus.InterView]: 'badge--interview',
  [JobApplicationStatus.Accepted]: 'badge--accepted',
  [JobApplicationStatus.Rejected]: 'badge--rejected',
};

@Component({
  selector: 'app-application-status-badge',
  template: `<span [class]="'badge ' + cssClass()">{{ label() }}</span>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApplicationStatusBadgeComponent {
  readonly status = input.required<JobApplicationStatus>();

  protected readonly label = computed(
    () => JOB_APPLICATION_STATUS_LABELS[this.status()] ?? 'Unknown',
  );
  protected readonly cssClass = computed(() => STATUS_CLASSES[this.status()] ?? '');
}
