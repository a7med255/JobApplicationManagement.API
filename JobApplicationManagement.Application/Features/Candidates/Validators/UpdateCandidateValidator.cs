using FluentValidation;
using JobApplicationManagement.Application.Features.Candidates.DTOs;

namespace JobApplicationManagement.Application.Features.Candidates.Validators;

public class UpdateCandidateValidator : AbstractValidator<UpdateCandidateDto>
{
    public UpdateCandidateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");
    }
}
