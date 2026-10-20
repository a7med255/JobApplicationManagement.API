# Careerly

Careerly is a recruitment and job-application platform for candidates, recruiters, and administrators.

**Find Your Next Opportunity**

## What is included

- Candidate and recruiter registration, verified email, login, JWT refresh-token rotation, logout/revocation, password recovery, and rate limiting
- Resource ownership checks for profiles, jobs, saved jobs, applications, CVs, interviews, and notifications
- Professional job publishing lifecycle, search/filtering, saved jobs, recommendations, similar jobs, and job alerts
- Multiple PDF CVs with rename/default/preview/download/delete operations and immutable submitted-CV references
- Private Cloudinary `raw/authenticated` CV storage with server-side authorized delivery
- Cloudinary candidate photos and company logos with file type, signature, and size validation
- Application cover letters, controlled status transitions, status history, interview scheduling/rescheduling/cancellation
- Persistent in-app notifications and preferences
- Transactional email outbox, encrypted payloads, finite retries, Hangfire processing, and admin failure visibility
- Admin user lockout/role management and audit records
- Responsive Angular UI with lazy routes, accessible controls, loading/error/empty states, and centralized API handling

## Architecture

The solution follows Clean Architecture dependency direction:

```text
Domain <- Application <- Infrastructure <- API
                         ^
                         |
                      Angular UI
```

- `JobApplicationManagement.Domain`: entities and enums; no infrastructure dependencies
- `JobApplicationManagement.Application`: CQRS use cases, validation, authorization workflows, DTOs, and external-service abstractions
- `JobApplicationManagement.Infrastucture`: EF Core, SQL Server, Identity, JWT, SMTP, Cloudinary, data protection, repositories, and outbox delivery
- `JobApplicationManagement.API`: HTTP endpoints, middleware, rate limiting, health checks, Hangfire host, and dependency composition
- `frontend`: Angular standalone components, lazy routes, reactive forms, signals, guards, and interceptors
- `JobApplicationManagement.Tests`: xUnit unit tests for authentication, ownership, jobs, applications, CV security, and workflows

## Technology

- .NET 8, ASP.NET Core Web API, EF Core 8, SQL Server
- ASP.NET Core Identity, JWT bearer authentication, FluentValidation, MediatR
- Hangfire with SQL Server storage, MailKit SMTP, CloudinaryDotNet
- Serilog and ASP.NET Core health checks
- Angular 22, TypeScript 6, RxJS, SCSS, Vitest

## Prerequisites

- .NET 8 SDK
- SQL Server or SQL Server LocalDB
- Node.js 22 LTS or later compatible with the `frontend/package.json` engine
- Cloudinary account
- SMTP account that permits authenticated outbound mail

## Local configuration

Secrets are intentionally absent from tracked settings. From the repository root, configure .NET user secrets:

```powershell
dotnet user-secrets init --project JobApplicationManagement.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=Careerly;Trusted_Connection=True;TrustServerCertificate=True" --project JobApplicationManagement.API
dotnet user-secrets set "Jwt:Key" "replace-with-a-random-secret-at-least-32-characters" --project JobApplicationManagement.API
dotnet user-secrets set "Cloudinary:CloudName" "your-cloud-name" --project JobApplicationManagement.API
dotnet user-secrets set "Cloudinary:ApiKey" "your-api-key" --project JobApplicationManagement.API
dotnet user-secrets set "Cloudinary:ApiSecret" "your-api-secret" --project JobApplicationManagement.API
```

Email is disabled in the base development configuration. To test real email locally:

```powershell
dotnet user-secrets set "Email:Enabled" "true" --project JobApplicationManagement.API
dotnet user-secrets set "Email:Host" "smtp.example.com" --project JobApplicationManagement.API
dotnet user-secrets set "Email:Port" "587" --project JobApplicationManagement.API
dotnet user-secrets set "Email:UseSsl" "false" --project JobApplicationManagement.API
dotnet user-secrets set "Email:Username" "smtp-user" --project JobApplicationManagement.API
dotnet user-secrets set "Email:Password" "smtp-password" --project JobApplicationManagement.API
dotnet user-secrets set "Email:FromAddress" "no-reply@example.com" --project JobApplicationManagement.API
dotnet user-secrets set "Email:FrontendBaseUrl" "http://localhost:4200" --project JobApplicationManagement.API
```

Do not put real credentials in any `appsettings*.json`, Angular environment file, source file, or GitHub workflow.

## Database

Apply the checked-in migrations:

```powershell
dotnet tool update --global dotnet-ef --version 8.*
dotnet ef database update --project JobApplicationManagement.Infrastucture --startup-project JobApplicationManagement.API
```

Generate an idempotent deployment script for review:

```powershell
dotnet ef migrations script --idempotent --project JobApplicationManagement.Infrastucture --startup-project JobApplicationManagement.API --output migration-preview.sql
```

