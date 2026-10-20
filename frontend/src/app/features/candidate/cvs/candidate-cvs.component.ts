import { ChangeDetectionStrategy, Component, HostListener, inject, OnInit, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { CandidateCv } from '../../../core/models/candidate-cv.models';
import { CandidateCvsService } from '../../../core/services/candidate-cvs.service';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { APP_CONFIG } from '../../../core/config/app.constants';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-candidate-cvs',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, DatePipe, DecimalPipe, EmptyStateComponent],
  templateUrl: './candidate-cvs.component.html',
  styleUrl: './candidate-cvs.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateCvsComponent implements OnInit {
  private readonly cvsService = inject(CandidateCvsService);
  private readonly apiError = inject(ApiErrorService);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly cvs = signal<CandidateCv[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly isUploading = signal(false);
  protected readonly isRenaming = signal(false);
  protected readonly isDragOver = signal(false);
  protected readonly pendingId = signal<number | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly renamingCv = signal<CandidateCv | null>(null);
  protected readonly deletingCv = signal<CandidateCv | null>(null);
  protected readonly maximumCvMegabytes = APP_CONFIG.maximumCvBytes / 1024 / 1024;

  protected readonly uploadForm = this.formBuilder.nonNullable.group({
    displayName: ['', [Validators.required, Validators.maxLength(200)]],
    setAsDefault: [false],
  });

  protected readonly renameForm = this.formBuilder.nonNullable.group({
    displayName: ['', [Validators.required, Validators.maxLength(200)]],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.cvsService.getMine().pipe(finalize(() => this.isLoading.set(false))).subscribe({
      next: (cvs) => this.cvs.set(cvs),
      error: (error: unknown) => this.errorMessage.set(this.apiError.getMessage(error, 'CVs could not be loaded.')),
    });
  }

  async chooseFile(event: Event): Promise<void> {
    const file = (event.target as HTMLInputElement).files?.[0] ?? null;
    await this.processSelectedFile(file);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(false);
  }

  async onDrop(event: DragEvent): Promise<void> {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(false);
    const file = event.dataTransfer?.files?.[0] ?? null;
    await this.processSelectedFile(file);
  }

  clearSelectedFile(): void {
    this.selectedFile.set(null);
  }

  private async processSelectedFile(file: File | null): Promise<void> {
    this.errorMessage.set(null);
    if (!file) {
      this.selectedFile.set(null);
      return;
    }
    if (!file.name.toLowerCase().endsWith('.pdf') || !['application/pdf', 'application/octet-stream'].includes(file.type || 'application/octet-stream')) {
      this.selectedFile.set(null);
      this.errorMessage.set('Choose a genuine PDF file. Other document and image types are not supported.');
      return;
    }
    if (file.size === 0 || file.size > APP_CONFIG.maximumCvBytes) {
      this.selectedFile.set(null);
      this.errorMessage.set(`PDF files must be non-empty and no larger than ${this.maximumCvMegabytes} MB.`);
      return;
    }
    try {
      const signature = new TextDecoder().decode(await file.slice(0, 5).arrayBuffer());
      if (signature !== '%PDF-') {
        this.selectedFile.set(null);
        this.errorMessage.set('The selected file does not contain a valid PDF signature.');
        return;
      }
    } catch {
      this.selectedFile.set(null);
      this.errorMessage.set('Could not verify PDF file format.');
      return;
    }

    this.selectedFile.set(file);
    if (!this.uploadForm.controls.displayName.value) {
      this.uploadForm.controls.displayName.setValue(file.name.replace(/\.[^.]+$/, ''));
    }
  }

  upload(): void {
    const file = this.selectedFile();
    if (this.uploadForm.invalid || !file) {
      this.uploadForm.markAllAsTouched();
      this.errorMessage.set(file ? null : 'Please choose a PDF file.');
      return;
    }

    if (!file.name.toLowerCase().endsWith('.pdf') || !['application/pdf', 'application/octet-stream'].includes(file.type || 'application/octet-stream')) {
      this.errorMessage.set('Only PDF files are supported.');
      return;
    }

    if (file.size > APP_CONFIG.maximumCvBytes) {
      this.errorMessage.set(`The selected file is larger than ${this.maximumCvMegabytes} MB.`);
      return;
    }

    this.isUploading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    const values = this.uploadForm.getRawValue();
    this.cvsService.upload(values.displayName, file, values.setAsDefault)
      .pipe(finalize(() => this.isUploading.set(false)))
      .subscribe({
        next: () => {
          this.uploadForm.reset({ displayName: '', setAsDefault: false });
          this.selectedFile.set(null);
          this.successMessage.set('CV uploaded successfully.');
          this.load();
        },
        error: (error: unknown) => this.errorMessage.set(this.apiError.getMessage(error, 'CV upload failed.')),
      });
  }

  startRename(cv: CandidateCv): void {
    this.renamingCv.set(cv);
    this.renameForm.setValue({ displayName: cv.displayName });
  }

  cancelRename(): void {
    this.renamingCv.set(null);
    this.renameForm.reset();
  }

  saveRename(): void {
    const cv = this.renamingCv();
    if (!cv || this.renameForm.invalid) {
      this.renameForm.markAllAsTouched();
      return;
    }

    const newName = this.renameForm.controls.displayName.value.trim();
    this.isRenaming.set(true);
    this.errorMessage.set(null);

    this.cvsService.rename(cv.id, newName)
      .pipe(finalize(() => this.isRenaming.set(false)))
      .subscribe({
        next: () => {
          this.renamingCv.set(null);
          this.successMessage.set('CV renamed successfully.');
          this.load();
        },
        error: (error: unknown) => this.errorMessage.set(this.apiError.getMessage(error, 'Failed to rename CV.')),
      });
  }

  setDefault(id: number): void {
    this.runAction(id, this.cvsService.setDefault(id));
  }

  requestDelete(cv: CandidateCv): void {
    this.deletingCv.set(cv);
  }

  cancelDelete(): void {
    this.deletingCv.set(null);
  }

  confirmDelete(): void {
    const cv = this.deletingCv();
    if (!cv) return;
    this.deletingCv.set(null);
    this.runAction(cv.id, this.cvsService.delete(cv.id));
  }

  @HostListener('document:keydown.escape')
  closeDialogOnEscape(): void {
    if (!this.isRenaming()) this.cancelRename();
    this.cancelDelete();
  }

  download(cv: CandidateCv): void {
    this.pendingId.set(cv.id);
    this.cvsService.download(cv.id).pipe(finalize(() => this.pendingId.set(null))).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = cv.originalFileName || `${cv.displayName}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
      },
      error: (error: unknown) => this.errorMessage.set(this.apiError.getMessage(error, 'CV download failed.')),
    });
  }

  preview(cv: CandidateCv): void {
    this.pendingId.set(cv.id);
    this.cvsService.download(cv.id).pipe(finalize(() => this.pendingId.set(null))).subscribe({
      next: (blob) => {
        const fileBlob = new Blob([blob], { type: 'application/pdf' });
        const url = URL.createObjectURL(fileBlob);
        window.open(url, '_blank', 'noopener,noreferrer');
        window.setTimeout(() => URL.revokeObjectURL(url), 120_000);
      },
      error: (error: unknown) => this.errorMessage.set(this.apiError.getMessage(error, 'Failed to preview CV.')),
    });
  }

  private runAction(id: number, request: import('rxjs').Observable<unknown>): void {
    this.pendingId.set(id);
    this.errorMessage.set(null);
    request.pipe(finalize(() => this.pendingId.set(null))).subscribe({
      next: () => this.load(),
      error: (error: unknown) => this.errorMessage.set(this.apiError.getMessage(error, 'The CV could not be updated.')),
    });
  }
}
