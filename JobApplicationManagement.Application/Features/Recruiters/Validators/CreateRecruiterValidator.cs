using FluentValidation;
using JobApplicationManagement.Application.Features.Recruiters.DTOs;

namespace JobApplicationManagement.Application.Features.Recruiters.Validators;

public class CreateRecruiterValidator : AbstractValidator<CreateRecruiterDto>
{
    public CreateRecruiterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
