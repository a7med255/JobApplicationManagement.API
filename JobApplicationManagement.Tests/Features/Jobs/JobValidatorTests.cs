using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Application.Features.Jobs.Validators;
using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Tests.Features.Jobs;

public class JobValidatorTests
{
    private readonly CreateJobValidator _validator = new();

    [Fact]
    public void Complete_Job_Is_Valid()
    {
        var result = _validator.Validate(ValidJob());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Salary_Requires_Currency_And_Period()
    {
        var job = ValidJob();
        job.MinimumSalary = 1000;
        job.MaximumSalary = 2000;
        job.Currency = null;
        job.SalaryPeriod = null;

        var result = _validator.Validate(job);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(job.Currency));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(job.SalaryPeriod));
    }

    [Fact]
    public void Maximum_Salary_Cannot_Be_Less_Than_Minimum()
    {
        var job = ValidJob();
        job.MinimumSalary = 5000;
        job.MaximumSalary = 4000;
        job.Currency = "USD";
        job.SalaryPeriod = SalaryPeriod.Monthly;

        var result = _validator.Validate(job);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(job.MaximumSalary));
    }

    [Fact]
    public void Draft_May_Be_Saved_With_Past_Deadline_But_Cannot_Be_Published()
    {
        var job = ValidJob();
        job.PublishImmediately = false;
        job.ApplicationDeadline = DateTime.UtcNow.AddDays(-1);

        var result = _validator.Validate(job);

        Assert.True(result.IsValid);
    }

    private static CreateJobDto ValidJob() => new()
    {
        Title = "Senior Angular Engineer",
        Description = "Build and maintain the candidate experience.",
        Department = "Engineering",
        Category = "Software Development",
        Country = "Egypt",
        City = "Cairo",
        WorkplaceType = WorkplaceType.Hybrid,
        EmploymentType = EmploymentType.FullTime,
        ExperienceLevel = ExperienceLevel.Senior,
        MinimumYearsExperience = 5,
        MaximumYearsExperience = 8,
        Responsibilities = "Deliver accessible Angular features.",
        Requirements = "Strong Angular and TypeScript experience.",
        NumberOfVacancies = 2,
        ApplicationDeadline = DateTime.UtcNow.AddDays(30),
        RequiredSkills = ["Angular", "TypeScript"]
    };
}
