import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { SavedJobsService } from './saved-jobs.service';

describe('SavedJobsService', () => {
  let service: SavedJobsService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(SavedJobsService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('requests saved jobs with pagination', () => {
    service.getMySavedJobs(2, 12).subscribe();

    const request = httpTesting.expectOne(
      (candidate) =>
        candidate.url === '/api/jobs/saved' &&
        candidate.params.get('pageNumber') === '2' &&
        candidate.params.get('pageSize') === '12',
    );
    expect(request.request.method).toBe('GET');
    request.flush({ items: [], pageNumber: 2, pageSize: 12, totalCount: 0, totalPages: 0 });
  });

  it('saves and removes a job', () => {
    service.save(9).subscribe();
    const saveRequest = httpTesting.expectOne('/api/jobs/9/save');
    expect(saveRequest.request.method).toBe('POST');
    saveRequest.flush({
      id: 1,
      jobId: 9,
      jobTitle: 'Engineer',
      jobDescription: 'Description',
      isActive: true,
      savedAt: '2026-09-28T00:00:00Z',
    });

    service.remove(9).subscribe();
    const removeRequest = httpTesting.expectOne('/api/jobs/9/save');
    expect(removeRequest.request.method).toBe('DELETE');
    removeRequest.flush(null);
  });

  it('checks the saved status', () => {
    service.isSaved(9).subscribe((status) => expect(status.isSaved).toBe(true));

    const request = httpTesting.expectOne('/api/jobs/9/saved-status');
    expect(request.request.method).toBe('GET');
    request.flush({ isSaved: true });
  });
});
