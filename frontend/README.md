# JobPortal frontend

Angular 22 standalone application for the Job Application Management API in the repository root.

## Toolchain

- Angular CLI 22.2.0
- Angular 22.2.0
- Node.js 24.21.0 was used during setup and verification
- npm 11.x
- Strict TypeScript and strict Angular templates

## Local development

Install the locked dependencies:

```powershell
npm ci
```

Start the ASP.NET Core API from the repository root. Its HTTP development profile listens on `http://localhost:5141`:

```powershell
dotnet run --project .\JobApplicationManagement.API\JobApplicationManagement.API.csproj --launch-profile http
```

In a second terminal, start Angular from this directory:

```powershell
npm start
```

Open `http://localhost:4200`. The development server proxies `/api` to `http://localhost:5141` using `proxy.conf.json`.

## API and CORS configuration

Both development and production environments use `/api` as the API base URL. During development, the Angular proxy makes browser requests same-origin, so backend CORS middleware is not required.

The current ASP.NET Core backend does not register or enable CORS. Production should therefore serve the frontend and proxy `/api` from the same public origin. If the frontend and API are deployed to different origins, add an explicit, restricted CORS policy to the backend before deployment and change `src/environments/environment.production.ts` to the public API origin.

## Architecture

- `core/auth`: session state, functional guards, and the functional JWT/refresh interceptor
- `core/models`: DTOs matching the backend response/request contracts
- `core/services`: typed API clients and centralized error parsing
- `features/auth`: login and Candidate/Recruiter registration
- `features/jobs`: authenticated job browsing and details
- `features/candidate`: application and saved-job workflows
- `features/recruiter`: job creation/editing and scoped application review
- `features/admin`: global job, application, candidate-profile, and recruiter-profile management
- `shared/components`: reusable status and job presentation components

Feature routes are lazy-loaded. Route guards enforce the backend roles (`Candidate`, `Recruiter`, and `Admin`) before feature activation.

## Backend-driven constraints

- Job browsing requires authentication because the backend job endpoints are not public.
- Recruiters can create jobs, but the API does not expose an owned-jobs query or recruiter ID on job responses. The UI therefore does not claim that the global job list is “My Jobs.”
- Candidate and recruiter self-profile pages are not linked because authentication responses do not include profile IDs and the API has no current-profile endpoint.
- Interview and notification screens are omitted because there are no corresponding API endpoints.
- Admin profile creation is omitted because the API requires an existing Identity user ID but exposes no user-discovery or user-management endpoint.
- Admin job creation is omitted because the command currently requires the caller to have a recruiter profile even though the controller permits the Admin role.

## Verification

Run the non-watch unit suite:

```powershell
npm run test:ci
```

Create an optimized production build:

```powershell
npm run build:production
```

Build output is written to `dist/job-portal/browser` (or the Angular CLI output path reported by the installed builder).

## Deployment checklist

- Serve the generated browser files over HTTPS.
- Configure the host to fall back to `index.html` for Angular routes.
- Reverse-proxy `/api` to the ASP.NET Core API on the same origin, or add a restricted backend CORS policy for a separate API origin.
- Supply production database, JWT, email, and Hangfire configuration to the backend through secure deployment settings.
- Run `npm run test:ci`, `npm run build:production`, backend tests, and backend build in CI.