Migrations are additive. They preserve legacy database-backed CV bytes for existing applications, backfill their storage metadata, and normalize the historic `InterView` status spelling.

## Run locally

API:

```powershell
dotnet restore JobApplicationManagement.API.sln
.\start-api.ps1
```

The launcher keeps a single-backend workflow: it reuses a healthy Careerly API on
port `5142`, reports unrelated port owners without terminating them, and starts the
API only when the configured port is free.

Angular:

```powershell
cd frontend
npm ci
npm start
```

The Angular development server proxies `/api` to the API address configured in `frontend/proxy.conf.json`. Swagger is available only in Development. `GET /health` verifies the API and database connection.

## Tests and security audits

```powershell
dotnet build JobApplicationManagement.API.sln --configuration Release
dotnet test JobApplicationManagement.API.sln --configuration Release --no-build
dotnet list JobApplicationManagement.API.sln package --vulnerable --include-transitive

cd frontend
npm ci
npm run build:production
npm run test:ci
npm audit --audit-level=high
```

CI runs the same backend and frontend checks on `master` and `main`.

## Production environment variables

ASP.NET Core maps `__` to nested configuration keys. Set these in the hosting control panel, not in source control:

| Variable | Purpose |
|---|---|
| `ASPNETCORE_ENVIRONMENT=Production` | Enables production validation and security behavior |
| `ConnectionStrings__DefaultConnection` | Careerly and Hangfire SQL Server connection |
| `ConnectionStrings__HangfireConnection` | Optional separate Hangfire SQL connection |
| `Jwt__Key` | Random signing key, at least 32 characters |
| `Jwt__Issuer`, `Jwt__Audience` | Token issuer/audience |
| `Cors__AllowedOrigins__0` | Exact HTTPS Angular origin |
| `Cloudinary__CloudName` | Cloudinary cloud name |
| `Cloudinary__ApiKey` | Cloudinary API key |
| `Cloudinary__ApiSecret` | Cloudinary API secret |
| `Email__Enabled=true` | Required in production |
| `Email__Host`, `Email__Port` | SMTP server and port |
| `Email__UseSsl` | SMTP transport setting required by the provider |
| `Email__Username`, `Email__Password` | SMTP credentials |
| `Email__FromAddress`, `Email__FromName` | Sender identity |
| `Email__FrontendBaseUrl` | Public Angular base URL used by email CTAs |
| `DataProtection__KeysPath` | Persistent, writable, access-controlled key directory |

See `JobApplicationManagement.API/appsettings.Production.example.json` for placeholder structure.

## MonsterASP.NET deployment

1. Verify the plan supports .NET 8, SQL Server, environment variables, outbound HTTPS/SMTP, and an always-running application.
2. Create the SQL database and apply the idempotent migration script through the hosting SQL tooling.
3. Publish the API:

   ```powershell
   dotnet publish JobApplicationManagement.API --configuration Release --output artifacts/api
   ```

4. Configure the API as an ASP.NET Core application at `/api` or adapt the Angular production API base URL to its HTTPS origin.
5. Build Angular and upload the contents of `frontend/dist/job-portal/browser` to the web root. The included `frontend/public/web.config` provides SPA route fallback and security headers while excluding `/api`.
6. Create a persistent writable directory such as `App_Data/DataProtectionKeys`, restrict its ACL to the application identity, and set `DataProtection__KeysPath` accordingly.
7. Configure every production variable above and enable HTTPS-only access.
8. Enable MonsterASP.NET's Always Running feature. Hangfire must stay active for email retries and scheduled job expiration.
9. Confirm `/health`, registration email, password reset, CV upload/download authorization, application emails, notifications, and interview emails using production provider accounts.

### Hosting limitations to verify

- Hangfire reliability depends on the hosting plan keeping the worker process alive. Without Always Running, queued emails remain safely in SQL but delivery can be delayed until the app wakes.
- Data-protection keys must survive deployments and restarts; ephemeral storage will make encrypted outbox payloads unreadable.
- SMTP and Cloudinary require outbound network access. Provider connectivity cannot be established by a build alone.
- SignalR is deliberately not required. Persistent database notifications remain the source of truth and work without WebSockets.
- The included Dockerfile builds the API only; MonsterASP.NET's standard IIS deployment does not require Docker.

## Operational behavior

- The application transaction commits applications, status history, notifications, and encrypted email-outbox records together.
- SMTP failures do not roll back valid applications. The outbox retries five times with backoff and exposes pending failures to administrators.
- CVs are never emailed and are not exposed using permanent public URLs. Candidates can access their own CVs; recruiters can access only the submitted CV on jobs they own.
- Deleting a referenced CV soft-deletes it but retains the Cloudinary object so historical applications remain valid.
- Production startup fails fast when database, JWT, CORS, Cloudinary, SMTP, or persistent data-protection configuration is missing.
