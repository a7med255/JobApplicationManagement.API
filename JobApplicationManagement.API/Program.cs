using Hangfire;
using JobApplicationManagement.API.Middleware;
using JobApplicationManagement.API.Services;
using JobApplicationManagement.Application;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Infrastructure;
using JobApplicationManagement.Infrastucture.Repositories;
using JobApplicationManagement.Infrastucture.Services;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;

// ── Serilog bootstrap logger ──────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting JobApplicationManagement API");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
                     .ReadFrom.Services(services)
                     .Enrich.FromLogContext());

    // ── Application Layer Services ────────────────────────────────────────────
    builder.Services.AddApplicationServices();

    // ── Infrastructure Layer Services ─────────────────────────────────────────
    // Registers: DbContext, Identity, JWT auth, IGenericRepository<>, IUnitOfWork,
    //            IIdentityService, IJwtService, IRefreshTokenService
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // ── Current User Service ──────────────────────────────────────────────────
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    builder.Services.AddScoped<IBackgroundJob, HangfireBackgroundJob>();
    builder.Services.AddScoped<INotificationService, EmailNotificationService>();

    // ── ASP.NET Core ──────────────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    
    // ── Health Checks ─────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);

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
    builder.Services.AddHangfire(config => config
           .UseSimpleAssemblyNameTypeSerializer()
           .UseRecommendedSerializerSettings()
           .UseSqlServerStorage(
               builder.Configuration.GetConnectionString("HangfireConnection")));

    builder.Services.AddHangfireServer();
    var app = builder.Build();

    // ── Seed Roles ────────────────────────────────────────────────────────────
    await JobApplicationManagement.Infrastructure.DependencyInjection.SeedRolesAsync(app.Services);

    // ── Global Exception Handling ─────────────────────────────────────────────
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // ── Serilog Request Logging ───────────────────────────────────────────────
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                diagnosticContext.Set("UserId", userId);
            }

            var userEmail = httpContext.User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(userEmail))
            {
                diagnosticContext.Set("UserEmail", userEmail);
            }

            var roles = httpContext.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
            if (roles.Any())
            {
                diagnosticContext.Set("UserRole", string.Join(",", roles));
            }

            diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // ── Authentication must come before Authorization ─────────────────────────
    app.UseAuthentication();
    app.UseAuthorization();

    // ── Push user details to Serilog LogContext for business logs ─────────────
    app.UseMiddleware<RequestContextLoggingMiddleware>();

    // ── Hangfire Dashboard & Recurring Jobs ───────────────────────────────────
    app.UseHangfireDashboard();

    RecurringJob.AddOrUpdate<IStaleApplicationCleanupJob>(
        "auto-close-stale-applications",
        job => job.CloseStaleApplicationsAsync(CancellationToken.None),
        Cron.Daily());

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
