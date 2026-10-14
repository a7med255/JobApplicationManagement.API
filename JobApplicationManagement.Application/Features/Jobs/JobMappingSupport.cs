using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Jobs;

public static class JobDtoFactory
{
    public static JobSummaryDto ToSummary(Job job)
    {
        var salaryVisible = job.IsSalaryVisible;
        return new JobSummaryDto
        {
            Id = job.Id,
            Title = job.Title,
            Company = ToCompany(job.Company),
            Department = job.Department,
            Category = job.Category,
            Country = job.Country,
            City = job.City,
            WorkplaceType = job.WorkplaceType,
            EmploymentType = job.EmploymentType,
            ExperienceLevel = job.ExperienceLevel,
            MinimumYearsExperience = job.MinimumYearsExperience,
            MaximumYearsExperience = job.MaximumYearsExperience,
            MinimumSalary = salaryVisible ? job.MinimumSalary : null,
            MaximumSalary = salaryVisible ? job.MaximumSalary : null,
            Currency = salaryVisible ? job.Currency : null,
            SalaryPeriod = salaryVisible ? job.SalaryPeriod : null,
            IsSalaryVisible = salaryVisible,
            NumberOfVacancies = job.NumberOfVacancies,
            PostedAt = job.PostedAt,
            ApplicationDeadline = job.ApplicationDeadline,
            Status = job.Status,
            IsActive = job.IsActive,
            RequiredSkills = job.JobSkills
                .Where(js => js.IsRequired)
                .Select(js => js.Skill.Name)
                .OrderBy(name => name)
                .ToArray()
        };
    }

    public static JobResponseDto ToResponse(Job job, bool isSaved = false, int? applicationId = null)
    {
        var summary = ToSummary(job);
        return new JobResponseDto
        {
            Id = summary.Id,
            Title = summary.Title,
            Company = summary.Company,
            Department = summary.Department,
            Category = summary.Category,
            Country = summary.Country,
            City = summary.City,
            WorkplaceType = summary.WorkplaceType,
            EmploymentType = summary.EmploymentType,
            ExperienceLevel = summary.ExperienceLevel,
            MinimumYearsExperience = summary.MinimumYearsExperience,
            MaximumYearsExperience = summary.MaximumYearsExperience,
            MinimumSalary = summary.MinimumSalary,
            MaximumSalary = summary.MaximumSalary,
            Currency = summary.Currency,
            SalaryPeriod = summary.SalaryPeriod,
            IsSalaryVisible = summary.IsSalaryVisible,
            NumberOfVacancies = summary.NumberOfVacancies,
            PostedAt = summary.PostedAt,
            ApplicationDeadline = summary.ApplicationDeadline,
            Status = summary.Status,
            IsActive = summary.IsActive,
            RequiredSkills = summary.RequiredSkills,
            Description = job.Description,
            Location = job.Location,
            Responsibilities = job.Responsibilities,
            Requirements = job.Requirements,
            PreferredQualifications = job.PreferredQualifications,
            EducationRequirements = job.EducationRequirements,
            Benefits = job.Benefits,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            ClosedAt = job.ClosedAt,
            PreferredSkills = job.JobSkills
                .Where(js => !js.IsRequired)
                .Select(js => js.Skill.Name)
                .OrderBy(name => name)
                .ToArray(),
            IsSaved = isSaved,
            HasApplied = applicationId.HasValue,
            ApplicationId = applicationId
        };
    }

    private static CompanySummaryDto ToCompany(Company? company) => new()
    {
        Id = company?.Id ?? 0,
        Name = company?.Name ?? string.Empty,
        LogoUrl = company?.LogoUrl,
        Description = company?.Description,
        Website = company?.Website,
        Industry = company?.Industry,
        CompanySize = company?.CompanySize,
        Headquarters = company?.Headquarters
    };
}

public static class JobWriteMapper
{
    public static void Apply(CreateJobDto dto, Job job) => ApplyValues(
        job, dto.Title, dto.Description, dto.Department, dto.Category, dto.Country, dto.City,
        dto.Location, dto.WorkplaceType, dto.EmploymentType, dto.ExperienceLevel,
        dto.MinimumYearsExperience, dto.MaximumYearsExperience, dto.Responsibilities,
        dto.Requirements, dto.PreferredQualifications, dto.EducationRequirements, dto.Benefits,
        dto.MinimumSalary, dto.MaximumSalary, dto.Currency, dto.SalaryPeriod, dto.IsSalaryVisible,
        dto.NumberOfVacancies, dto.ApplicationDeadline);

