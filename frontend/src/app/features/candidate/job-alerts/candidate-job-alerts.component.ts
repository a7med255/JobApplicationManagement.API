import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { JobAlert } from '../../../core/models/job-alert.models';
import { EMPLOYMENT_TYPE_LABELS, EmploymentType } from '../../../core/models/job.models';
import { JobAlertsService } from '../../../core/services/job-alerts.service';
import { ApiErrorService } from '../../../core/services/api-error.service';

@Component({
  selector: 'app-candidate-job-alerts', standalone: true, imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './candidate-job-alerts.component.html', styleUrl: './candidate-job-alerts.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateJobAlertsComponent implements OnInit {
  private readonly service = inject(JobAlertsService);
  private readonly fb = inject(FormBuilder);
  private readonly apiError = inject(ApiErrorService);
  protected readonly alerts = signal<JobAlert[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly deletingAlertId = signal<number | null>(null);
  protected readonly pendingAlertId = signal<number | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly employmentOptions = Object.entries(EMPLOYMENT_TYPE_LABELS).map(([value, label]) => ({ value: Number(value) as EmploymentType, label }));
  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]], keyword: [''], location: [''], employmentType: [''], isActive: [true],
  });
  ngOnInit(): void { this.load(); }
  load(): void { this.loading.set(true); this.service.getMine().pipe(finalize(() => this.loading.set(false))).subscribe({ next: (x) => this.alerts.set(x), error: (e: unknown) => this.error.set(this.apiError.getMessage(e, 'Job alerts could not be loaded.')) }); }
  create(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const value = this.form.getRawValue();
    this.saving.set(true); this.error.set(null);
    this.service.create({ name: value.name.trim(), keyword: value.keyword.trim() || null, location: value.location.trim() || null, employmentType: value.employmentType === '' ? null : Number(value.employmentType), isActive: value.isActive })
      .pipe(finalize(() => this.saving.set(false))).subscribe({ next: () => { this.form.reset({ name: '', keyword: '', location: '', employmentType: '', isActive: true }); this.load(); }, error: (e: unknown) => this.error.set(this.apiError.getMessage(e, 'Job alert could not be created.')) });
  }
  toggle(alert: JobAlert): void {
    this.pendingAlertId.set(alert.id);
    this.service.update(alert.id, { name: alert.name, keyword: alert.keyword, location: alert.location, employmentType: alert.employmentType, isActive: !alert.isActive })
      .pipe(finalize(() => this.pendingAlertId.set(null)))
      .subscribe({ next: () => this.load(), error: (e: unknown) => this.error.set(this.apiError.getMessage(e, 'Job alert could not be updated.')) });
  }
  requestDelete(id: number): void { this.deletingAlertId.set(id); }
  cancelDelete(): void { this.deletingAlertId.set(null); }
  remove(id: number): void {
    this.pendingAlertId.set(id);
    this.service.delete(id).pipe(finalize(() => this.pendingAlertId.set(null))).subscribe({
      next: () => { this.deletingAlertId.set(null); this.load(); },
      error: (e: unknown) => this.error.set(this.apiError.getMessage(e, 'Job alert could not be deleted.')),
    });
  }
}
