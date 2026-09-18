using FluentValidation;
using JobApplicationManagement.Application.Features.Applications.DTOs;

namespace JobApplicationManagement.Application.Features.Applications.Validators;

public class UpdateJobApplicationValidator : AbstractValidator<UpdateJobApplicationDto>
{
    public UpdateJobApplicationValidator()
    {
        RuleFor(x => x.JobApplicationStatus)
            .IsInEnum().WithMessage("A valid JobApplicationStatus is required.");
    }
}
