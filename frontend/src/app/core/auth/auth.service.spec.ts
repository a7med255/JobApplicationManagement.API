import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthResponse } from '../models/auth.models';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpTesting: HttpTestingController;

  const authResponse: AuthResponse = {
    accessToken: 'access-token',
    refreshToken: 'refresh-token',
    accessTokenExpiration: '2099-01-01T00:00:00Z',
    email: 'candidate@example.com',
    roles: ['Candidate'],
  };

  beforeEach(() => {
    localStorage.clear();
    sessionStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: Router,
          useValue: { navigate: vi.fn(() => Promise.resolve(true)) },
        },
      ],
    });

    service = TestBed.inject(AuthService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
    localStorage.clear();
    sessionStorage.clear();
  });

  it('stores a successful login in session storage', () => {
    service.login({ email: 'candidate@example.com', password: 'Password1' }).subscribe();

    const request = httpTesting.expectOne('/api/Auth/login');
    expect(request.request.method).toBe('POST');
    request.flush(authResponse);

    expect(service.isAuthenticated()).toBe(true);
    expect(service.userEmail()).toBe('candidate@example.com');
    expect(service.isCandidate()).toBe(true);
    expect(sessionStorage.length).toBe(1);
    expect(localStorage.length).toBe(0);
  });

  it('rotates tokens using the current refresh token', () => {
    service.login({ email: 'candidate@example.com', password: 'Password1' }).subscribe();
    httpTesting.expectOne('/api/Auth/login').flush(authResponse);

    service.refreshToken().subscribe();

    const request = httpTesting.expectOne('/api/Auth/refresh-token');
    expect(request.request.body).toEqual({
      accessToken: 'access-token',
      refreshToken: 'refresh-token',
    });
    request.flush({
      ...authResponse,
      accessToken: 'rotated-access-token',
      refreshToken: 'rotated-refresh-token',
    });

    expect(service.getAccessToken()).toBe('rotated-access-token');
  });

  it('clears authentication state on logout', () => {
    service.login({ email: 'candidate@example.com', password: 'Password1' }).subscribe();
    httpTesting.expectOne('/api/Auth/login').flush(authResponse);

    service.logout();

    expect(service.isAuthenticated()).toBe(false);
    expect(service.getAccessToken()).toBeNull();
    expect(sessionStorage.length).toBe(0);
  });
});
