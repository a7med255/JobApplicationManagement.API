import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import {
  CreateJobRequest,
  EmploymentType,
  ExperienceLevel,
  JobResponse,
  JobStatus,
  SalaryPeriod,
  WorkplaceType,
} from '../models/job.models';
import { JobsService } from './jobs.service';

describe('JobsService', () => {
  let service: JobsService;
  let httpTesting: HttpTestingController;

  const writeRequest: CreateJobRequest = {
    title: 'Frontend Engineer',
    description: 'Build accessible Angular applications.',
    department: 'Engineering',
    category: 'Software Development',
    country: 'Egypt',
    city: 'Cairo',
    location: null,
    workplaceType: WorkplaceType.Hybrid,
    employmentType: EmploymentType.FullTime,
    experienceLevel: ExperienceLevel.Senior,
    minimumYearsExperience: 5,
    maximumYearsExperience: 8,
    responsibilities: 'Build product features.',
    requirements: 'Angular expertise.',
    preferredQualifications: null,
    educationRequirements: null,
    benefits: null,
    minimumSalary: 30000,
    maximumSalary: 50000,
    currency: 'EGP',
    salaryPeriod: SalaryPeriod.Monthly,
    isSalaryVisible: true,
    numberOfVacancies: 2,
    applicationDeadline: '2026-12-31T23:59:59Z',
    requiredSkills: ['Angular', 'TypeScript'],
    preferredSkills: ['Accessibility'],
    publishImmediately: true,
  };

  const job: JobResponse = {
    ...writeRequest,
    id: 7,
    company: { id: 3, name: 'Acme', logoUrl: null, description: null, website: null, industry: null, companySize: null, headquarters: null },
    postedAt: '2026-09-29T12:00:00Z',
    status: JobStatus.Published,
    isActive: true,
    createdAt: '2026-09-29T12:00:00Z',
    updatedAt: '2026-09-29T12:00:00Z',
    closedAt: null,
    isSaved: false,
    hasApplied: false,
    applicationId: null,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(JobsService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('requests a filtered paginated jobs page', () => {
    service.getAll(2, 12, { city: 'Cairo', workplaceType: WorkplaceType.Hybrid }).subscribe();
    const request = httpTesting.expectOne((candidate) =>
      candidate.url === '/api/Jobs' && candidate.params.get('pageNumber') === '2' &&
      candidate.params.get('city') === 'Cairo' && candidate.params.get('workplaceType') === '2');
    expect(request.request.method).toBe('GET');
    request.flush({ items: [job], pageNumber: 2, pageSize: 12, totalCount: 1, totalPages: 1 });
  });

  it('requests recruiter-owned jobs separately', () => {
    service.getMine(1, 15, { status: JobStatus.Draft }).subscribe();
    const request = httpTesting.expectOne((candidate) => candidate.url === '/api/Jobs/mine' && candidate.params.get('status') === '0');
    expect(request.request.method).toBe('GET');
    request.flush({ items: [], pageNumber: 1, pageSize: 15, totalCount: 0, totalPages: 0 });
  });

  it('requests a job by ID', () => {
    service.getById(7).subscribe((result) => expect(result).toEqual(job));
    const request = httpTesting.expectOne('/api/Jobs/7');
    expect(request.request.method).toBe('GET');
    request.flush(job);
  });

  it('creates a complete job request', () => {
    service.create(writeRequest).subscribe();
    const request = httpTesting.expectOne('/api/Jobs');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(writeRequest);
    request.flush(job);
  });

  it('updates a complete job request', () => {
    const { publishImmediately: _, ...updateRequest } = writeRequest;
    service.update(7, updateRequest).subscribe();
    const request = httpTesting.expectOne('/api/Jobs/7');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(updateRequest);
    request.flush(job);
  });

  it('publishes, closes and reopens a job through explicit lifecycle endpoints', () => {
    service.publish(7).subscribe();
    const publishRequest = httpTesting.expectOne('/api/Jobs/7/publish');
    expect(publishRequest.request.method).toBe('PUT');
    publishRequest.flush(job);

    service.close(7).subscribe();
    const closeRequest = httpTesting.expectOne('/api/Jobs/7/close');
    expect(closeRequest.request.method).toBe('PUT');
    closeRequest.flush({ ...job, status: JobStatus.Closed, isActive: false });

    service.reopen(7).subscribe();
    const reopenRequest = httpTesting.expectOne('/api/Jobs/7/reopen');
    expect(reopenRequest.request.method).toBe('PUT');
    reopenRequest.flush(job);
  });
});
