import { Component, Input, OnInit, inject, signal, ChangeDetectionStrategy, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { Interview, InterviewStatus, InterviewType, JobApplicationResponse, JobApplicationStatus } from '../../../core/models/application.models';
import { ApplicationsService } from '../../../core/services/applications.service';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { AuthService } from '../../../core/auth/auth.service';
import { InterviewsService } from '../../../core/services/interviews.service';

@Component({
  selector: 'app-recruiter-application-review',
  standalone: true,
  imports: [DatePipe, RouterLink, ReactiveFormsModule, StatusBadgeComponent],
  templateUrl: './recruiter-application-review.component.html',
  styleUrl: './recruiter-application-review.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecruiterApplicationReviewComponent implements OnInit {
  @Input() id!: string;

  private readonly applicationsService = inject(ApplicationsService);
  private readonly apiError = inject(ApiErrorService);
  private readonly authService = inject(AuthService);
  private readonly interviewsService = inject(InterviewsService);
  private readonly fb = inject(FormBuilder);

  protected readonly application = signal<JobApplicationResponse | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly isUpdating = signal(false);
  protected readonly updateError = signal<string | null>(null);
  protected readonly updateSuccess = signal(false);
  protected readonly isDownloadingCv = signal(false);
  protected readonly interviews = signal<Interview[]>([]);
  protected readonly interviewBusy = signal(false);
  protected readonly editingInterviewId = signal<number | null>(null);
  protected readonly cancellingInterviewId = signal<number | null>(null);
  protected readonly applicationStatus = JobApplicationStatus;
  protected readonly interviewStatus = InterviewStatus;
  protected readonly interviewTypes = [
    { value: InterviewType.Online, label: 'Online' },
    { value: InterviewType.OnSite, label: 'On-site' },
    { value: InterviewType.Phone, label: 'Phone' },
  ];
  protected readonly backRoute = computed(() =>
    this.authService.isAdmin() ? '/admin/applications' : '/recruiter/applications',
  );

  // Status mapping for the dropdown
  protected readonly statusOptions: ReadonlyArray<{ value: JobApplicationStatus; label: string }> = [
    { value: JobApplicationStatus.Applied, label: 'Applied' },
    { value: JobApplicationStatus.UnderReview, label: 'Under Review' },
    { value: JobApplicationStatus.Shortlisted, label: 'Shortlisted' },
    { value: JobApplicationStatus.Interview, label: 'Interview' },
    { value: JobApplicationStatus.Accepted, label: 'Accepted' },
    { value: JobApplicationStatus.Rejected, label: 'Rejected' },
  ];
  
  protected readonly statusControl = new FormControl(JobApplicationStatus.Applied, { nonNullable: true });
  protected readonly statusNoteControl = new FormControl('', { nonNullable: true });
  protected readonly interviewForm = this.fb.nonNullable.group({
    scheduledAt: ['', Validators.required],
    durationMinutes: [60, [Validators.required, Validators.min(15), Validators.max(480)]],
    interviewType: [InterviewType.Online, Validators.required],
    meetingLink: [''],
    location: [''],
    notes: ['', Validators.maxLength(2000)],
  });

  ngOnInit(): void {
    if (!this.getApplicationId()) {
      this.isLoading.set(false);
      this.errorMessage.set('The application identifier is invalid.');
      return;
    }

    this.loadApplication();
  }

  loadApplication(): void {
    const applicationId = this.getApplicationId();
    if (!applicationId) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.applicationsService
      .getById(applicationId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (application) => {
          this.application.set(application);
          this.statusControl.setValue(application.jobApplicationStatus);
          if (application.jobApplicationStatus === JobApplicationStatus.Cancelled) {
            this.statusControl.disable();
          } else {
            this.statusControl.enable();
          }
          this.loadInterviews(application.id);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.apiError.getMessage(error, 'Application not found.'));
        },
      });
  }

  updateStatus(): void {
    const applicationId = this.getApplicationId();
    if (!this.application() || !applicationId) {
      return;
    }
    
    this.isUpdating.set(true);
    this.updateError.set(null);
    this.updateSuccess.set(false);

    this.applicationsService
      .updateStatus(applicationId, { jobApplicationStatus: this.statusControl.getRawValue(), note: this.statusNoteControl.value.trim() || null })
      .pipe(finalize(() => this.isUpdating.set(false)))
      .subscribe({
        next: (application) => {
          this.application.set(application);
          this.statusControl.setValue(application.jobApplicationStatus);
          this.statusNoteControl.setValue('');
          this.updateSuccess.set(true);
        },
        error: (error: unknown) => {
          this.updateError.set(this.apiError.getMessage(error, 'Failed to update application status.'));
        },
      });
  }

  saveInterview(): void {
    const application = this.application();
    if (!application || this.interviewForm.invalid) { this.interviewForm.markAllAsTouched(); return; }
    const value = this.interviewForm.getRawValue();
    const scheduledAt = new Date(value.scheduledAt);
    if (Number.isNaN(scheduledAt.getTime()) || scheduledAt <= new Date()) {
      this.updateError.set('Choose an interview date and time in the future.');
      return;
    }
    const request = {
      scheduledAt: scheduledAt.toISOString(), durationMinutes: value.durationMinutes,
      interviewType: value.interviewType, meetingLink: value.meetingLink.trim() || null,
      location: value.location.trim() || null, notes: value.notes.trim() || null,
    };
    this.interviewBusy.set(true); this.updateError.set(null);
    const editId = this.editingInterviewId();
    const operation = editId
      ? this.interviewsService.reschedule(editId, request)
      : this.interviewsService.schedule({ applicationId: application.id, ...request });
    operation.pipe(finalize(() => this.interviewBusy.set(false))).subscribe({
      next: () => { this.cancelInterviewEdit(); this.loadApplication(); },
      error: (error: unknown) => this.updateError.set(this.apiError.getMessage(error, 'The interview could not be saved.')),
    });
  }

  editInterview(interview: Interview): void {
    this.editingInterviewId.set(interview.id);
    const local = new Date(interview.scheduledAt);
    local.setMinutes(local.getMinutes() - local.getTimezoneOffset());
    this.interviewForm.setValue({
      scheduledAt: local.toISOString().slice(0, 16), durationMinutes: interview.durationMinutes,
      interviewType: interview.interviewType, meetingLink: interview.meetingLink ?? '',
      location: interview.location ?? '', notes: interview.notes ?? '',
    });
  }

  cancelInterviewEdit(): void {
    this.editingInterviewId.set(null);
    this.interviewForm.reset({ scheduledAt: '', durationMinutes: 60, interviewType: InterviewType.Online, meetingLink: '', location: '', notes: '' });
  }

  requestInterviewCancellation(interviewId: number): void {
    this.cancellingInterviewId.set(interviewId);
  }

  dismissInterviewCancellation(): void {
    this.cancellingInterviewId.set(null);
  }

  confirmInterviewCancellation(interview: Interview): void {
    this.interviewBusy.set(true);
    this.interviewsService.cancel(interview.id).pipe(finalize(() => this.interviewBusy.set(false))).subscribe({
      next: () => {
        this.cancellingInterviewId.set(null);
        this.loadInterviews(interview.applicationId);
      },
      error: (error: unknown) => this.updateError.set(this.apiError.getMessage(error, 'The interview could not be cancelled.')),
    });
  }

  downloadSubmittedCv(): void {
    const application = this.application();
    if (!application?.submittedCvId) return;
    this.isDownloadingCv.set(true);
    this.applicationsService.downloadSubmittedCv(application.id)
      .pipe(finalize(() => this.isDownloadingCv.set(false)))
      .subscribe({
        next: (blob) => {
          const url = URL.createObjectURL(blob);
          window.open(url, '_blank', 'noopener,noreferrer');
          window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
        },
        error: (error: unknown) => this.updateError.set(this.apiError.getMessage(error, 'The submitted CV could not be downloaded.')),
      });
  }

  private getApplicationId(): number | null {
    const parsedId = Number(this.id);
    return Number.isInteger(parsedId) && parsedId > 0 ? parsedId : null;
  }

  private loadInterviews(applicationId: number): void {
    this.interviewsService.getForApplication(applicationId).subscribe({
      next: (values) => this.interviews.set(values),
      error: (error: unknown) => this.updateError.set(this.apiError.getMessage(error, 'Interviews could not be loaded.')),
    });
  }
}
