using JobApplicationManagement.API.Middleware;
using JobApplicationManagement.API.Services;
using JobApplicationManagement.Application;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Infrastructure;
using Microsoft.OpenApi.Models;
using Serilog;

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

    // ── ASP.NET Core ──────────────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

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
    });

    var app = builder.Build();

    // ── Seed Roles ────────────────────────────────────────────────────────────
    await JobApplicationManagement.Infrastructure.DependencyInjection.SeedRolesAsync(app.Services);

    // ── Global Exception Handling ─────────────────────────────────────────────
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // ── Serilog Request Logging ───────────────────────────────────────────────
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // ── Authentication must come before Authorization ─────────────────────────
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

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
