using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using JobApplicationManagement.Application.Features.Identity.Interfaces;
using JobApplicationManagement.Application.Features.Identity.Services;
using JobApplicationManagement.Application.Features.Identity.Validators;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Application.Features.Jobs.Mappings;
using JobApplicationManagement.Application.Features.Jobs.Validators;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Interviews;
using JobApplicationManagement.Application.Features.JobAlerts;
using JobApplicationManagement.Application.Features.Admin;

namespace JobApplicationManagement.Application;

/// <summary>
/// Extension method to register all Application layer services.
/// Called from API/Program.cs — keeps Program.cs clean.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper — scans this assembly for Profile classes
        services.AddAutoMapper(cfg => { }, typeof(JobMappingProfile).Assembly);

        // FluentValidation — registers all validators in this assembly
        services.AddValidatorsFromAssemblyContaining<CreateJobValidator>();

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(JobApplicationManagement.Application.Common.Behaviors.LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(JobApplicationManagement.Application.Common.Behaviors.ValidationBehavior<,>));
        });

        // Identity feature services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInterviewService, InterviewService>();
        services.AddScoped<IJobAlertService, JobAlertService>();
        services.AddScoped<IAdminOperationsService, AdminOperationsService>();

        // Background Job services
        services.AddScoped<JobApplicationManagement.Application.Common.Interfaces.IStaleApplicationCleanupJob, JobApplicationManagement.Application.Features.Applications.Services.StaleApplicationCleanupJob>();
        services.AddScoped<IJobExpirationService, JobApplicationManagement.Application.Features.Jobs.Services.JobExpirationService>();

        return services;
    }
}
