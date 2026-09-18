using FluentValidation;
using JobApplicationManagement.Application.Features.Jobs.DTOs;

namespace JobApplicationManagement.Application.Features.Jobs.Validators;

/// <summary>
/// FluentValidation validator for UpdateJobDto.
/// Same rules as CreateJobValidator — Title and Description are required with max lengths.
/// </summary>
public class UpdateJobValidator : AbstractValidator<UpdateJobDto>
{
    public UpdateJobValidator()
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
