using FluentValidation;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Application.Features.Jobs.Commands;

namespace JobApplicationManagement.Application.Features.Jobs.Validators;

public class UpdateJobValidator : AbstractValidator<UpdateJobDto>
{
    public UpdateJobValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(20000);
        RuleFor(x => x.Department).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Location).MaximumLength(300);
        RuleFor(x => x.WorkplaceType).IsInEnum();
        RuleFor(x => x.EmploymentType).IsInEnum();
        RuleFor(x => x.ExperienceLevel).IsInEnum();
        RuleFor(x => x.MinimumYearsExperience).InclusiveBetween(0, 80);
        RuleFor(x => x.MaximumYearsExperience)
            .InclusiveBetween(0, 80)
            .GreaterThanOrEqualTo(x => x.MinimumYearsExperience)
            .When(x => x.MaximumYearsExperience.HasValue);
        RuleFor(x => x.Responsibilities).NotEmpty().MaximumLength(10000);
        RuleFor(x => x.Requirements).NotEmpty().MaximumLength(10000);
        RuleFor(x => x.PreferredQualifications).MaximumLength(10000);
        RuleFor(x => x.EducationRequirements).MaximumLength(5000);
        RuleFor(x => x.Benefits).MaximumLength(10000);
        RuleFor(x => x.MinimumSalary).GreaterThanOrEqualTo(0).When(x => x.MinimumSalary.HasValue);
        RuleFor(x => x.MaximumSalary).GreaterThanOrEqualTo(0).When(x => x.MaximumSalary.HasValue);
        RuleFor(x => x.MaximumSalary)
            .GreaterThanOrEqualTo(x => x.MinimumSalary!.Value)
            .When(x => x.MinimumSalary.HasValue && x.MaximumSalary.HasValue);
        RuleFor(x => x.Currency).NotEmpty().Length(3).When(HasSalary);
        RuleFor(x => x.SalaryPeriod).NotNull().IsInEnum().When(HasSalary);
        RuleFor(x => x.IsSalaryVisible)
            .Equal(false)
            .When(x => !HasSalary(x))
            .WithMessage("Salary cannot be visible when no salary range is supplied.");
        RuleFor(x => x.NumberOfVacancies).InclusiveBetween(1, 1000);
        RuleFor(x => x.ApplicationDeadline)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ApplicationDeadline.HasValue)
            .WithMessage("The application deadline must be in the future.");
        RuleFor(x => x.RequiredSkills).Must(skills => skills.Count <= 50);
        RuleFor(x => x.PreferredSkills).Must(skills => skills.Count <= 50);
        RuleForEach(x => x.RequiredSkills).NotEmpty().MaximumLength(100);
        RuleForEach(x => x.PreferredSkills).NotEmpty().MaximumLength(100);
    }

    private static bool HasSalary(UpdateJobDto dto) =>
        dto.MinimumSalary.HasValue || dto.MaximumSalary.HasValue;
}

public class UpdateJobCommandValidator : AbstractValidator<UpdateJobCommand>
{
    public UpdateJobCommandValidator() => RuleFor(command => command.JobDto).SetValidator(new UpdateJobValidator());
}
