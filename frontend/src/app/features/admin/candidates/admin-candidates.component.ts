import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { CandidateResponse } from '../../../core/models/candidate.models';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { CandidatesService } from '../../../core/services/candidates.service';

@Component({
  selector: 'app-admin-candidates',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './admin-candidates.component.html',
  styleUrl: './admin-candidates.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminCandidatesComponent implements OnInit {
  private readonly candidatesService = inject(CandidatesService);
  private readonly apiError = inject(ApiErrorService);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly candidates = signal<CandidateResponse[]>([]);
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
    cvUrl: [''],
  });

  ngOnInit(): void {
    this.loadCandidates();
  }

  loadCandidates(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.actionError.set(null);

    this.candidatesService
      .getAll(this.pageNumber(), this.pageSize)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (result) => {
          this.candidates.set(result.items);
          this.totalPages.set(result.totalPages);
          this.totalCount.set(result.totalCount);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.apiError.getMessage(error, 'Failed to load candidate profiles.'));
        },
      });
  }

  startEdit(candidate: CandidateResponse): void {
    this.editingId.set(candidate.id);
    this.confirmDeleteId.set(null);
    this.actionError.set(null);
    this.editForm.setValue({ name: candidate.name, cvUrl: candidate.cvUrl });
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.editForm.reset();
  }

  saveCandidate(candidateId: number): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.pendingId.set(candidateId);
    this.actionError.set(null);
    this.candidatesService
      .update(candidateId, this.editForm.getRawValue())
      .pipe(finalize(() => this.pendingId.set(null)))
      .subscribe({
        next: (updated) => {
          this.candidates.update((items) =>
            items.map((candidate) => candidate.id === updated.id ? updated : candidate),
          );
          this.cancelEdit();
        },
        error: (error: unknown) => {
          this.actionError.set(this.apiError.getMessage(error, 'Failed to update the candidate profile.'));
        },
      });
  }

  requestDelete(candidateId: number): void {
    this.editingId.set(null);
    this.confirmDeleteId.set(candidateId);
    this.actionError.set(null);
  }

  deleteCandidate(candidateId: number): void {
    this.pendingId.set(candidateId);
    this.actionError.set(null);
    this.candidatesService
      .delete(candidateId)
      .pipe(finalize(() => this.pendingId.set(null)))
      .subscribe({
        next: () => {
          this.confirmDeleteId.set(null);
          if (this.candidates().length === 1 && this.pageNumber() > 1) {
            this.pageNumber.update((page) => page - 1);
          }
          this.loadCandidates();
        },
        error: (error: unknown) => {
          this.actionError.set(this.apiError.getMessage(error, 'Failed to delete the candidate profile.'));
        },
      });
  }

  previousPage(): void {
    if (this.pageNumber() > 1) {
      this.pageNumber.update((page) => page - 1);
      this.loadCandidates();
    }
  }

  nextPage(): void {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update((page) => page + 1);
      this.loadCandidates();
    }
  }
}
