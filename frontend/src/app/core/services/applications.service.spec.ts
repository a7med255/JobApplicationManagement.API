import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { JobApplicationStatus } from '../models/application.models';
import { ApplicationsService } from './applications.service';

describe('ApplicationsService', () => {
  let service: ApplicationsService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ApplicationsService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('requests the role-scoped applications page', () => {
    service.getAll(3, 10).subscribe();

    const request = httpTesting.expectOne(
      (candidate) =>
        candidate.url === '/api/Applications' &&
        candidate.params.get('pageNumber') === '3' &&
        candidate.params.get('pageSize') === '10',
    );
    expect(request.request.method).toBe('GET');
    request.flush({ items: [], pageNumber: 3, pageSize: 10, totalCount: 0, totalPages: 0 });
  });

  it('submits the backend job application DTO', () => {
    service.apply({ jobId: 42 }).subscribe();

    const request = httpTesting.expectOne('/api/Applications');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ jobId: 42 });
    request.flush({
      id: 5,
      candidateId: 2,
      jobId: 42,
      jobApplicationStatus: JobApplicationStatus.Applied,
      appliedAt: '2026-09-28T00:00:00Z',
      statusUpdatedAt: '2026-09-28T00:00:00Z',
      cancelledAt: null,
    });
  });

  it('cancels an application with the dedicated endpoint', () => {
    service.cancel(5).subscribe();

    const request = httpTesting.expectOne('/api/Applications/5/cancel');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({});
    request.flush(null);
  });

  it('sends numeric application statuses', () => {
    service.updateStatus(5, { jobApplicationStatus: JobApplicationStatus.Accepted }).subscribe();

    const request = httpTesting.expectOne('/api/Applications/5');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ jobApplicationStatus: 4 });
    request.flush({});
  });
});
