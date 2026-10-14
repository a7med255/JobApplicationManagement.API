import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { RecruiterResponse } from '../../../core/models/recruiter.models';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { RecruitersService } from '../../../core/services/recruiters.service';

@Component({
  selector: 'app-admin-recruiters',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './admin-recruiters.component.html',
  styleUrl: './admin-recruiters.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminRecruitersComponent implements OnInit {
  private readonly recruitersService = inject(RecruitersService);
  private readonly apiError = inject(ApiErrorService);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly recruiters = signal<RecruiterResponse[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly actionError = signal<string | null>(null);
  protected readonly pageNumber = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly totalCount = signal(0);
  protected readonly pendingId = signal<number | null>(null);
  protected readonly editingId = signal<number | null>(null);
  protected readonly confirmDeleteId = signal<number | null>(null);
  protected readonly pageSize = 15;

  protected readonly editForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
  });

  ngOnInit(): void {
    this.loadRecruiters();
  }

  loadRecruiters(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.actionError.set(null);

    this.recruitersService
      .getAll(this.pageNumber(), this.pageSize)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (result) => {
          this.recruiters.set(result.items);
          this.totalPages.set(result.totalPages);
          this.totalCount.set(result.totalCount);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.apiError.getMessage(error, 'Failed to load recruiter profiles.'));
        },
      });
  }

  startEdit(recruiter: RecruiterResponse): void {
    this.editingId.set(recruiter.id);
    this.confirmDeleteId.set(null);
    this.actionError.set(null);
    this.editForm.setValue({ name: recruiter.name });
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.editForm.reset();
  }

  saveRecruiter(recruiterId: number): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.pendingId.set(recruiterId);
    this.actionError.set(null);
    this.recruitersService
      .update(recruiterId, this.editForm.getRawValue())
      .pipe(finalize(() => this.pendingId.set(null)))
      .subscribe({
        next: (updated) => {
          this.recruiters.update((items) =>
            items.map((recruiter) => recruiter.id === updated.id ? updated : recruiter),
          );
          this.cancelEdit();
        },
        error: (error: unknown) => {
          this.actionError.set(this.apiError.getMessage(error, 'Failed to update the recruiter profile.'));
        },
      });
  }

  requestDelete(recruiterId: number): void {
    this.editingId.set(null);
    this.confirmDeleteId.set(recruiterId);
    this.actionError.set(null);
  }

  deleteRecruiter(recruiterId: number): void {
    this.pendingId.set(recruiterId);
    this.actionError.set(null);
    this.recruitersService
      .delete(recruiterId)
      .pipe(finalize(() => this.pendingId.set(null)))
      .subscribe({
        next: () => {
          this.confirmDeleteId.set(null);
          if (this.recruiters().length === 1 && this.pageNumber() > 1) {
            this.pageNumber.update((page) => page - 1);
          }
          this.loadRecruiters();
        },
        error: (error: unknown) => {
          this.actionError.set(this.apiError.getMessage(error, 'Failed to delete the recruiter profile.'));
        },
      });
  }

  previousPage(): void {
    if (this.pageNumber() > 1) {
      this.pageNumber.update((page) => page - 1);
      this.loadRecruiters();
    }
  }

  nextPage(): void {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update((page) => page + 1);
      this.loadRecruiters();
    }
  }
}
