import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { CompaniesService } from '../../../core/services/companies.service';
import { ApiErrorService } from '../../../core/services/api-error.service';

@Component({
  selector: 'app-recruiter-company',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './recruiter-company.component.html',
  styleUrl: './recruiter-company.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecruiterCompanyComponent implements OnInit {
  private readonly companiesService = inject(CompaniesService);
  private readonly apiError = inject(ApiErrorService);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly message = signal<string | null>(null);
  protected readonly isError = signal(false);
  protected readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    logoUrl: ['', Validators.maxLength(2000)],
    description: ['', Validators.maxLength(5000)],
    website: ['', Validators.maxLength(2000)],
    industry: ['', Validators.maxLength(150)],
    companySize: ['', Validators.maxLength(100)],
    headquarters: ['', Validators.maxLength(300)],
    country: ['', Validators.maxLength(100)],
    city: ['', Validators.maxLength(100)],
  });

  ngOnInit(): void {
    this.companiesService.getMine().pipe(finalize(() => this.isLoading.set(false))).subscribe({
      next: (company) => this.form.patchValue({
        name: company.name,
        logoUrl: company.logoUrl ?? '',
        description: company.description ?? '',
        website: company.website ?? '',
        industry: company.industry ?? '',
        companySize: company.companySize ?? '',
        headquarters: company.headquarters ?? '',
        country: company.country ?? '',
        city: company.city ?? '',
      }),
      error: (error: unknown) => {
        // A recruiter without a company receives 404 and can create it with this same form.
        const message = this.apiError.getMessage(error, 'Enter your company information to continue.');
        if (!message.toLowerCase().includes('not found')) {
          this.message.set(message);
          this.isError.set(true);
        }
      },
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.isSaving.set(true);
    this.message.set(null);
    this.companiesService.updateMine(this.form.getRawValue())
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => { this.isError.set(false); this.message.set('Company profile saved. You can now create job postings.'); },
        error: (error: unknown) => { this.isError.set(true); this.message.set(this.apiError.getMessage(error, 'Company profile could not be saved.')); },
      });
  }
}
