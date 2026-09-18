using FluentValidation;
using JobApplicationManagement.Application.Features.Jobs.DTOs;

namespace JobApplicationManagement.Application.Features.Jobs.Validators;

/// <summary>
/// FluentValidation validator for <see cref="CreateJobDto"/>.
///
/// Validation limits rationale:
///   - Title:       required, max 200 chars — industry standard for job titles
///                  (LinkedIn, Indeed both cap around 200). A title longer than this
///                  is almost certainly a data entry error.
///   - Description: required, max 5000 chars — sufficient for a detailed job posting
///                  with responsibilities, requirements, and benefits sections.
///                  Prevents unbounded input reaching the database column.
/// </summary>
public class CreateJobValidator : AbstractValidator<CreateJobDto>
{
    public CreateJobValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
                .WithMessage("Job title is required.")
            .MaximumLength(200)
                .WithMessage("Job title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
                .WithMessage("Job description is required.")
            .MaximumLength(5000)
                .WithMessage("Job description must not exceed 5000 characters.");
    }
}
