using FluentValidation;
using JobApplicationManagement.Application.Features.Recruiters.DTOs;

namespace JobApplicationManagement.Application.Features.Recruiters.Validators;

public class UpdateRecruiterValidator : AbstractValidator<UpdateRecruiterDto>
{
    public UpdateRecruiterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");
    }
}
