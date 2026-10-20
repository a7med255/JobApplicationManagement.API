using Hangfire;
using Hangfire.Dashboard;
using JobApplicationManagement.API.Middleware;
using JobApplicationManagement.API.Security;
using JobApplicationManagement.API.Services;
using JobApplicationManagement.Application;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Infrastructure;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http.Features;

// ── Serilog bootstrap logger ──────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting JobApplicationManagement API");

    var builder = WebApplication.CreateBuilder(args);

    var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(defaultConnection))
        throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection is required. Configure it with user secrets or environment variables.");

    var jwtKey = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
        throw new InvalidOperationException(
            "Jwt:Key must be provided securely and contain at least 32 characters.");

    if (!builder.Environment.IsDevelopment())
    {
        var requiredSettings = new[]
        {
            "Cloudinary:CloudName", "Cloudinary:ApiKey", "Cloudinary:ApiSecret",
            "Email:Host", "Email:Username", "Email:Password", "Email:FromAddress", "Email:FrontendBaseUrl",
            "DataProtection:KeysPath"
        };
        var missing = requiredSettings.Where(key => string.IsNullOrWhiteSpace(builder.Configuration[key])).ToArray();
        if (missing.Length > 0)
            throw new InvalidOperationException($"Missing required production configuration: {string.Join(", ", missing)}.");
        if (!builder.Configuration.GetValue<bool>("Email:Enabled"))
            throw new InvalidOperationException("Email:Enabled must be true in production.");
    }

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
                     .ReadFrom.Services(services)
                     .Enrich.FromLogContext());

    // ── Application Layer Services ────────────────────────────────────────────
    builder.Services.AddApplicationServices();
    builder.Services.Configure<FileUploadOptions>(
        builder.Configuration.GetSection(FileUploadOptions.SectionName));
    var maximumUploadBytes = Math.Max(
        builder.Configuration.GetValue<long>("FileStorage:MaximumCvBytes", 5 * 1024 * 1024),
        builder.Configuration.GetValue<long>("FileStorage:MaximumImageBytes", 2 * 1024 * 1024));
    builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = maximumUploadBytes + 64 * 1024);

    // ── Infrastructure Layer Services ─────────────────────────────────────────
    // Registers: DbContext, Identity, JWT auth, IGenericRepository<>, IUnitOfWork,
    //            IIdentityService, IJwtService, IRefreshTokenService
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // ── Current User Service ──────────────────────────────────────────────────
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    // ── ASP.NET Core ──────────────────────────────────────────────────────────
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    if (builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
        allowedOrigins = ["http://localhost:4200", "https://localhost:4200", "http://localhost:56172"];
    if (!builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
        throw new InvalidOperationException("Cors:AllowedOrigins must contain the deployed frontend origin in production.");

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendPolicy", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
        options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    });

    builder.Services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromDays(365);
        options.IncludeSubDomains = true;
        options.Preload = true;
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // ── Health Checks ─────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddSqlServer(defaultConnection);

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "JobApplicationManagement API",
            Version = "v1",
            Description = "API for managing job postings and applications."
        });

        // JWT Bearer support in Swagger UI
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT access token. Example: Bearer {token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
    });
    var hangfireConnection = builder.Configuration.GetConnectionString("HangfireConnection")
        ?? defaultConnection;
    builder.Services.AddHangfire(config => config
           .UseSimpleAssemblyNameTypeSerializer()
           .UseRecommendedSerializerSettings()
           .UseSqlServerStorage(
               hangfireConnection));

    builder.Services.AddHangfireServer();
    var app = builder.Build();

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                           Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto,
        ForwardLimit = 1
    });

    // ── Seed Roles ────────────────────────────────────────────────────────────
    await JobApplicationManagement.Infrastructure.DependencyInjection.SeedRolesAsync(app.Services);

    if (app.Environment.IsDevelopment())
    {
        await JobApplicationManagement.Infrastructure.Persistence.DevelopmentDataSeeder.SeedAsync(app.Services);
    }

    // ── Serilog Request Logging (outermost to log the final HTTP response status) ───
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                diagnosticContext.Set("UserId", userId);
            }

            var roles = httpContext.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
            if (roles.Any())
            {
                diagnosticContext.Set("UserRole", string.Join(",", roles));
            }

            diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
        };
    });

    // ── Global Exception Handling ─────────────────────────────────────────────
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
        app.UseHttpsRedirection();
        app.Use(async (context, next) =>
        {
            context.Response.Headers.XContentTypeOptions = "nosniff";
            context.Response.Headers.XFrameOptions = "DENY";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";
            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            await next();
        });
    }

    app.UseCors("FrontendPolicy");
    app.UseRateLimiter();

    // ── Authentication must come before Authorization ─────────────────────────
    app.UseAuthentication();
    app.UseAuthorization();

    // ── Push user details to Serilog LogContext for business logs ─────────────
    app.UseMiddleware<RequestContextLoggingMiddleware>();

    // ── Hangfire Dashboard & Recurring Jobs ───────────────────────────────────
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new AdminHangfireAuthorizationFilter()]
    });

    RecurringJob.AddOrUpdate<IStaleApplicationCleanupJob>(
        "auto-close-stale-applications",
        job => job.CloseStaleApplicationsAsync(CancellationToken.None),
        Cron.Daily());

    RecurringJob.AddOrUpdate<IJobExpirationService>(
        "expire-job-postings-after-deadline",
        service => service.ExpireJobsAsync(CancellationToken.None),
        Cron.Hourly());

    if (builder.Configuration.GetValue<bool>("Email:Enabled"))
        RecurringJob.AddOrUpdate<IOutboxProcessor>(
            "process-email-outbox",
            service => service.ProcessPendingAsync(CancellationToken.None),
            Cron.Minutely());
    else
        RecurringJob.RemoveIfExists("process-email-outbox");

    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
