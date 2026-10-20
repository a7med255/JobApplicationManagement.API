import { Routes } from '@angular/router';
import { APP_CONFIG } from './core/config/app.constants';
import { authGuard, roleGuard, guestGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  // ── Public ────────────────────────────────────────────────────────────────
  {
    path: '',
    title: `${APP_CONFIG.name} | Job Application Management`,
    loadComponent: () => import('./pages/home/home.component').then((m) => m.HomeComponent),
  },
  {
    path: 'login',
    title: `Sign In | ${APP_CONFIG.name}`,
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    title: `Create Account | ${APP_CONFIG.name}`,
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: 'verify-email',
    title: `Verify Email | ${APP_CONFIG.name}`,
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/verify-email/verify-email.component').then((m) => m.VerifyEmailComponent),
  },
  {
    path: 'forgot-password',
    title: `Forgot Password | ${APP_CONFIG.name}`,
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then((m) => m.ForgotPasswordComponent),
  },
  {
    path: 'reset-password',
    title: `Reset Password | ${APP_CONFIG.name}`,
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/reset-password/reset-password.component').then((m) => m.ResetPasswordComponent),
  },

  // ── Jobs (all authenticated users) ────────────────────────────────────────
  {
    path: 'jobs',
    children: [
      {
        path: '',
        title: `Jobs | ${APP_CONFIG.name}`,
        loadComponent: () => import('./features/jobs/job-list/job-list.component').then((m) => m.JobListComponent),
      },
      {
        path: ':id',
        title: `Job Details | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/jobs/job-details/job-details.component').then((m) => m.JobDetailsComponent),
      },
    ],
  },
  {
    path: 'notifications',
    title: `Notifications | ${APP_CONFIG.name}`,
    canActivate: [authGuard],
    loadComponent: () => import('./features/notifications/notifications.component').then((m) => m.NotificationsComponent),
  },

  // ── Candidate ─────────────────────────────────────────────────────────────
  {
    path: 'candidate',
    canActivate: [authGuard, roleGuard('Candidate')],
    children: [
      {
        path: 'dashboard',
        title: `Candidate Dashboard | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/candidate/dashboard/candidate-dashboard.component').then(
            (m) => m.CandidateDashboardComponent,
          ),
      },
      {
        path: 'applications',
        title: `My Applications | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/candidate/applications/candidate-applications.component').then(
            (m) => m.CandidateApplicationsComponent,
          ),
      },
      {
        path: 'applications/:id',
        title: `Application Details | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/candidate/application-details/candidate-application-details.component').then(
            (m) => m.CandidateApplicationDetailsComponent,
          ),
      },
      {
        path: 'saved-jobs',
        title: `Saved Jobs | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/candidate/saved-jobs/candidate-saved-jobs.component').then(
            (m) => m.CandidateSavedJobsComponent,
          ),
      },
      {
        path: 'profile',
        title: `My Profile | ${APP_CONFIG.name}`,
        loadComponent: () => import('./features/candidate/profile/candidate-profile/candidate-profile.component').then((m) => m.CandidateProfileComponent),
      },
      {
        path: 'cvs',
        title: `My CVs | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/candidate/cvs/candidate-cvs.component').then((m) => m.CandidateCvsComponent),
      },
      {
        path: 'job-alerts',
        title: `Job Alerts | ${APP_CONFIG.name}`,
        loadComponent: () => import('./features/candidate/job-alerts/candidate-job-alerts.component').then((m) => m.CandidateJobAlertsComponent),
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },

  // ── Recruiter ─────────────────────────────────────────────────────────────
  {
    path: 'recruiter',
    canActivate: [authGuard, roleGuard('Recruiter')],
    children: [
      {
        path: 'dashboard',
        title: `Recruiter Dashboard | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/recruiter/dashboard/recruiter-dashboard.component').then(
            (m) => m.RecruiterDashboardComponent,
          ),
      },
      {
        path: 'jobs/create',
        title: `Post a Job | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/recruiter/job-form/recruiter-job-form.component').then(
            (m) => m.RecruiterJobFormComponent,
          ),
      },
      {
        path: 'jobs',
        title: `My Jobs | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/recruiter/jobs/recruiter-jobs.component').then((m) => m.RecruiterJobsComponent),
      },
      {
        path: 'company',
        title: `Company Profile | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/recruiter/company/recruiter-company.component').then((m) => m.RecruiterCompanyComponent),
      },
      {
        path: 'profile',
        title: `My Profile | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/recruiter/profile/recruiter-profile/recruiter-profile.component').then((m) => m.RecruiterProfileComponent),
      },
      {
        path: 'jobs/:id/edit',
        title: `Edit Job | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/recruiter/job-form/recruiter-job-form.component').then(
            (m) => m.RecruiterJobFormComponent,
          ),
      },
      {
        path: 'applications',
        title: `Recruiter Applications | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/recruiter/applications/recruiter-applications.component').then(
            (m) => m.RecruiterApplicationsComponent,
          ),
      },
      {
        path: 'applications/:id',
        title: `Review Application | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/recruiter/application-review/recruiter-application-review.component').then(
            (m) => m.RecruiterApplicationReviewComponent,
          ),
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },

  // ── Admin ─────────────────────────────────────────────────────────────────
  {
    path: 'admin',
    canActivate: [authGuard, roleGuard('Admin')],
    loadComponent: () => import('./features/admin/admin-shell.component').then((m) => m.AdminShellComponent),
    children: [
      {
        path: 'dashboard',
        title: `Admin Dashboard | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/admin/dashboard/admin-dashboard.component').then((m) => m.AdminDashboardComponent),
      },
      {
        path: 'candidates',
        title: `Candidate Profiles | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/admin/candidates/admin-candidates.component').then((m) => m.AdminCandidatesComponent),
      },
      {
        path: 'recruiters',
        title: `Recruiter Profiles | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/admin/recruiters/admin-recruiters.component').then((m) => m.AdminRecruitersComponent),
      },
      {
        path: 'jobs',
        title: `Manage Jobs | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/admin/jobs/admin-jobs.component').then((m) => m.AdminJobsComponent),
      },
      {
        path: 'jobs/:id/edit',
        title: `Edit Job | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/recruiter/job-form/recruiter-job-form.component').then(
            (m) => m.RecruiterJobFormComponent,
          ),
      },
      {
        path: 'applications',
        title: `Manage Applications | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/admin/applications/admin-applications.component').then(
            (m) => m.AdminApplicationsComponent,
          ),
      },
      {
        path: 'applications/:id',
        title: `Review Application | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/recruiter/application-review/recruiter-application-review.component').then(
            (m) => m.RecruiterApplicationReviewComponent,
          ),
      },
      {
        path: 'users',
        title: `Users & Operations | ${APP_CONFIG.name}`,
        loadComponent: () =>
          import('./features/admin/users/admin-users.component').then(
            (m) => m.AdminUsersComponent,
          ),
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },

  // ── Error pages ───────────────────────────────────────────────────────────
  {
    path: 'forbidden',
    title: `Access Denied | ${APP_CONFIG.name}`,
    loadComponent: () => import('./pages/forbidden/forbidden.component').then((m) => m.ForbiddenComponent),
  },
  {
    path: '**',
    title: `Page Not Found | ${APP_CONFIG.name}`,
    loadComponent: () => import('./pages/not-found/not-found.component').then((m) => m.NotFoundComponent),
  },
];
