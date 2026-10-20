import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CandidateResponse } from '../models/candidate.models';
import { CandidatesService } from './candidates.service';

describe('CandidatesService', () => {
  let service: CandidatesService;
  let httpTesting: HttpTestingController;

  const candidate: CandidateResponse = {
    id: 4,
    name: 'Taylor Morgan',
    cvUrl: 'https://example.test/cv',
    userId: 'identity-user-4',
    profileImageUrl: null,
    professionalTitle: null,
    summary: null,
    country: null,
    city: null,
    yearsOfExperience: null,
    profileCompleteness: 25,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(CandidatesService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('requests a paginated candidate profile page', () => {
    service.getAll(2, 15).subscribe();

    const request = httpTesting.expectOne(
      (candidateRequest) =>
        candidateRequest.url === '/api/Candidates' &&
        candidateRequest.params.get('pageNumber') === '2' &&
        candidateRequest.params.get('pageSize') === '15',
    );
    expect(request.request.method).toBe('GET');
    request.flush({ items: [candidate], pageNumber: 2, pageSize: 15, totalCount: 1, totalPages: 1 });
  });

  it('updates only editable candidate profile fields', () => {
    const body = {
      name: 'Updated Candidate', cvUrl: 'https://example.test/new-cv', professionalTitle: null,
      summary: null, country: null, city: null, yearsOfExperience: null,
    };
    service.update(candidate.id, body).subscribe();

    const request = httpTesting.expectOne('/api/Candidates/4');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(body);
    request.flush({ ...candidate, ...body });
  });

  it('deletes a candidate profile by ID', () => {
    service.delete(candidate.id).subscribe();

    const request = httpTesting.expectOne('/api/Candidates/4');
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
  });
});
