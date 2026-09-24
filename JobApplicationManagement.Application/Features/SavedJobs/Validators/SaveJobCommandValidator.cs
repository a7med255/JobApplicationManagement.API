using FluentValidation;
using JobApplicationManagement.Application.Features.SavedJobs.Commands;

namespace JobApplicationManagement.Application.Features.SavedJobs.Validators;

/// <summary>
/// Validates the SaveJobCommand input.
/// Business rules (job existence, duplicate check, ownership) are enforced in the handler.
/// </summary>
public class SaveJobCommandValidator : AbstractValidator<SaveJobCommand>
{
    public SaveJobCommandValidator()
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0).WithMessage("JobId is required and must be greater than 0.");
    }
}