    public static void Apply(UpdateJobDto dto, Job job) => ApplyValues(
        job, dto.Title, dto.Description, dto.Department, dto.Category, dto.Country, dto.City,
        dto.Location, dto.WorkplaceType, dto.EmploymentType, dto.ExperienceLevel,
        dto.MinimumYearsExperience, dto.MaximumYearsExperience, dto.Responsibilities,
        dto.Requirements, dto.PreferredQualifications, dto.EducationRequirements, dto.Benefits,
        dto.MinimumSalary, dto.MaximumSalary, dto.Currency, dto.SalaryPeriod, dto.IsSalaryVisible,
        dto.NumberOfVacancies, dto.ApplicationDeadline);

    private static void ApplyValues(
        Job job,
        string title,
        string description,
        string department,
        string category,
        string country,
        string city,
        string? location,
        WorkplaceType workplaceType,
        EmploymentType employmentType,
        ExperienceLevel experienceLevel,
        int minimumYearsExperience,
        int? maximumYearsExperience,
        string responsibilities,
        string requirements,
        string? preferredQualifications,
        string? educationRequirements,
        string? benefits,
        decimal? minimumSalary,
        decimal? maximumSalary,
        string? currency,
        SalaryPeriod? salaryPeriod,
        bool isSalaryVisible,
        int numberOfVacancies,
        DateTime? applicationDeadline)
    {
        job.Title = title.Trim();
        job.Description = description.Trim();
        job.Department = department.Trim();
        job.Category = category.Trim();
        job.Country = country.Trim();
        job.City = city.Trim();
        job.Location = NormalizeOptional(location);
        job.WorkplaceType = workplaceType;
        job.EmploymentType = employmentType;
        job.ExperienceLevel = experienceLevel;
        job.MinimumYearsExperience = minimumYearsExperience;
        job.MaximumYearsExperience = maximumYearsExperience;
        job.Responsibilities = responsibilities.Trim();
        job.Requirements = requirements.Trim();
        job.PreferredQualifications = NormalizeOptional(preferredQualifications);
        job.EducationRequirements = NormalizeOptional(educationRequirements);
        job.Benefits = NormalizeOptional(benefits);
        job.MinimumSalary = minimumSalary;
        job.MaximumSalary = maximumSalary;
        job.Currency = currency?.Trim().ToUpperInvariant();
        job.SalaryPeriod = salaryPeriod;
        job.IsSalaryVisible = isSalaryVisible;
        job.NumberOfVacancies = numberOfVacancies;
        job.ApplicationDeadline = applicationDeadline?.ToUniversalTime();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public static class JobSkillSynchronizer
{
    public static async Task ReplaceAsync(
        IUnitOfWork unitOfWork,
        Job job,
        IEnumerable<string> requiredSkills,
        IEnumerable<string> preferredSkills,
        CancellationToken cancellationToken)
    {
        var requested = new Dictionary<string, (string Name, bool IsRequired)>(StringComparer.OrdinalIgnoreCase);
        AddSkills(requested, preferredSkills, false);
        AddSkills(requested, requiredSkills, true);

        var normalizedNames = requested.Keys.ToArray();
        var existingSkills = await unitOfWork.Skills.Query()
            .Where(skill => normalizedNames.Contains(skill.NormalizedName))
            .ToListAsync(cancellationToken);

        foreach (var existingLink in job.JobSkills.ToArray())
        {
            unitOfWork.JobSkills.Delete(existingLink);
        }
        job.JobSkills.Clear();

        foreach (var requestedSkill in requested.Values)
        {
            var normalizedName = Normalize(requestedSkill.Name);
            var skill = existingSkills.FirstOrDefault(s => s.NormalizedName == normalizedName);
            if (skill is null)
            {
                skill = new Skill { Name = requestedSkill.Name.Trim(), NormalizedName = normalizedName };
                await unitOfWork.Skills.AddAsync(skill);
                existingSkills.Add(skill);
            }

            var link = new JobSkill { Job = job, Skill = skill, IsRequired = requestedSkill.IsRequired };
            job.JobSkills.Add(link);
            if (job.Id > 0)
            {
                await unitOfWork.JobSkills.AddAsync(link);
            }
        }
    }

    private static void AddSkills(
        IDictionary<string, (string Name, bool IsRequired)> target,
        IEnumerable<string> skills,
        bool isRequired)
    {
        foreach (var value in skills.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            var name = value.Trim();
            var normalizedName = Normalize(name);
            if (!target.TryGetValue(normalizedName, out var existing) || isRequired && !existing.IsRequired)
            {
                target[normalizedName] = (name, isRequired);
            }
        }
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
}
