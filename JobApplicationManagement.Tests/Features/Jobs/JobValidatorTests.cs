using System;
using System.Linq;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Application.Features.Jobs.Validators;
using JobApplicationManagement.Domain.Enums;
using Xunit;

namespace JobApplicationManagement.Tests.Features.Jobs;

public class JobValidatorTests
{
    private readonly CreateJobValidator _createValidator = new();
    private readonly UpdateJobValidator _updateValidator = new();

    [Fact]
    public void Complete_Job_Is_Valid()
    {
        var result = _createValidator.Validate(ValidJob());

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

        var result = _createValidator.Validate(job);

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

        var result = _createValidator.Validate(job);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(job.MaximumSalary));
    }

    [Fact]
    public void Negative_Salary_Is_Rejected()
    {
        var jobMinNegative = ValidJob();
        jobMinNegative.MinimumSalary = -100;
        jobMinNegative.Currency = "USD";
        jobMinNegative.SalaryPeriod = SalaryPeriod.Monthly;

        var resultMin = _createValidator.Validate(jobMinNegative);
        Assert.Contains(resultMin.Errors, error => error.PropertyName == nameof(jobMinNegative.MinimumSalary));

        var jobMaxNegative = ValidJob();
        jobMaxNegative.MinimumSalary = null;
        jobMaxNegative.MaximumSalary = -500;
        jobMaxNegative.Currency = "USD";
        jobMaxNegative.SalaryPeriod = SalaryPeriod.Monthly;

        var resultMax = _createValidator.Validate(jobMaxNegative);
        Assert.Contains(resultMax.Errors, error => error.PropertyName == nameof(jobMaxNegative.MaximumSalary));
    }

    [Fact]
    public void Experience_Bounds_Are_Enforced()
    {
        var jobNegativeExp = ValidJob();
        jobNegativeExp.MinimumYearsExperience = -1;
        var resultNegative = _createValidator.Validate(jobNegativeExp);
        Assert.Contains(resultNegative.Errors, error => error.PropertyName == nameof(jobNegativeExp.MinimumYearsExperience));

        var jobHighExp = ValidJob();
        jobHighExp.MinimumYearsExperience = 85;
        var resultHigh = _createValidator.Validate(jobHighExp);
        Assert.Contains(resultHigh.Errors, error => error.PropertyName == nameof(jobHighExp.MinimumYearsExperience));

        var jobMaxLessThanMin = ValidJob();
        jobMaxLessThanMin.MinimumYearsExperience = 5;
        jobMaxLessThanMin.MaximumYearsExperience = 3;
        var resultMaxLess = _createValidator.Validate(jobMaxLessThanMin);
        Assert.Contains(resultMaxLess.Errors, error => error.PropertyName == nameof(jobMaxLessThanMin.MaximumYearsExperience));

        var jobMaxTooHigh = ValidJob();
        jobMaxTooHigh.MinimumYearsExperience = 5;
        jobMaxTooHigh.MaximumYearsExperience = 90;
        var resultMaxHigh = _createValidator.Validate(jobMaxTooHigh);
        Assert.Contains(resultMaxHigh.Errors, error => error.PropertyName == nameof(jobMaxTooHigh.MaximumYearsExperience));
    }

    [Fact]
    public void Number_Of_Vacancies_Must_Be_Between_1_And_1000()
    {
        var jobZeroVacancies = ValidJob();
        jobZeroVacancies.NumberOfVacancies = 0;
        var resultZero = _createValidator.Validate(jobZeroVacancies);
        Assert.Contains(resultZero.Errors, error => error.PropertyName == nameof(jobZeroVacancies.NumberOfVacancies));

        var jobTooManyVacancies = ValidJob();
        jobTooManyVacancies.NumberOfVacancies = 1001;
        var resultTooMany = _createValidator.Validate(jobTooManyVacancies);
        Assert.Contains(resultTooMany.Errors, error => error.PropertyName == nameof(jobTooManyVacancies.NumberOfVacancies));
    }

    [Fact]
    public void Salary_Visibility_Cannot_Be_True_When_No_Salary_Is_Provided()
    {
        var job = ValidJob();
        job.MinimumSalary = null;
        job.MaximumSalary = null;
        job.IsSalaryVisible = true;

        var result = _createValidator.Validate(job);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(job.IsSalaryVisible));
    }

    [Fact]
    public void Draft_May_Be_Saved_With_Past_Deadline_But_Cannot_Be_Published()
    {
        var job = ValidJob();
        job.PublishImmediately = false;
        job.ApplicationDeadline = DateTime.UtcNow.AddDays(-1);

        var result = _createValidator.Validate(job);
        Assert.True(result.IsValid);

        job.PublishImmediately = true;
        var publishResult = _createValidator.Validate(job);
        Assert.Contains(publishResult.Errors, error => error.PropertyName == nameof(job.ApplicationDeadline));
    }

    [Fact]
    public void More_Than_50_Skills_Is_Rejected()
    {
        var job = ValidJob();
        job.RequiredSkills = Enumerable.Range(1, 51).Select(i => $"Skill{i}").ToList();

        var result = _createValidator.Validate(job);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(job.RequiredSkills));
    }

    [Fact]
    public void Required_Fields_Cannot_Be_Empty()
    {
        var job = new CreateJobDto();
        var result = _createValidator.Validate(job);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(job.Title));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(job.Description));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(job.Department));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(job.Category));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(job.Country));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(job.City));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(job.Responsibilities));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(job.Requirements));
    }

    // ── UpdateJobValidator Tests ───────────────────────────────────────────────

    [Fact]
    public void UpdateJob_Valid_Dto_Passes()
    {
        var updateDto = ValidUpdateJob();
        var result = _updateValidator.Validate(updateDto);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateJob_Negative_Salary_Fails()
    {
        var updateDto = ValidUpdateJob();
        updateDto.MaximumSalary = -200;
        updateDto.Currency = "EUR";
        updateDto.SalaryPeriod = SalaryPeriod.Monthly;

        var result = _updateValidator.Validate(updateDto);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(updateDto.MaximumSalary));
    }

    [Fact]
    public void UpdateJob_Past_Deadline_Fails()
    {
        var updateDto = ValidUpdateJob();
        updateDto.ApplicationDeadline = DateTime.UtcNow.AddMinutes(-5);

        var result = _updateValidator.Validate(updateDto);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(updateDto.ApplicationDeadline));
    }

    [Fact]
    public void UpdateJob_Invalid_Experience_Fails()
    {
        var updateDto = ValidUpdateJob();
        updateDto.MinimumYearsExperience = 8;
        updateDto.MaximumYearsExperience = 2;

        var result = _updateValidator.Validate(updateDto);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(updateDto.MaximumYearsExperience));
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

    private static UpdateJobDto ValidUpdateJob() => new()
    {
        Title = "Lead .NET Architect",
        Description = "Architect high-throughput scalable microservices.",
        Department = "Architecture",
        Category = "Engineering",
        Country = "Egypt",
        City = "Cairo",
        WorkplaceType = WorkplaceType.Remote,
        EmploymentType = EmploymentType.FullTime,
        ExperienceLevel = ExperienceLevel.Lead,
        MinimumYearsExperience = 8,
        MaximumYearsExperience = 12,
        Responsibilities = "Lead backend architecture and security posture.",
        Requirements = "Expert in .NET 8, EF Core, and distributed systems.",
        NumberOfVacancies = 1,
        ApplicationDeadline = DateTime.UtcNow.AddDays(45),
        RequiredSkills = [".NET", "C#", "SQL Server"]
    };
}
