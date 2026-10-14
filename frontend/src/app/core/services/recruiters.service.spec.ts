import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { RecruiterResponse } from '../models/recruiter.models';
import { RecruitersService } from './recruiters.service';

describe('RecruitersService', () => {
  let service: RecruitersService;
  let httpTesting: HttpTestingController;

  const recruiter: RecruiterResponse = {
    id: 8,
    name: 'Alex Rivera',
    userId: 'identity-user-8',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(RecruitersService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('requests a paginated recruiter profile page', () => {
    service.getAll(3, 15).subscribe();

    const request = httpTesting.expectOne(
      (recruiterRequest) =>
        recruiterRequest.url === '/api/Recruiters' &&
        recruiterRequest.params.get('pageNumber') === '3' &&
        recruiterRequest.params.get('pageSize') === '15',
    );
    expect(request.request.method).toBe('GET');
    request.flush({ items: [recruiter], pageNumber: 3, pageSize: 15, totalCount: 1, totalPages: 1 });
  });

  it('updates only the editable recruiter name', () => {
    const body = { name: 'Updated Recruiter' };
    service.update(recruiter.id, body).subscribe();

    const request = httpTesting.expectOne('/api/Recruiters/8');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(body);
    request.flush({ ...recruiter, ...body });
  });

  it('deletes a recruiter profile by ID', () => {
    service.delete(recruiter.id).subscribe();

    const request = httpTesting.expectOne('/api/Recruiters/8');
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
  });
});
