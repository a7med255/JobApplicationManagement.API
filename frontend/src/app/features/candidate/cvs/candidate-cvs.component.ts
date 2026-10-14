import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { CandidateCv } from '../../../core/models/candidate-cv.models';
import { CandidateCvsService } from '../../../core/services/candidate-cvs.service';
import { ApiErrorService } from '../../../core/services/api-error.service';

@Component({
  selector: 'app-candidate-cvs',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, DatePipe, DecimalPipe],
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
  protected readonly pendingId = signal<number | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly uploadForm = this.formBuilder.nonNullable.group({
    displayName: ['', [Validators.required, Validators.maxLength(200)]],
    setAsDefault: [false],
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

  chooseFile(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0] ?? null;
    this.selectedFile.set(file);
    if (file && !this.uploadForm.controls.displayName.value) {
      this.uploadForm.controls.displayName.setValue(file.name.replace(/\.[^.]+$/, ''));
    }
  }

  upload(): void {
    const file = this.selectedFile();
    if (this.uploadForm.invalid || !file) {
      this.uploadForm.markAllAsTouched();
      this.errorMessage.set(file ? null : 'Choose a PDF, DOC or DOCX file.');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.errorMessage.set('The selected file is larger than 5 MB.');
      return;
    }

    this.isUploading.set(true);
    this.errorMessage.set(null);
    const values = this.uploadForm.getRawValue();
    this.cvsService.upload(values.displayName, file, values.setAsDefault)
      .pipe(finalize(() => this.isUploading.set(false)))
      .subscribe({
        next: () => {
          this.uploadForm.reset({ displayName: '', setAsDefault: false });
          this.selectedFile.set(null);
          this.load();
        },
        error: (error: unknown) => this.errorMessage.set(this.apiError.getMessage(error, 'CV upload failed.')),
      });
  }

  setDefault(id: number): void {
    this.runAction(id, this.cvsService.setDefault(id));
  }

  delete(id: number): void {
    this.runAction(id, this.cvsService.delete(id));
  }

  download(cv: CandidateCv): void {
    this.pendingId.set(cv.id);
    this.cvsService.download(cv.id).pipe(finalize(() => this.pendingId.set(null))).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank', 'noopener,noreferrer');
        window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
      },
      error: (error: unknown) => this.errorMessage.set(this.apiError.getMessage(error, 'CV download failed.')),
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
