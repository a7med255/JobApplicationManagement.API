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
        RuleFor(x => x.CvUrl).MaximumLength(2000);
        RuleFor(x => x.ProfessionalTitle).MaximumLength(200);
        RuleFor(x => x.Summary).MaximumLength(2000);
        RuleFor(x => x.Country).MaximumLength(100);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 70).When(x => x.YearsOfExperience.HasValue);
    }
}
