import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import {
  CreateJobRequest,
  EmploymentType,
  ExperienceLevel,
  JobWriteRequest,
  SalaryPeriod,
  WorkplaceType,
} from '../../../core/models/job.models';
import { JobsService } from '../../../core/services/jobs.service';
import { ApiErrorService } from '../../../core/services/api-error.service';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-recruiter-job-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './recruiter-job-form.component.html',
  styleUrl: './recruiter-job-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecruiterJobFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly jobsService = inject(JobsService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly apiError = inject(ApiErrorService);
  private readonly authService = inject(AuthService);

  protected readonly workplaceOptions = [
    [WorkplaceType.OnSite, 'On-site'], [WorkplaceType.Remote, 'Remote'], [WorkplaceType.Hybrid, 'Hybrid'],
  ] as const;
  protected readonly employmentOptions = [
    [EmploymentType.FullTime, 'Full-time'], [EmploymentType.PartTime, 'Part-time'],
    [EmploymentType.Contract, 'Contract'], [EmploymentType.Internship, 'Internship'],
    [EmploymentType.Freelance, 'Freelance'], [EmploymentType.Temporary, 'Temporary'],
  ] as const;
  protected readonly experienceOptions = [
    [ExperienceLevel.Internship, 'Internship'], [ExperienceLevel.EntryLevel, 'Entry level'],
    [ExperienceLevel.Junior, 'Junior'], [ExperienceLevel.MidLevel, 'Mid level'],
    [ExperienceLevel.Senior, 'Senior'], [ExperienceLevel.Lead, 'Lead'], [ExperienceLevel.Manager, 'Manager'],
  ] as const;
  protected readonly salaryPeriodOptions = [
    [SalaryPeriod.Hourly, 'Hourly'], [SalaryPeriod.Monthly, 'Monthly'], [SalaryPeriod.Yearly, 'Yearly'],
  ] as const;

  protected readonly jobForm = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.maxLength(20000)]],
    department: ['', [Validators.required, Validators.maxLength(150)]],
    category: ['', [Validators.required, Validators.maxLength(150)]],
    country: ['', [Validators.required, Validators.maxLength(100)]],
    city: ['', [Validators.required, Validators.maxLength(100)]],
    location: ['', Validators.maxLength(300)],
    workplaceType: [WorkplaceType.OnSite, Validators.required],
    employmentType: [EmploymentType.FullTime, Validators.required],
    experienceLevel: [ExperienceLevel.EntryLevel, Validators.required],
    minimumYearsExperience: [0, [Validators.required, Validators.min(0), Validators.max(80)]],
    maximumYearsExperience: this.fb.control<number | null>(null, [Validators.min(0), Validators.max(80)]),
    responsibilities: ['', [Validators.required, Validators.maxLength(10000)]],
    requirements: ['', [Validators.required, Validators.maxLength(10000)]],
    preferredQualifications: ['', Validators.maxLength(10000)],
    educationRequirements: ['', Validators.maxLength(5000)],
    benefits: ['', Validators.maxLength(10000)],
    minimumSalary: this.fb.control<number | null>(null, Validators.min(0)),
    maximumSalary: this.fb.control<number | null>(null, Validators.min(0)),
    currency: ['', Validators.maxLength(3)],
    salaryPeriod: this.fb.control<SalaryPeriod | null>(null),
    isSalaryVisible: [false],
    numberOfVacancies: [1, [Validators.required, Validators.min(1), Validators.max(1000)]],
    applicationDeadline: [''],
    requiredSkills: [''],
    preferredSkills: [''],
    publishImmediately: [true],
  });
  protected readonly isSubmitting = signal(false);
  protected readonly isLoading = signal(false);
  protected readonly isError = signal(false);
  protected readonly errorMsg = signal('');
  protected readonly backRoute = computed(() => this.authService.isAdmin() ? '/admin/jobs' : '/recruiter/dashboard');
  protected readonly backLabel = computed(() => this.authService.isAdmin() ? 'Back to admin jobs' : 'Back to dashboard');
  protected jobId: number | null = null;
  protected isEditMode = false;

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    const parsedId = idParam ? Number(idParam) : null;
    if (parsedId !== null && Number.isInteger(parsedId) && parsedId > 0) {
      this.jobId = parsedId;
      this.isEditMode = true;
      this.loadJob(parsedId);
    } else if (idParam !== null) {
      this.isError.set(true);
      this.errorMsg.set('The job identifier is invalid.');
    }
  }

  loadJob(id: number): void {
    this.isLoading.set(true);
    this.jobsService.getById(id).pipe(finalize(() => this.isLoading.set(false))).subscribe({
      next: (job) => this.jobForm.patchValue({
        ...job,
        location: job.location ?? '',
        maximumYearsExperience: job.maximumYearsExperience,
        preferredQualifications: job.preferredQualifications ?? '',
        educationRequirements: job.educationRequirements ?? '',
        benefits: job.benefits ?? '',
        minimumSalary: job.minimumSalary,
        maximumSalary: job.maximumSalary,
        currency: job.currency ?? '',
        salaryPeriod: job.salaryPeriod,
        applicationDeadline: job.applicationDeadline?.slice(0, 10) ?? '',
        requiredSkills: job.requiredSkills.join(', '),
        preferredSkills: job.preferredSkills.join(', '),
      }),
      error: (error: unknown) => { this.isError.set(true); this.errorMsg.set(this.apiError.getMessage(error, 'Failed to load job details.')); },
    });
  }

  onSubmit(): void {
    if (this.jobForm.invalid) {
      this.jobForm.markAllAsTouched();
      return;
    }
    const values = this.jobForm.getRawValue();
    if ((values.minimumSalary !== null || values.maximumSalary !== null) && (!values.currency || values.salaryPeriod === null)) {
      this.isError.set(true);
      this.errorMsg.set('Currency and salary period are required when a salary range is entered.');
      return;
    }
    if (values.maximumSalary !== null && values.minimumSalary !== null && values.maximumSalary < values.minimumSalary) {
      this.isError.set(true);
      this.errorMsg.set('Maximum salary cannot be lower than minimum salary.');
      return;
    }

    this.isSubmitting.set(true);
    this.isError.set(false);
    const request = this.buildRequest(values);
    const saveRequest = this.isEditMode && this.jobId !== null
      ? this.jobsService.update(this.jobId, request)
      : this.jobsService.create({ ...request, publishImmediately: values.publishImmediately } as CreateJobRequest);

    saveRequest.pipe(finalize(() => this.isSubmitting.set(false))).subscribe({
      next: (job) => void this.router.navigate(['/jobs', job.id]),
      error: (error: unknown) => {
        this.isError.set(true);
        this.errorMsg.set(this.apiError.getMessage(error, this.isEditMode ? 'Failed to update job posting.' : 'Failed to create job posting.'));
      },
    });
  }

  protected isInvalid(name: keyof typeof this.jobForm.controls): boolean {
    const control = this.jobForm.controls[name];
    return control.invalid && (control.dirty || control.touched);
  }

  private buildRequest(values: ReturnType<typeof this.jobForm.getRawValue>): JobWriteRequest {
    return {
      title: values.title.trim(), description: values.description.trim(), department: values.department.trim(),
      category: values.category.trim(), country: values.country.trim(), city: values.city.trim(),
      location: this.optional(values.location), workplaceType: values.workplaceType,
      employmentType: values.employmentType, experienceLevel: values.experienceLevel,
      minimumYearsExperience: values.minimumYearsExperience, maximumYearsExperience: values.maximumYearsExperience,
      responsibilities: values.responsibilities.trim(), requirements: values.requirements.trim(),
      preferredQualifications: this.optional(values.preferredQualifications),
      educationRequirements: this.optional(values.educationRequirements), benefits: this.optional(values.benefits),
      minimumSalary: values.minimumSalary, maximumSalary: values.maximumSalary,
      currency: this.optional(values.currency)?.toUpperCase() ?? null, salaryPeriod: values.salaryPeriod,
      isSalaryVisible: values.isSalaryVisible, numberOfVacancies: values.numberOfVacancies,
      applicationDeadline: values.applicationDeadline ? new Date(`${values.applicationDeadline}T23:59:59Z`).toISOString() : null,
      requiredSkills: this.skills(values.requiredSkills), preferredSkills: this.skills(values.preferredSkills),
    };
  }

  private skills(value: string): string[] {
    return [...new Set(value.split(',').map((skill) => skill.trim()).filter(Boolean))];
  }

  private optional(value: string): string | null {
    return value.trim() || null;
  }
}
