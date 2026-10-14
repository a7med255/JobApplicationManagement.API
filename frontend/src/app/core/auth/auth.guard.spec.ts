import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  CanActivateFn,
  Router,
  RouterStateSnapshot,
  UrlTree,
  provideRouter,
} from '@angular/router';
import { AuthService } from './auth.service';
import { authGuard, guestGuard, roleGuard } from './auth.guard';

describe('authentication guards', () => {
  const auth = {
    isAuthenticated: vi.fn(() => false),
    hasAnyRole: vi.fn(() => false),
    getDefaultRoute: vi.fn(() => '/candidate/dashboard'),
  };

  let router: Router;

  beforeEach(() => {
    auth.isAuthenticated.mockReturnValue(false);
    auth.hasAnyRole.mockReturnValue(false);
    auth.getDefaultRoute.mockReturnValue('/candidate/dashboard');

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: auth },
      ],
    });

    router = TestBed.inject(Router);
  });

  it('redirects unauthenticated users to login with a return URL', () => {
    const result = runGuard(authGuard, '/candidate/applications');

    expect(result).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(result as UrlTree)).toBe(
      '/login?returnUrl=%2Fcandidate%2Fapplications',
    );
  });

  it('allows an authenticated user through the authentication guard', () => {
    auth.isAuthenticated.mockReturnValue(true);

    expect(runGuard(authGuard, '/jobs')).toBe(true);
  });

  it('redirects an authenticated user with the wrong role', () => {
    auth.isAuthenticated.mockReturnValue(true);

    const result = runGuard(roleGuard('Admin'), '/admin/dashboard');

    expect(result).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(result as UrlTree)).toBe('/forbidden');
  });

  it('redirects authenticated guests to their role home', () => {
    auth.isAuthenticated.mockReturnValue(true);

    const result = runGuard(guestGuard, '/login');

    expect(result).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(result as UrlTree)).toBe('/candidate/dashboard');
  });
});

function runGuard(guard: CanActivateFn, url: string): boolean | UrlTree {
  return TestBed.runInInjectionContext(() =>
    guard(
      {} as ActivatedRouteSnapshot,
      { url } as RouterStateSnapshot,
    ) as boolean | UrlTree,
  );
}
