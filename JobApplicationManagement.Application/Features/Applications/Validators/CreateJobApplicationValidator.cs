using FluentValidation;
using JobApplicationManagement.Application.Features.Applications.DTOs;

namespace JobApplicationManagement.Application.Features.Applications.Validators;

public class CreateJobApplicationValidator : AbstractValidator<CreateJobApplicationDto>
{
    public CreateJobApplicationValidator()
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0).WithMessage("JobId is required and must be greater than 0.");
    }
}
