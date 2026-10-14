import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { vi } from 'vitest';
import { AuthResponse } from '../models/auth.models';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpTesting: HttpTestingController;
  let refreshResponse: Subject<AuthResponse>;

  const auth = {
    getAccessToken: vi.fn(() => 'expired-token'),
    canRefresh: vi.fn(() => true),
    refreshToken: vi.fn(),
    logout: vi.fn(),
  };
  const router = { navigate: vi.fn() };

  beforeEach(() => {
    refreshResponse = new Subject<AuthResponse>();
    auth.getAccessToken.mockReturnValue('expired-token');
    auth.canRefresh.mockReturnValue(true);
    auth.refreshToken.mockReturnValue(refreshResponse.asObservable());
    auth.logout.mockReset();
    router.navigate.mockReset();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: auth },
        { provide: Router, useValue: router },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('shares one refresh request across concurrent 401 responses and retries both calls', () => {
    http.get('/api/Jobs').subscribe();
    http.get('/api/Applications').subscribe();

    const initialRequests = httpTesting.match(
      (request) => request.url === '/api/Jobs' || request.url === '/api/Applications',
    );
    expect(initialRequests).toHaveLength(2);
    initialRequests.forEach((request) => {
      expect(request.request.headers.get('Authorization')).toBe('Bearer expired-token');
      request.flush(null, { status: 401, statusText: 'Unauthorized' });
    });

    expect(auth.refreshToken).toHaveBeenCalledTimes(1);

    refreshResponse.next({
      email: 'candidate@example.test',
      roles: ['Candidate'],
      accessToken: 'fresh-token',
      refreshToken: 'rotated-refresh-token',
      accessTokenExpiration: '2026-09-30T00:00:00Z',
    });
    refreshResponse.complete();

    const retriedRequests = httpTesting.match(
      (request) => request.url === '/api/Jobs' || request.url === '/api/Applications',
    );
    expect(retriedRequests).toHaveLength(2);
    retriedRequests.forEach((request) => {
      expect(request.request.headers.get('Authorization')).toBe('Bearer fresh-token');
      request.flush({});
    });
  });

  it('redirects forbidden responses without discarding the server error', () => {
    let receivedStatus: number | undefined;
    http.get('/api/Candidates').subscribe({
      error: (error: { status: number }) => {
        receivedStatus = error.status;
      },
    });

    const request = httpTesting.expectOne('/api/Candidates');
    request.flush(null, { status: 403, statusText: 'Forbidden' });

    expect(router.navigate).toHaveBeenCalledWith(['/forbidden']);
    expect(receivedStatus).toBe(403);
  });
});
