using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using JobApplicationManagement.Application.Features.Applications.Interfaces;
using JobApplicationManagement.Application.Features.Applications.Services;
using JobApplicationManagement.Application.Features.Identity.Interfaces;
using JobApplicationManagement.Application.Features.Identity.Services;
using JobApplicationManagement.Application.Features.Identity.Validators;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Application.Features.Jobs.Interfaces;
using JobApplicationManagement.Application.Features.Jobs.Mappings;
using JobApplicationManagement.Application.Features.Jobs.Services;
using JobApplicationManagement.Application.Features.Jobs.Validators;

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
        services.AddAutoMapper(typeof(JobMappingProfile).Assembly);

        // FluentValidation — registers all validators in this assembly
        services.AddValidatorsFromAssemblyContaining<CreateJobValidator>();

        // Job feature services
        services.AddScoped<IJobService, JobService>();

        // Application (job application) feature services
        services.AddScoped<IApplicationService, ApplicationService>();

        // Candidate feature services
        services.AddScoped<JobApplicationManagement.Application.Features.Candidates.Interfaces.ICandidateService, JobApplicationManagement.Application.Features.Candidates.Services.CandidateService>();

        // Recruiter feature services
        services.AddScoped<JobApplicationManagement.Application.Features.Recruiters.Interfaces.IRecruiterService, JobApplicationManagement.Application.Features.Recruiters.Services.RecruiterService>();

        // Identity feature services
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
